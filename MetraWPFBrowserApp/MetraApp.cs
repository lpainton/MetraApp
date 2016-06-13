using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Metra.Axxess;
using System.IO;
using Microsoft.Win32;
using System.Windows;

namespace MetraWPFBrowserApp
{
    public enum AppState
    {
        Loading,
        Setup,
        NoDevice,
        DeviceConnected,
        Streaming
    };

    class MetraApp
    {
        public AppState State { get; private set; }
        AppUpdateTimer UpdateTimer { get; set; }
        FileManager FManager { get; set; }
        //IAxxessBoard AttachedDevice { get { return _device; } set { _device = value; } }
        //IAxxessBoard _device;
        IOperation ActiveOperation { get; set; }

        MainWindow MainWin { get; set; }
        BoardInfoLogic BInfoLogic { get; set; }
        MainLabelLogic MainLblLogic { get; set; }
        ConnectButtonLogic ConnBtnLogic { get; set; }
        UpdateButtonLogic UBtnLogic { get; set; }
        LoadButtonLogic LBtnLogic { get; set; }
        AboutButtonLogic ABtnLogic { get; set; }
        ConfigureButtonLogic ConfBtnLogic { get; set; }
        ProgressBarLogic ProgBarLogic { get; set; }

        public MetraApp(MainWindow mainWin)
        {
            this.MainWin = mainWin;
            this.BInfoLogic = new BoardInfoLogic(mainWin.BoardInfoBlock);
            this.MainLblLogic = new MainLabelLogic(mainWin.MainLabel);
            this.ConnBtnLogic = new ConnectButtonLogic(mainWin.ConnectButton);
            this.UBtnLogic = new UpdateButtonLogic(mainWin.UpdateFirmwareButton);
            this.LBtnLogic = new LoadButtonLogic(mainWin.LoadFirmwareButton);
            this.ABtnLogic = new AboutButtonLogic(mainWin.AboutButton);
            this.ConfBtnLogic = new ConfigureButtonLogic(mainWin.ConfigureButtonsButton);
            this.ProgBarLogic = new ProgressBarLogic(mainWin.MainProgressBar);

            //this.AttachedDevice = null;
            this.ActiveOperation = null;
        }

        #region Startup
        public void Initialize()
        {
            //Start in the setup phase
            this.ChangeAppState(AppState.Loading);

            this.FManager = new FileManager();
            FManager.CreateDirectory(FManager.AppDataFolder);

            this.UpdateTimer = new AppUpdateTimer(Update);
            this.UpdateTimer.Enable();

            LogManager.Initialize(LoggingMode.Active, FManager);
            Log.Initialize(LogManager.Log);
            BoardManager.Initialize();

            this.Setup();
        }
        public void Setup()
        {
            this.ChangeAppState(AppState.Setup);

            FManager.CreateDirectory(FManager.FirmwareFolder);
            FManager.CreateDirectory(FManager.MapFolder);

            this.ProgBarLogic.SetVal(10);

            //Validate firmware archive
            if (!File.Exists(FManager.FirmwareArchive))
            {
                SetupForm s = new SetupForm(FManager);
                s.ShowDialog();
            }
            this.ProgBarLogic.SetVal(20);

            this.ChangeAppState(AppState.NoDevice);
        }
        #endregion

        //Event fired by timer object
        private void Update(object s, System.Timers.ElapsedEventArgs e)
        {
            if (!(this.ActiveOperation == null) && !this.ActiveOperation.Message.Equals(String.Empty))
                this.MainLblLogic.ChangeText(this.ActiveOperation.Message);
            else
                this.MainLblLogic.Update(this.State);

            BInfoLogic.Update(BoardManager.ConnectedBoard);
            ProgBarLogic.Update(this.ActiveOperation);
        }

        public void ResetToIdle()
        {
            if (this.ActiveOperation != null)
            {
                this.ActiveOperation.Stop();
                this.ActiveOperation = null;
            }

            if (BoardManager.ConnectedBoard != null)
            {
                BoardManager.SetConnectedBoard(null);
            }

            this.ChangeAppState(AppState.NoDevice);
        }

        public void ChangeAppState(AppState newState)
        {
            this.State = newState;

            ConnBtnLogic.Update(this.State);
            UBtnLogic.Update(this.State);
            LBtnLogic.Update(this.State);
            ConfBtnLogic.Update(this.State);
            ABtnLogic.Update(this.State);
        }

        #region Events
        public void OnConnectButtonClick()
        {
            ConnBtnLogic.OnClick();
            if (BoardManager.ConnectedBoard != null)
                this.OnDeviceConnect();
        }
        public void OnUpdateButtonClick()
        {
            this.ChangeAppState(AppState.Streaming);
            if (!File.Exists(FManager.ManifestFile) && FManager.IsInternetAvailable())
            {
                FManager.DownloadManifest(FManager.ManifestURL);
            }

            while (FManager.Web.IsBusy)
            {
                Thread.Sleep(200);
            }

            AxxessFirmwareToken fileToken = FManager.SearchManifest(BoardManager.ConnectedBoard.ProductID);

            if (fileToken.IsNull)
            {
                MessageBox.Show("Could not determine a firmware file for the connected device!");
                this.ChangeAppState(AppState.DeviceConnected);
            }
            else
            {
                //Version checking here
                AxxessFirmwareVersion currVer = new AxxessFirmwareVersion(BoardManager.ConnectedBoard.AppFirmwareVersion);
                if (currVer.CompareTo(fileToken.FileVersion) >= 0)
                {
                    MessageBoxResult res = MessageBox.Show("The firmware version on the device is up-to-date.  Should we force the update anyway?",
                        "Force Update?", MessageBoxButton.YesNo);

                    if (!res.Equals(MessageBoxResult.Yes)) return;
                }

                string filePath = String.Empty;
                try 
                {
                    filePath = FManager.GetPathToFirmware(fileToken.FileName);
                } 
                catch (FileNotFoundException)
                {
                    LogManager.WriteToLog(String.Format("Batch did not contain firmware {0} for {1}.  Downloading directly...", fileToken.FileName, fileToken.BoardID));
                    try 
                    {
                        FManager.DownloadFirmwareDirectly(fileToken);
                        filePath = FManager.GetPathToFirmware(fileToken.FileName);
                    }
                    catch (FileNotFoundException)
                    {
                        MessageBox.Show("Error: Could not download firmware for this device.  Please disconnect the device and try again.");
                        LogManager.WriteToLog(String.Format("Could not download firmware {0} for {1}!", fileToken.FileName, fileToken.BoardID));
                        ResetToIdle();
                        return;
                    }
                }
                if (filePath.Equals(String.Empty))
                {
                    MessageBox.Show("An unknown error occured.  Please disconnect the device and try again.");
                    LogManager.WriteToLog("File path was not passed correctly to the update method.");
                    ResetToIdle();
                    return;
                }
                UpdateFromFile(filePath, fileToken);
            }
        }
        public void OnLoadButtonClick()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            //fileDialog.Filter = "Hex files (.hex) | *.hex";
            fileDialog.FilterIndex = 1;

            fileDialog.Multiselect = false;

            bool? resultOk = fileDialog.ShowDialog();

            if (resultOk == true)
            {
                this.UpdateFromFile(fileDialog.FileName, new AxxessFirmwareToken(fileDialog.FileName, String.Empty, String.Empty));
            }
        }
        public void OnAboutButtonClick()
        {
            this.ABtnLogic.OnClick(this, new EventArgs());
        }
        public void OnConfigButtonClick()
        {
            //Stop any in progress ops
            if (this.ActiveOperation != null)
            {
                this.ActiveOperation.Stop();
                this.ActiveOperation.WorkerThread.Join();
                this.ActiveOperation = null;
            }

            ASWCWindow ascwin = new ASWCWindow(BoardManager.ConnectedBoard);
            try
            {
                ascwin.ShowDialog();
            }
            catch (NullReferenceException)
            {
                LogManager.WriteToLog("Error: Null reference exception when attempting to display ASWC window.");
            }
            catch (NotImplementedException e)
            {
                LogManager.WriteToLog(e.Message);
            }
            if (BoardManager.IsBoardConnected)
            {
                this.ActiveOperation = OperationFactory.SpawnOperation(OperationType.Boot, new OpArgs(BoardManager.ConnectedBoard));
            }
        }
        public void OnDeviceConnect()
        {
            OpArgs oargs = new OpArgs(BoardManager.ConnectedBoard);
            this.ActiveOperation = OperationFactory.SpawnOperation(OperationType.Boot, oargs);
            this.ActiveOperation.Start();

            this.ChangeAppState(AppState.DeviceConnected);

            BoardManager.OnDeviceRemoved += OnDeviceRemoved;
        }
        public void OnDeviceRemoved(object sender, EventArgs e)
        {
            if (this.ActiveOperation != null)
            {
                ActiveOperation.Stop();
                ActiveOperation = null;
                this.ProgBarLogic.SetVal(0);
            }
            this.ChangeAppState(AppState.NoDevice);
        }
        public void OnClose()
        {
            this.UpdateTimer.Disable();
            LogManager.CloseOut();
        }
        #endregion

        public void Dispose()
        {
            this.UpdateTimer.Dispose();
        }

        private void UpdateFromFile(string path, AxxessFirmwareToken token)
        {
            //Checkpoint
            if (this.ActiveOperation == null || !this.ActiveOperation.Type.Equals(OperationType.Boot))
                return;

            this.ActiveOperation.Stop();
            this.ActiveOperation.WorkerThread.Join();
            this.ActiveOperation = OperationFactory.SpawnOperation(OperationType.Firmware, new OpArgs(BoardManager.ConnectedBoard, path, token));
            this.ActiveOperation.AddCompletedHandler(
                delegate(object sender, OperationEventArgs args)
                {
                    MessageBox.Show("Firmware was successfully updated!");
                    ResetToIdle();
                });
            this.ActiveOperation.Start();

            this.ChangeAppState(AppState.Streaming);
        }
    }
}

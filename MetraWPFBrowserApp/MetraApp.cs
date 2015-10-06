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
        IAxxessBoard AttachedDevice { get { return _device; } set { _device = value; } }
        IAxxessBoard _device;
        IOperation ActiveOperation { get; set; }

        MainWindow MainWin { get; set; }
        BoardInfoLogic BInfoLogic { get; set; }
        MainLabelLogic MainLblLogic { get; set; }
        ConnectButtonLogic ConnBtnLogic { get; set; }
        UpdateButtonLogic UBtnLogic { get; set; }
        LoadButtonLogic LBtnLogic { get; set; }
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
            this.ConfBtnLogic = new ConfigureButtonLogic(mainWin.ConfigureButtonsButton);
            this.ProgBarLogic = new ProgressBarLogic(mainWin.MainProgressBar);

            this.AttachedDevice = null;
            this.ActiveOperation = null;
        }

        public void Initialize()
        {
            //Start in the setup phase
            this.ChangeAppState(AppState.Loading);

            this.FManager = new FileManager();

            this.UpdateTimer = new AppUpdateTimer(Update);
            this.UpdateTimer.Enable();

            this.Setup();
        }

        public void Setup()
        {
            this.ChangeAppState(AppState.Setup);

            FManager.CreateDirectory(FManager.FirmwareFolder);
            this.ProgBarLogic.SetVal(10);

            //Validate firmware archive
            if (!File.Exists(FManager.FirmwareArchive))
            {
                SetupForm s = new SetupForm(FManager);
                s.ShowDialog();
            }
            this.ProgBarLogic.SetVal(20);

            FManager.CreateDirectory(FManager.MapFolder);

            this.ChangeAppState(AppState.NoDevice);
        }

        //Event fired by timer object
        private void Update(object s, System.Timers.ElapsedEventArgs e)
        {
            if (!(this.ActiveOperation == null) && !this.ActiveOperation.Message.Equals(String.Empty))
                this.MainLblLogic.ChangeText(this.ActiveOperation.Message);
            else
                this.MainLblLogic.Update(this.State);

            BInfoLogic.Update(this.AttachedDevice);
            ProgBarLogic.Update(this.ActiveOperation);
        }

        public void ChangeAppState(AppState newState)
        {
            this.State = newState;

            ConnBtnLogic.Update(this.State);
            UBtnLogic.Update(this.State);
            LBtnLogic.Update(this.State);
            ConfBtnLogic.Update(this.State);
        }

        public void OnConnectButtonClick()
        {
            ConnBtnLogic.OnClick(out _device);
            if (_device != null)
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

            AxxessFirmwareToken fileToken = FManager.SearchManifest(this.AttachedDevice.ProductID);

            if (fileToken.IsNull)
            {
                MessageBox.Show("Could not determine a firmware file for the connected device!");
                this.ChangeAppState(AppState.DeviceConnected);
            }
            else
            {
                //Version checking here
                AxxessFirmwareVersion currVer = new AxxessFirmwareVersion(this.AttachedDevice.AppFirmwareVersion);
                if (currVer.CompareTo(fileToken.FileVersion) >= 0)
                {
                    MessageBoxResult res = MessageBox.Show("The firmware version on the device is up-to-date.  Should we force the update anyway?",
                        "Force Update?", MessageBoxButton.YesNo);

                    if (!res.Equals(MessageBoxResult.Yes)) return;
                }

                UpdateFromFile(FManager.GetPathToFirmware(fileToken.FileName), fileToken);
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
                this.UpdateFromFile(fileDialog.FileName, new AxxessFirmwareToken(fileDialog.FileName, String.Empty));
            }
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

            ASCWWindow ascwin = new ASCWWindow(this.AttachedDevice);
            ascwin.ShowDialog();

            this.ActiveOperation = OperationFactory.SpawnOperation(OperationType.Boot, new OpArgs(AttachedDevice));
        }

        public void OnDeviceConnect()
        {
            OpArgs oargs = new OpArgs(this.AttachedDevice);
            this.ActiveOperation = OperationFactory.SpawnOperation(OperationType.Boot, oargs);
            this.ActiveOperation.Start();

            this.ChangeAppState(AppState.DeviceConnected);

            this.AttachedDevice.AddRemovedEvent(OnDeviceRemoved);
        }
        public void OnDeviceRemoved(object sender, EventArgs e)
        {
            this.AttachedDevice.Dispose();
            this.AttachedDevice = null;
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
        }

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
            this.ActiveOperation = OperationFactory.SpawnOperation(OperationType.Firmware, new OpArgs(this.AttachedDevice, path, token));
            this.ActiveOperation.Start();

            this.ChangeAppState(AppState.Streaming);
        }
    }
}

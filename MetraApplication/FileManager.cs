using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.AccessControl;

namespace MetraApplication
{

    #region Exceptions
    public class NoInternetMAE : MetraApplicationException
    {
        public Uri Address { get; private set; }

        public NoInternetMAE(Uri address)
            : base(@"No internet connection detected!")
        {
            this.Address = address;
        }
    }
    public class FirmwareFolderValidationFailureMAE : MetraApplicationException 
    {
        public FirmwareFolderValidationFailureMAE()
            : base(@"Firmware folder failed validation check!")
        {
        }
    }
    #endregion

    /// <summary>
    /// Class incapsulates the status of and management of file system operations for the application.
    /// </summary>
    public class FileManager
    {
        const string MAPS_FOLDER = "maps";
        const string CONFIG_FILE = "appconfig.cfg";
        const string FIRMWARE_FOLDER = "firmware";
        const string FIRMWARE_ARCHIVE = "firmware.zip";
        const string FIRMWARE_MANIFEST = "firmware.txt";
        const string MANIFEST_URL = "http://axxessupdater.com/admin/secure/manifest-request.php";
        const string BATCH_URL = "http://axxessupdater.com/admin/secure/batch-download.php";

        public string FirmwareFolder { get; set; }
        public string MapFolder { get; set; }
        public string ManifestFile { get; set; }
        public string FirmwareArchive { get; set; }
        public string ManifestURL { get; set; }
        public string BatchURL { get; set; }

        public WebClient Web { get; set; }

        public FileManager(ErrorManager Eman)
        {
            FirmwareFolder = FIRMWARE_FOLDER;
            MapFolder = MAPS_FOLDER;
            ManifestFile = FIRMWARE_MANIFEST;
            FirmwareArchive = FIRMWARE_ARCHIVE;
            ManifestURL = MANIFEST_URL;
            BatchURL = BATCH_URL;

            Web = new WebClient();

            //Register error handlers
        }

        public void ValidateFirmwareDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new FirmwareFolderValidationFailureMAE();
            }
        }

        /// <summary>
        /// Checks if a directory exists and creates it if necessary.
        /// </summary>
        /// <param name="folder">Path to directory</param>
        public DirectoryInfo CreateDirectory(string path)
        {
            DirectorySecurity dsec = new DirectorySecurity();
            dsec.AddAccessRule(new FileSystemAccessRule(
                System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                FileSystemRights.FullControl, AccessControlType.Allow));

            DirectoryInfo dinfo = Directory.CreateDirectory(path, dsec);

            dinfo.Attributes &= ~FileAttributes.ReadOnly;

            return dinfo;
        }

        public bool IsInternetAvailable()
        {
            Ping myPing = new Ping();
            String host = "8.8.8.8";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            return (reply.Status == IPStatus.Success);
        }

        private bool CompareManifests()
        {
            Web.DownloadFile(ManifestFile, "temp");

            string newManifest = File.ReadAllText("temp");
            string oldManifest = File.ReadAllText(FirmwareFolder);

            return (newManifest.Equals(oldManifest));
        }

        public string SearchManifest(string boardID)
        {
            foreach (string line in File.ReadAllLines(ManifestFile))
            {
                string[] entry = line.Split(',');
                if (entry[0].Equals(boardID))
                {
                    return entry[2];
                }
            }
            return String.Empty;
        }

        public void DownloadArchive(string url)
        {
            if (!IsInternetAvailable())
                throw new NoInternetMAE(new Uri(url));

            //mainStatusLabel.Text = "Downloading latest firmware archive...";
            Web.DownloadFileAsync(new Uri(url), FirmwareArchive);
            File.SetAttributes(FirmwareArchive, FileAttributes.Normal);
            //mainStatusLabel.Text = "Finshed downloading.";
        }

        public void DownloadManifest(string url)
        {
            if (!IsInternetAvailable())
                throw new NoInternetMAE(new Uri(url));

            //mainStatusLabel.Text = "Downloading latest firmware manifest...";
            Web.DownloadFileAsync(new Uri(url), ManifestFile);
            File.SetAttributes(ManifestFile, FileAttributes.Normal);
            //mainStatusLabel.Text = "Finshed downloading.";
        }

        public string GetPathToFirmware(string firmware)
        {
            if (!File.Exists(FIRMWARE_FOLDER + "\\" + firmware))
            {
                throw new FileNotFoundException(firmware);
            }
            else
            {
                return Path.GetFullPath(FIRMWARE_FOLDER + "\\" + firmware);
            }
        }

        public void UnpackFirmwareArchive()
        {
            try
            {
                ZipFile.ExtractToDirectory(FirmwareArchive, FirmwareFolder);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}

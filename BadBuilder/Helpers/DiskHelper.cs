using BadBuilder.Models;
using System.Diagnostics;
using System.Management;

namespace BadBuilder.Helpers
{
    internal static class DiskHelper
    {
        public static List<DiskInfo> GetDisks()
        {
            var disks = new List<DiskInfo>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    string driveLetter = drive.Name;
                    string volumeLabel = drive.VolumeLabel;
                    string type = drive.DriveType.ToString();
                    long totalSize = drive.TotalSize;
                    long availableFreeSpace = drive.AvailableFreeSpace;
                    int diskNumber = 2;

                    disks.Add(new DiskInfo(driveLetter, type, totalSize, volumeLabel, availableFreeSpace, diskNumber));
                }
            }

            return disks;
        }

        internal static async Task FormatDisk(DiskInfo disk)
        {
            ResourceHelper.ExtractEmbeddedBinary("fat32format.exe");

            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = @"fat32format.exe",
                    Arguments = $"-c64 {disk.DriveLetter}",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
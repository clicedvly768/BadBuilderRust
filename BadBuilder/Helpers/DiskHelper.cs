using BadBuilder.Models;
using BadBuilder.Formatter;

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

        internal static string FormatDisk(DiskInfo disk) => DiskFormatter.FormatVolume(disk.DriveLetter.ToCharArray()[0]);
    }
}
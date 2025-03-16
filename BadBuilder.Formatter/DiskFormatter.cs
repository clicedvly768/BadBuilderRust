using static BadBuilder.Formatter.Win32;
using static BadBuilder.Formatter.Constants;
using static BadBuilder.Formatter.FAT32Utilities;
using System.Runtime.InteropServices;

namespace BadBuilder.Formatter
{
    public static class DiskFormatter
    {
        public static unsafe (int, string) FormatVolume(char driveLetter)
        {
            uint cbRet;

            DISK_GEOMETRY diskGeometry;
            PARTITION_INFORMATION diskPartInfo;
            PARTITION_INFORMATION_EX exDiskPartInfo;
            bool isGPT = false;
            uint bytesPerSector = 0;
            uint totalSectors;
            uint fatSize;

            string devicePath = $"\\\\.\\{driveLetter}:";
            uint volumeID = GetVolumeID();


            IntPtr driveHandle = CreateFileW(
                devicePath,
                GENERIC_READ | GENERIC_WRITE,
                0,
                0,
                OPEN_EXISTING,
                FILE_FLAG_NO_BUFFERING,
                0);

            if (driveHandle == -1) return (-1, Error("Unable to open device - close all open programs or windows that may have a handle lock on the drive."));

            if (!DeviceIoControl( driveHandle, FSCTL_ALLOW_EXTENDED_DASD_IO, 0, 0, 0, 0, out cbRet, 0)) 
                return (-1, Error("Failed to enable extended DASD IO on the device."));

            if (!DeviceIoControl(driveHandle, FSCTL_LOCK_VOLUME, 0, 0, 0, 0, out cbRet, 0)) 
                return (-1, Error("Failed to lock the device."));

       
            using (var pDiskGeometry = NativePointer.Allocate<DISK_GEOMETRY>())
            {
                if (!DeviceIoControl(driveHandle, IOCTL_DISK_GET_DRIVE_GEOMETRY, 0, 0, pDiskGeometry.Pointer, pDiskGeometry.Size, out cbRet, 0))
                    return (-1, Error("Failed to get the drive geometry."));

                diskGeometry = Marshal.PtrToStructure<DISK_GEOMETRY>(pDiskGeometry.Pointer);
            }
            bytesPerSector = diskGeometry.BytesPerSector;

            using (var pDrivePartInfo = NativePointer.Allocate<PARTITION_INFORMATION>())
            {
                if (!DeviceIoControl(driveHandle, IOCTL_DISK_GET_PARTITION_INFO, 0, 0, pDrivePartInfo.Pointer, pDrivePartInfo.Size, out cbRet, 0))
                    return (-1, Error("Failed to get the drive partition information."));

                diskPartInfo = Marshal.PtrToStructure<PARTITION_INFORMATION>(pDrivePartInfo.Pointer);
            }

            using (var pDriveExPartInfo = NativePointer.Allocate<PARTITION_INFORMATION_EX>())
            {
                if (!DeviceIoControl(driveHandle, IOCTL_DISK_GET_PARTITION_INFO_EX, 0, 0, pDriveExPartInfo.Pointer, pDriveExPartInfo.Size, out cbRet, 0))
                    return (-1, Error("Failed to get the drive extended partition information."));

                exDiskPartInfo = Marshal.PtrToStructure<PARTITION_INFORMATION_EX>(pDriveExPartInfo.Pointer);
            }
            isGPT = (exDiskPartInfo.PartitionStyle == PARTITION_STYLE.GPT);

            totalSectors = (uint)(diskPartInfo.PartitionLength / diskGeometry.BytesPerSector);
            if (totalSectors < 65536 || totalSectors >= 0xffffffff)
                return (-1, Error("Invalid drive size for FAT32 - either too small (less than 64K clusters) or too large (greater than 2TB)."));

            FAT32BootSector bootSector;
            FAT32FsInfoSector fsInfo;

            bootSector.JumpCode = [0xEB, 0x58, 0x90];
            bootSector.OEMName = "MSWIN4.1".ToCharArray();
            bootSector.BytesPerSector = (ushort)bytesPerSector;
            bootSector.SectorsPerCluster = CalculateSectorsPerCluster((ulong)diskPartInfo.PartitionLength, bytesPerSector);
            bootSector.ReservedSectorCount = 32;
            bootSector.NumberOfFATs = 2;
            bootSector.MaxRootEntries = 0;
            bootSector.TotalSectors16 = 0;
            bootSector.MediaDescriptor = 0xF8;
            bootSector.SectorsPerFAT16 = 0;
            bootSector.SectorsPerTrack = (ushort)diskGeometry.SectorsPerTrack;
            bootSector.NumberOfHeads = (ushort)diskGeometry.TracksPerCylinder;
            bootSector.HiddenSectors = diskPartInfo.HiddenSectors;
            bootSector.TotalSectors = totalSectors;

            fatSize = CalculateFATSize(bootSector.TotalSectors, bootSector.ReservedSectorCount, bootSector.SectorsPerCluster, bootSector.NumberOfFATs, bytesPerSector);

            bootSector.SectorsPerFAT = fatSize;
            bootSector.FATFlags = 0;
            bootSector.FileSystemVersion = 0;
            bootSector.RootCluster = 2;
            bootSector.FSInfoSector = 1;
            bootSector.BackupBootSector = 6;
            bootSector.DriveNumber = 0x80;
            bootSector.Reserved1 = 0;
            bootSector.BootSignature = 0x29;
            bootSector.VolumeID = volumeID;
            bootSector.VolumeLabel = "BADUPDATE  ".ToCharArray();
            bootSector.FileSystemType = "FAT32   ".ToCharArray();
            bootSector.Signature = 0x55AA;

            if (!DeviceIoControl(driveHandle, FSCTL_UNLOCK_VOLUME, 0, 0, 0, 0, out cbRet, 0))
                return (-1, Error("Failed to unlock the device."));

            return (0, "");
        }
    }
}

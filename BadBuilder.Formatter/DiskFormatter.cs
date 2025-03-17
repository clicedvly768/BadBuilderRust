#pragma warning disable CA1416
using System.Text;
using Windows.Win32.System.Ioctl;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using Windows.Win32.Storage.FileSystem;

using static Windows.Win32.PInvoke;
using static BadBuilder.Formatter.Constants;
using static BadBuilder.Formatter.Utilities;

namespace BadBuilder.Formatter
{
    public static class DiskFormatter
    {
        public static unsafe string FormatVolume(char driveLetter)
        {
            string devicePath = $"\\\\.\\{driveLetter}:";
            uint volumeID = GetVolumeID();

            using SafeFileHandle driveHandle = OpenDeviceHandle(devicePath);
            if (driveHandle.IsInvalid) return Error("Unable to open device. GetLastError: " + Marshal.GetLastWin32Error());

            if (!EnableExtendedDASDIO(driveHandle) || !LockDevice(driveHandle))
                return Error("Failed to initialize device access.");

            DISK_GEOMETRY diskGeometry;
            if (!TryGetDiskGeometry(driveHandle, out diskGeometry))
                return Error("Failed to get drive geometry.");

            PARTITION_INFORMATION partitionInfo = new();
            bool isGPT = false;
            if (!TryGetPartitionInfo(driveHandle, ref diskGeometry, out partitionInfo, out isGPT))
                return Error("Failed to get partition information.");

            uint totalSectors = (uint)(partitionInfo.PartitionLength / diskGeometry.BytesPerSector);
            if (!IsValidFAT32Size(totalSectors))
                return Error("Invalid drive size for FAT32.");

            FAT32BootSector bootSector = InitializeBootSector(diskGeometry, partitionInfo, totalSectors, volumeID);
            FAT32FsInfoSector fsInfo = InitializeFsInfo();
            uint[] firstFATSector = InitializeFirstFATSector(diskGeometry.BytesPerSector);

            string formatOutput = FormatVolumeData(driveHandle, diskGeometry, bootSector, fsInfo, firstFATSector, isGPT, partitionInfo);
            if (formatOutput != string.Empty)
                return formatOutput;

            if (!UnlockDevice(driveHandle) || !DismountVolume(driveHandle))
                return Error($"Failed to release the device. GetLastError: {Marshal.GetLastWin32Error()}");


            return string.Empty;
        }


        private static SafeFileHandle OpenDeviceHandle(string devicePath) =>
            CreateFile(devicePath, GENERIC_READ | GENERIC_WRITE, 0, null, FILE_CREATION_DISPOSITION.OPEN_EXISTING, FILE_FLAGS_AND_ATTRIBUTES.FILE_FLAG_NO_BUFFERING, null);

        private static unsafe bool EnableExtendedDASDIO(SafeFileHandle handle) => 
            DeviceIoControl(handle, FSCTL_ALLOW_EXTENDED_DASD_IO, null, 0, null, 0, null, null);

        private static unsafe bool LockDevice(SafeFileHandle handle) => 
            DeviceIoControl(handle, FSCTL_LOCK_VOLUME, null, 0, null, 0, null, null);

        private static unsafe bool UnlockDevice(SafeFileHandle handle) => 
            DeviceIoControl(handle, FSCTL_UNLOCK_VOLUME, null, 0, null, 0, null, null);

        private static unsafe bool DismountVolume(SafeFileHandle handle) => 
            DeviceIoControl(handle, FSCTL_DISMOUNT_VOLUME, null, 0, null, 0, null, null);

        private static unsafe bool TryGetDiskGeometry(SafeFileHandle handle, out DISK_GEOMETRY diskGeometry)
        {
            diskGeometry = new DISK_GEOMETRY();
            fixed (DISK_GEOMETRY* pDiskGeometry = &diskGeometry)
            {
                return DeviceIoControl(handle, IOCTL_DISK_GET_DRIVE_GEOMETRY, null, 0, pDiskGeometry, (uint)sizeof(DISK_GEOMETRY), null, null);
            }
        }

        private static unsafe bool TryGetPartitionInfo(SafeFileHandle handle, ref DISK_GEOMETRY diskGeometry, out PARTITION_INFORMATION partitionInfo, out bool isGPT)
        {
            partitionInfo = new PARTITION_INFORMATION();
            PARTITION_INFORMATION_EX diskExPartInfo = new();
            isGPT = false;

            fixed (PARTITION_INFORMATION* pPartitionInfo = &partitionInfo)
            {
                if (!DeviceIoControl(handle, IOCTL_DISK_GET_PARTITION_INFO, null, 0, pPartitionInfo, (uint)sizeof(PARTITION_INFORMATION), null, null))
                {
                    if (DeviceIoControl(handle, IOCTL_DISK_GET_PARTITION_INFO_EX, null, 0, &diskExPartInfo, (uint)sizeof(PARTITION_INFORMATION_EX), null, null))
                    {
                        partitionInfo = new PARTITION_INFORMATION
                        {
                            StartingOffset = diskExPartInfo.StartingOffset,
                            PartitionLength = diskExPartInfo.PartitionLength,
                            HiddenSectors = (uint)(diskExPartInfo.StartingOffset / diskGeometry.BytesPerSector)
                        };
                        isGPT = (diskExPartInfo.PartitionStyle == PARTITION_STYLE.PARTITION_STYLE_GPT);
                        return true;
                    }
                    return false;
                }
                return true;
            }
        }

        private static bool IsValidFAT32Size(uint totalSectors) =>
            totalSectors >= 65536 && totalSectors < 0xFFFFFFFF;

        private static unsafe FAT32BootSector InitializeBootSector(DISK_GEOMETRY diskGeometry, PARTITION_INFORMATION partitionInfo, uint totalSectors, uint volumeID)
        {
            byte sectorsPerCluster = CalculateSectorsPerCluster((ulong)partitionInfo.PartitionLength, diskGeometry.BytesPerSector);
            uint fatSize = CalculateFATSize(totalSectors, 32, sectorsPerCluster, 2, diskGeometry.BytesPerSector);

            FAT32BootSector bootSector = new FAT32BootSector
            {
                BytesPerSector = (ushort)diskGeometry.BytesPerSector,
                SectorsPerCluster = sectorsPerCluster,
                ReservedSectorCount = 32,
                NumberOfFATs = 2,
                MediaDescriptor = 0xF8,
                SectorsPerTrack = (ushort)diskGeometry.SectorsPerTrack,
                NumberOfHeads = (ushort)diskGeometry.TracksPerCylinder,
                HiddenSectors = partitionInfo.HiddenSectors,
                TotalSectors = totalSectors,
                SectorsPerFAT = fatSize,
                RootCluster = 2,
                FSInfoSector = 1,
                BackupBootSector = 6,
                DriveNumber = 0x80,
                BootSignature = 0x29,
                VolumeID = volumeID,
                Signature = 0x55AA
            };

            Span<byte> rawBytes = MemoryMarshal.AsBytes(new Span<FAT32BootSector>(ref bootSector));

            if (bootSector.BytesPerSector != 512)
            {
                rawBytes[bootSector.BytesPerSector - 2] = 0x55;
                rawBytes[bootSector.BytesPerSector - 1] = 0xAA;
            }

            rawBytes[0] = 0xEB;
            rawBytes[1] = 0x58;
            rawBytes[2] = 0x90;

            string oemName = "MSWIN4.1";
            Encoding.ASCII.GetBytes(oemName).CopyTo(rawBytes.Slice(3, 8));

            string volumeLabel = "BADUPDATE  ";
            Encoding.ASCII.GetBytes(volumeLabel).CopyTo(rawBytes.Slice(71, 11));

            string fileSystemType = "FAT32   ";
            Encoding.ASCII.GetBytes(fileSystemType).CopyTo(rawBytes.Slice(82, 8)); 

            return bootSector;
        }

        private static FAT32FsInfoSector InitializeFsInfo() => new FAT32FsInfoSector
        {
            LeadSignature = 0x41615252,
            StructureSignature = 0x61417272,
            TrailSignature = 0xAA550000,
            FreeClusterCount = 0,
            NextFreeCluster = 3
        };

        private static uint[] InitializeFirstFATSector(uint bytesPerSector)
        {
            uint[] sector = new uint[bytesPerSector / 4];
            sector[0] = 0x0FFFFFF8;
            sector[1] = 0x0FFFFFFF;
            sector[2] = 0x0FFFFFFF;
            return sector;
        }

        private static unsafe string FormatVolumeData(SafeFileHandle driveHandle, DISK_GEOMETRY geometry, FAT32BootSector bootSector, FAT32FsInfoSector fsInfo, uint[] firstFATSector, bool isGPT, PARTITION_INFORMATION partitionInfo)
        {
            uint bytesPerSector = geometry.BytesPerSector;
            uint totalSectors = (uint)(partitionInfo.PartitionLength / geometry.BytesPerSector);
            uint userAreaSize = totalSectors - bootSector.ReservedSectorCount - (bootSector.NumberOfFATs * bootSector.SectorsPerFAT);
            uint systemAreaSize = bootSector.ReservedSectorCount + (bootSector.NumberOfFATs * bootSector.SectorsPerFAT) + bootSector.SectorsPerCluster;
            uint clusterCount = userAreaSize / bootSector.SectorsPerCluster;

            if (clusterCount < 65536 || clusterCount > 0x0FFFFFFF)
                return Error("The drives cluster count is out of range (65536 < clusterCount < 0x0FFFFFFF)");

            fsInfo.FreeClusterCount = (userAreaSize / bootSector.SectorsPerCluster) - 1;

            ZeroOutSectors(driveHandle, 0, systemAreaSize, bytesPerSector);

            for (int i = 0; i < 2; i++)
            {
                uint sectorStart = (i == 0) ? 0 : (uint)bootSector.BackupBootSector;
                WriteSector(driveHandle, sectorStart, 1, bytesPerSector, StructToBytes(bootSector));
                WriteSector(driveHandle, sectorStart + 1, 1, bytesPerSector, StructToBytes(fsInfo));
            }

            for (int i = 0; i < bootSector.NumberOfFATs; i++)
            {
                uint sectorStart = bootSector.ReservedSectorCount + ((uint)i * bootSector.SectorsPerFAT);
                WriteSector(driveHandle, sectorStart, 1, bytesPerSector, UintArrayToBytes(firstFATSector));
            }

            if (!isGPT)
            {
                SET_PARTITION_INFORMATION setPartInfo = new SET_PARTITION_INFORMATION
                {
                    PartitionType = 0x0C
                };

                if (!DeviceIoControl(driveHandle, IOCTL_DISK_SET_PARTITION_INFO, &setPartInfo, (uint)sizeof(SET_PARTITION_INFORMATION), null, 0, null, null))
                    return Error($"Failed to set the drive partition information. GetLastError: {Marshal.GetLastWin32Error()}");
            }

            return "";
        }
    }
}
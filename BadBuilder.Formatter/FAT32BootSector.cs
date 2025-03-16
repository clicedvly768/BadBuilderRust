using System.Runtime.InteropServices;

namespace BadBuilder.Formatter
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct FAT32BootSector
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] JumpCode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] OEMName;

        public ushort BytesPerSector;
        public byte SectorsPerCluster;
        public ushort ReservedSectorCount;
        public byte NumberOfFATs;
        public ushort MaxRootEntries;  // Unused in FAT32
        public ushort TotalSectors16;  // If 0, use TotalSectors
        public byte MediaDescriptor;
        public ushort SectorsPerFAT16; // Unused in FAT32
        public ushort SectorsPerTrack;
        public ushort NumberOfHeads;
        public uint HiddenSectors;
        public uint TotalSectors;      // Total sectors (if TotalSectors16 is 0)

        // FAT32-specific fields
        public uint SectorsPerFAT;
        public ushort FATFlags;
        public ushort FileSystemVersion;
        public uint RootCluster;
        public ushort FSInfoSector;
        public ushort BackupBootSector;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] Reserved;

        public byte DriveNumber;
        public byte Reserved1;
        public byte BootSignature;
        public uint VolumeID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public byte[] VolumeLabel;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] FileSystemType;
    }
}
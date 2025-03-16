using System.Runtime.InteropServices;

namespace BadBuilder.Formatter
{
    // Reference: https://cscie92.dce.harvard.edu/spring2024/K70F120M/fsInfo.h

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 512)]
    internal struct FAT32FsInfoSector
    {
        [FieldOffset(0)] public uint LeadSignature;

        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
        public byte[] Reserved1; // Zeros

        [FieldOffset(484)] public uint StructureSignature;
        [FieldOffset(488)] public uint FreeClusterCount;
        [FieldOffset(492)] public uint NextFreeCluster;

        [FieldOffset(496)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] Reserved2;

        [FieldOffset(508)] public uint TrailSignature;
    }
}
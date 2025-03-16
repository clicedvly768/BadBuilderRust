using System.Runtime.InteropServices;

namespace BadBuilder.Formatter
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct FAT32FsInfo
    {
        public uint LeadSignature; // Should be 0x41615252

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 480)]
        public byte[] Reserved1; // Zeros

        public uint StructureSignature; // Should be 0x61417272
        public uint FreeClusterCount;   // Number of free clusters (or 0xFFFFFFFF if unknown)
        public uint NextFreeCluster;    // Next free cluster (or 0xFFFFFFFF if unknown)

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] Reserved2; // Zeros

        public uint TrailSignature; // Should be 0xAA550000
    }
}
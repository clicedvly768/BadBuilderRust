using System.Runtime.InteropServices;

namespace BadBuilder.Formatter
{
    static partial class Win32
    {
        internal const uint GENERIC_READ = 0x80000000;
        internal const uint GENERIC_WRITE = 0x40000000;
        internal const uint OPEN_EXISTING = 3;
        internal const uint FILE_SHARE_READ = 1;
        internal const uint FILE_BEGIN = 0;
        internal const uint FILE_FLAG_NO_BUFFERING = 0x20000000;

        internal const uint IOCTL_DISK_GET_DRIVE_GEOMETRY = 0x00070000;
        internal const uint IOCTL_DISK_GET_PARTITION_INFO_EX = 0x00070048;
        internal const uint IOCTL_DISK_GET_PARTITION_INFO = 0x00074004;
        internal const uint FSCTL_LOCK_VOLUME = 0x00090018;
        internal const uint FSCTL_UNLOCK_VOLUME = 0x0009001C;
        internal const uint FSCTL_QUERY_RETRIEVAL_POINTERS = 0x0009003B;
        internal const uint FSCTL_GET_COMPRESSION = 0x0009003C;
        internal const uint FSCTL_SET_COMPRESSION = 0x0009C040;
        internal const uint FSCTL_SET_BOOTLOADER_ACCESSED = 0x0009004F;
        internal const uint FSCTL_MARK_AS_SYSTEM_HIVE = 0x0009004F;
        internal const uint FSCTL_OPLOCK_BREAK_ACK_NO_2 = 0x00090050;
        internal const uint FSCTL_INVALIDATE_VOLUMES = 0x00090054;
        internal const uint FSCTL_QUERY_FAT_BPB = 0x00090058;
        internal const uint FSCTL_REQUEST_FILTER_OPLOCK = 0x0009005C;
        internal const uint FSCTL_FILESYSTEM_GET_STATISTICS = 0x00090060;
        internal const uint FSCTL_GET_NTFS_VOLUME_DATA = 0x00090064;
        internal const uint FSCTL_GET_NTFS_FILE_RECORD = 0x00090068;
        internal const uint FSCTL_GET_VOLUME_BITMAP = 0x0009006F;
        internal const uint FSCTL_GET_RETRIEVAL_POINTERS = 0x00090073;
        internal const uint FSCTL_MOVE_FILE = 0x00090074;
        internal const uint FSCTL_IS_VOLUME_DIRTY = 0x00090078;
        internal const uint FSCTL_ALLOW_EXTENDED_DASD_IO = 0x00090083;


        private const string Kernel32 = "kernel32.dll";

        [LibraryImport(Kernel32, SetLastError = true)]
        internal static partial IntPtr CreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            IntPtr hTemplateFile);


        [LibraryImport(Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool WriteFile(
            IntPtr hFile,
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite,
            out uint lpNumberOfBytesWritten,
            IntPtr lpOverlapped);

        [LibraryImport(Kernel32, SetLastError = true)]
        internal static partial uint SetFilePointer(
            IntPtr hFile,
            int lDistanceToMove,
            ref int lpDistanceToMoveHigh,
            uint dwMoveMethod);

        [LibraryImport(Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool DeviceIoControl(
            IntPtr hDevice,
            uint dwIoControlCode,
            IntPtr lpInBuffer,
            uint nInBufferSize,
            IntPtr lpOutBuffer,
            uint nOutBufferSize,
            out uint lpBytesReturned,
            IntPtr lpOverlapped);

        [LibraryImport(Kernel32, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static partial bool CloseHandle(IntPtr hObject);


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        internal struct DISK_GEOMETRY
        {
            internal long Cylinders;
            internal MediaType MediaType;
            internal uint TracksPerCylinder;
            internal uint SectorsPerTrack;
            internal uint BytesPerSector;
        }

        internal enum MediaType // from winioctl.h
        {
            Unknown,                // Format is unknown
            F5_1Pt2_512,            // 5.25", 1.2MB,  512 bytes/sector
            F3_1Pt44_512,           // 3.5",  1.44MB, 512 bytes/sector
            F3_2Pt88_512,           // 3.5",  2.88MB, 512 bytes/sector
            F3_20Pt8_512,           // 3.5",  20.8MB, 512 bytes/sector
            F3_720_512,             // 3.5",  720KB,  512 bytes/sector
            F5_360_512,             // 5.25", 360KB,  512 bytes/sector
            F5_320_512,             // 5.25", 320KB,  512 bytes/sector
            F5_320_1024,            // 5.25", 320KB,  1024 bytes/sector
            F5_180_512,             // 5.25", 180KB,  512 bytes/sector
            F5_160_512,             // 5.25", 160KB,  512 bytes/sector
            RemovableMedia,         // Removable media other than floppy
            FixedMedia,             // Fixed hard disk media
            F3_120M_512,            // 3.5", 120M Floppy
            F3_640_512,             // 3.5" ,  640KB,  512 bytes/sector
            F5_640_512,             // 5.25",  640KB,  512 bytes/sector
            F5_720_512,             // 5.25",  720KB,  512 bytes/sector
            F3_1Pt2_512,            // 3.5" ,  1.2Mb,  512 bytes/sector
            F3_1Pt23_1024,          // 3.5" ,  1.23Mb, 1024 bytes/sector
            F5_1Pt23_1024,          // 5.25",  1.23MB, 1024 bytes/sector
            F3_128Mb_512,           // 3.5" MO 128Mb   512 bytes/sector
            F3_230Mb_512,           // 3.5" MO 230Mb   512 bytes/sector
            F8_256_128,             // 8",     256KB,  128 bytes/sector
            F3_200Mb_512,           // 3.5",   200M Floppy (HiFD)
            F3_240M_512,            // 3.5",   240Mb Floppy (HiFD)
            F3_32M_512              // 3.5",   32Mb Floppy
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 16)]
        internal struct GUID
        {
            internal uint Data1;
            internal ushort Data2;
            internal ushort Data3;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            internal byte[] Data4;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 32)]
        internal struct PARTITION_INFORMATION
        {
            internal long StartingOffset;
            internal long PartitionLength;
            internal uint HiddenSectors;
            internal uint PartitionNumber;
            internal byte PartitionType;
            internal byte BootIndicator;
            internal byte RecognizedPattern;
            internal byte RewritePartition;
        }

        [StructLayout(LayoutKind.Explicit, Size = 144)]
        internal struct PARTITION_INFORMATION_EX
        {
            [FieldOffset(0)]
            internal PARTITION_STYLE PartitionStyle;

            [FieldOffset(8)]
            internal long StartingOffset;

            [FieldOffset(16)]
            internal long PartitionLength;

            [FieldOffset(24)]
            internal uint PartitionNumber;

            [FieldOffset(28)]
            internal byte RewritePartition;

            [FieldOffset(29)]
            internal byte IsServicePartition;

            [FieldOffset(32)]
            internal unsafe fixed byte Union[112];

            public unsafe PARTITION_INFORMATION_MBR Mbr
            {
                get
                {
                    fixed (byte* p = Union)
                    {
                        return *(PARTITION_INFORMATION_MBR*)p;
                    }
                }
                set
                {
                    fixed (byte* p = Union)
                    {
                        *(PARTITION_INFORMATION_MBR*)p = value;
                    }
                }
            }

            public unsafe PARTITION_INFORMATION_GPT Gpt
            {
                get
                {
                    fixed (byte* p = Union)
                    {
                        return *(PARTITION_INFORMATION_GPT*)p;
                    }
                }
                set
                {
                    fixed (byte* p = Union)
                    {
                        *(PARTITION_INFORMATION_GPT*)p = value;
                    }
                }
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 24)]
        internal struct PARTITION_INFORMATION_MBR
        {
            internal byte PartitionType;
            internal byte BootIndicator;
            internal byte RecognizedPartition;
            private byte _padding1;
            internal uint HiddenSectors;
            internal GUID PartitionId;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 112)]
        internal struct PARTITION_INFORMATION_GPT
        {
            internal GUID PartitionType;
            internal GUID PartitionId;
            internal ulong Attributes;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
            internal ushort[] Name;
        }

        internal enum PARTITION_STYLE : uint
        {
            MBR = 0,
            GPT = 1,
            RAW = 2
        }
    }
}
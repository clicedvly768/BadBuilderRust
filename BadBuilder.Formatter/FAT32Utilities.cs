using static BadBuilder.Formatter.Win32;
using static BadBuilder.Formatter.Constants;
using System.Runtime.InteropServices;

namespace BadBuilder.Formatter
{
    static class FAT32Utilities
    {
        internal struct NativePointer : IDisposable
        {
            internal IntPtr Pointer;
            internal uint Size;

            internal NativePointer(Type type)
            {
                uint size = (uint)Marshal.SizeOf(type);
                Pointer = Marshal.AllocHGlobal((int)size);
                Size = size;
            }

            public void Dispose()
            {
                if (Pointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Pointer);
                    Pointer = IntPtr.Zero;
                    Size = 0;
                }
            }

            internal static NativePointer Allocate<T>() where T : struct
            {
                return new NativePointer(typeof(T));
            }
        }

        internal static string Error(string error) => $"{ORANGE}[-]{ANSI_RESET} {error}";
        internal static void ExitWithError(string error)
        {
            Console.WriteLine($"{ORANGE}[-]{ANSI_RESET} {error}");
            Environment.Exit(-1);
        }

        internal static uint GetVolumeID()
        {
            DateTime now = DateTime.Now;

            ushort low = (ushort)(now.Day + (now.Month << 8));
            low += (ushort)((now.Millisecond / 10) + (now.Second << 8));

            ushort hi = (ushort)(now.Minute + (now.Hour << 8));
            hi += (ushort)now.Year;

            return (uint)(low | (hi << 16));
        }

        internal static uint CalculateFATSize(uint totalSectors, uint reservedSectors, uint sectorsPerCluster, uint numberOfFATs, uint bytesPerSector)
        {
            const ulong fatElementSize = 4;

            ulong numerator = fatElementSize * (totalSectors - reservedSectors);
            ulong denominator = (sectorsPerCluster * bytesPerSector) + (fatElementSize * numberOfFATs);

            return (uint)((numerator / denominator) + 1);
        }

        internal static byte CalculateSectorsPerCluster(ulong diskSizeBytes, uint bytesPerSector) => (diskSizeBytes / (1024 * 1024)) switch
        {
            var size when size > 512 => (byte)((4 * 1024) / bytesPerSector),
            var size when size > 8192 => (byte)((8 * 1024) / bytesPerSector),
            var size when size > 16384 => (byte)((16 * 1024) / bytesPerSector),
            var size when size > 32768 => (byte)((32 * 1024) / bytesPerSector),
            _ => 1
        };


        internal static void SeekTo(IntPtr hDevice, uint sector, uint bytesPerSector)
        {
            long offset = sector * bytesPerSector;

            int lowOffset = (int)(offset & 0xFFFFFFFF);
            int highOffset = (int)(offset >> 32);

            SetFilePointer(hDevice, lowOffset, ref highOffset, FILE_BEGIN);
        }

        internal static void WriteSector(IntPtr hDevice, uint sector, uint numberOfSectors, uint bytesPerSector, byte[] data)
        {
            uint bytesWritten;

            SeekTo(hDevice, sector, bytesPerSector);

            if (!WriteFile(hDevice, data, numberOfSectors * bytesPerSector, out bytesWritten, 0))
                ExitWithError("Unable to write to write sectors to FAT32 device, exiting.");
        }

        internal static void ZeroOutSectors(IntPtr hDevice, uint sector, uint numberOfSectors, uint bytesPerSector)
        {
            const uint burstSize = 128;
            uint writeSize;

            byte[] zeroBuffer = new byte[bytesPerSector * burstSize];
            Array.Clear(zeroBuffer);

            SeekTo(hDevice, sector, bytesPerSector);

            while (numberOfSectors > 0)
            {
                writeSize = (numberOfSectors > burstSize) ? burstSize : numberOfSectors;
                WriteSector(hDevice, sector, numberOfSectors, bytesPerSector, zeroBuffer);
                numberOfSectors -= writeSize;
            }
        }
    }
}
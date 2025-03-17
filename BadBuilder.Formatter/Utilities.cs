using Windows.Win32.Foundation;
using Windows.Win32.System.Memory;
using System.Runtime.InteropServices;

using static Windows.Win32.PInvoke;
using static BadBuilder.Formatter.Constants;

namespace BadBuilder.Formatter
{
    static class Utilities
    {
        internal static byte[] StructToBytes<T>(T @struct) where T : struct
        {
            Span<T> structSpan = MemoryMarshal.CreateSpan(ref @struct, 1);
            Span<byte> byteSpan = MemoryMarshal.AsBytes(structSpan);
            return byteSpan.ToArray();
        }

        internal static byte[] UintArrayToBytes(uint[] array) => MemoryMarshal.AsBytes<uint>(array.AsSpan()).ToArray();


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


        internal static unsafe void SeekTo(SafeHandle hDevice, uint sector, uint bytesPerSector)
        {
            long offset = sector * bytesPerSector;

            int lowOffset = (int)(offset & 0xFFFFFFFF);
            int highOffset = (int)(offset >> 32);

            SetFilePointer(hDevice, lowOffset, &highOffset, Windows.Win32.Storage.FileSystem.SET_FILE_POINTER_MOVE_METHOD.FILE_BEGIN);
        }

        internal static unsafe void WriteSector(SafeHandle hDevice, uint sector, uint numberOfSectors, uint bytesPerSector, byte[] data)
        {
            SeekTo(hDevice, sector, bytesPerSector);

            fixed (byte* pData = &data[0])
            {
                if (!WriteFile(new HANDLE(hDevice.DangerousGetHandle()), pData, numberOfSectors * bytesPerSector, null, null))
                    ExitWithError($"Unable to write sectors to FAT32 device, exiting. GetLastError: {Marshal.GetLastWin32Error()}");
            }
        }

        internal static unsafe void ZeroOutSectors(SafeHandle hDevice, uint sector, uint numberOfSectors, uint bytesPerSector)
        {
            const uint burstSize = 128;
            uint writeSize;

            byte* pZeroSector = (byte*)VirtualAlloc(null, bytesPerSector * burstSize, VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE, PAGE_PROTECTION_FLAGS.PAGE_READWRITE);

            try
            {
                SeekTo(hDevice, sector, bytesPerSector);

                while (numberOfSectors > 0)
                {
                    writeSize = (numberOfSectors > burstSize) ? burstSize : numberOfSectors;

                    if (!WriteFile(new HANDLE(hDevice.DangerousGetHandle()), pZeroSector, writeSize * bytesPerSector, null, null))
                        ExitWithError($"Unable to write sectors to FAT32 device, exiting. GetLastError: {Marshal.GetLastWin32Error()}");

                    numberOfSectors -= writeSize;
                }
            }
            finally
            {
                VirtualFree(pZeroSector, bytesPerSector * burstSize, VIRTUAL_FREE_TYPE.MEM_RELEASE);
            }
        }
    }
}
namespace BadBuilder.Formatter
{
    static class FAT32Utilities
    {
        internal static uint GetVolumeID()
        {
            DateTime now = DateTime.Now;

            ushort low = (ushort)(now.Day + (now.Month << 8));
            low += (ushort)((now.Millisecond / 10) + (now.Second << 8));

            ushort hi = (ushort)(now.Minute + (now.Hour << 8));
            hi += (ushort)now.Year;

            return (uint)(low | (hi << 16));
        }

        internal static uint CalculateFATSize(uint diskSize, uint reservedSectorCount, uint sectorsPerCluster, uint numberOfFATs, uint bytesPerSector)
        {
            const ulong fatElementSize = 4;

            ulong numerator = fatElementSize * (diskSize - reservedSectorCount);
            ulong denominator = (sectorsPerCluster * bytesPerSector) + (fatElementSize * numberOfFATs);

            return (uint)((numerator / denominator) + 1);
        }
    }
}
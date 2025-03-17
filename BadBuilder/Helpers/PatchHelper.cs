using System.Diagnostics;

namespace BadBuilder.Helpers
{
    internal static class PatchHelper
    {
        internal static async Task PatchXexAsync(string xexPath, string xexToolPath)
        {
            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = xexToolPath,
                    Arguments = $"-m r -r a \"{xexPath}\"",
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BadBuilder.Helpers
{
    internal static class DownloadHelper
    {
        internal static async Task DownloadFile(HttpClient client, ProgressTask task, string url)
        {
            try
            {
                using (HttpResponseMessage response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    task.MaxValue(response.Content.Headers.ContentLength ?? 0);
                    task.StartTask();

                    string filename = url.Substring(url.LastIndexOf('/') + 1);

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var buffer = new byte[8192];
                        while (true)
                        {
                            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                            if (read == 0)
                                break;

                            task.Increment(read);
                            await fileStream.WriteAsync(buffer, 0, read);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error downloading:[/] {ex}");
            }
        }
    }
}
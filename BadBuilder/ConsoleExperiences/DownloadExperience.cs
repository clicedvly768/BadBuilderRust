using Spectre.Console;
using BadBuilder.Helpers;

using static BadBuilder.Constants;

namespace BadBuilder
{
    internal partial class Program
    {
        static async Task<List<ArchiveItem>> DownloadRequiredFiles()
        {
            List<DownloadItem> items = new()
            {
                ("XEXMenu", "https://consolemods.org/wiki/images/3/35/XeXmenu_12.7z"),
                ("Rock Band Blitz", "https://download.digiex.net/Consoles/Xbox360/Arcade-games/RBBlitz.zip"),
                ("Simple 360 NAND Flasher", "https://www.consolemods.org/wiki/images/f/ff/Simple_360_NAND_Flasher.7z"),
            };
            await DownloadHelper.GetGitHubAssets(items);

            List<DownloadItem> existingFiles = items.Where(item =>
                File.Exists(Path.Combine(DOWNLOAD_DIR, item.url.Split('/').Last()))).ToList();

            List<string> choices = items.Select(item =>
                existingFiles.Any(e => e.name == item.name)
                    ? $"{item.name} [italic gray](already exists)[/]"
                    : item.name).ToList();

            var prompt = new MultiSelectionPrompt<string>()
                .Title("Which files do you already have? [gray](Select all that apply)[/]")
                .PageSize(10)
                .NotRequired()
                .HighlightStyle(GreenStyle)
                .AddChoices(choices);

            foreach (string choice in choices)
            {
                if (existingFiles.Any(e => choice.StartsWith(e.name)))
                    prompt.Select(choice);
            }

            List<string> selectedItems = AnsiConsole.Prompt(prompt)
                .Select(choice => choice.Split(" [")[0])
                .ToList();


            List<DownloadItem> itemsToDownload = items.Where(item => !selectedItems.Contains(item.name)).ToList();

            itemsToDownload.Sort((a, b) => b.name.Length.CompareTo(a.name.Length));

            if (itemsToDownload.Any())
            {
                if (!Directory.Exists($"{DOWNLOAD_DIR}")) 
                    Directory.CreateDirectory($"{DOWNLOAD_DIR}");

                HttpClient downloadClient = new();

                await AnsiConsole.Progress()
                    .Columns(
                        new TaskDescriptionColumn(),
                        new ProgressBarColumn().FinishedStyle(GreenStyle).CompletedStyle(LightOrangeStyle),
                        new PercentageColumn().CompletedStyle(GreenStyle),
                        new RemainingTimeColumn().Style(GrayStyle),
                        new TransferSpeedColumn()
                    )
                    .StartAsync(async ctx =>
                    {
                        AnsiConsole.MarkupLine("[#76B900]{0}[/] Downloading required files.", Markup.Escape("[*]"));
                        await Task.WhenAll(itemsToDownload.Select(async item =>
                        {
                            var task = ctx.AddTask(item.name, new ProgressTaskSettings { AutoStart = false });
                            await DownloadHelper.DownloadFileAsync(downloadClient, task, item.url);
                        }));
                    });

                string status = "[+]";
                AnsiConsole.MarkupInterpolated($"[#76B900]{status}[/] [bold]{itemsToDownload.Count()}[/] download(s) completed.\n");
            }
            else
            {
                AnsiConsole.MarkupLine("[italic #76B900]No downloads required. All files already exist.[/]");
            }


            Console.WriteLine();
            foreach (string selectedItem in selectedItems)
            {
                string expectedFileName = items.First(i => i.name == selectedItem).url.Split('/').Last();
                string destinationPath = Path.Combine(DOWNLOAD_DIR, expectedFileName);

                if (File.Exists(destinationPath)) continue;

                string existingPath = AnsiConsole.Prompt(
                    new TextPrompt<string>($"Enter the path for the [bold]{selectedItem}[/] archive:")
                        .PromptStyle(LightOrangeStyle)
                        .Validate(path =>
                        {
                            return File.Exists(path.Trim().Trim('"'))
                                ? ValidationResult.Success()
                                : ValidationResult.Error("[red]File does not exist.[/]");
                        })
                ).Trim().Trim('"');

                File.Copy(existingPath, destinationPath, overwrite: true);
                AnsiConsole.MarkupLine($"[italic #76B900]Successfully copied [bold]{selectedItem}[/] to the working directory.[/]\n");
            }


            return items.Select(item => new ArchiveItem(item.name, Path.Combine(DOWNLOAD_DIR, item.url.Split('/').Last()))).ToList();
        }
    }
}
using Spectre.Console;
using BadBuilder.Models;
using BadBuilder.Helpers;

namespace BadBuilder
{
    internal partial class Program
    {
        static readonly Style OrangeStyle = new Style(new Color(255, 114, 0));
        static readonly Style GreenStyle = new Style(new Color(118, 185, 0));
        static readonly Style GrayStyle = new Style(new Color(132, 133, 137));

        static void Main(string[] args)
        {
            ShowWelcomeMessage();

            while (true)
            {
                Console.WriteLine();
                string action = PromptForAction();

                if (action == "Exit") Environment.Exit(0);

                List<DiskInfo> disks = DiskHelper.GetDisks();
                string selectedDisk = PromptForDiskSelection(disks);

                bool confirmation = PromptFormatConfirmation(selectedDisk);
                if (confirmation)
                {
                    FormatDisk(disks, selectedDisk).Wait();
                    break;
                }
            }

            Console.WriteLine();

            var items = new (string name, string url)[]
            {
                ("rbblitz", "https://download.digiex.net/Consoles/Xbox360/Arcade-games/RBBlitz.zip"),
                ("BadUpdate", "https://github.com/grimdoomer/Xbox360BadUpdate/releases/download/v1.1/Xbox360BadUpdate-Retail-USB-v1.1.zip"),
                ("BadUpdate Tools", "https://github.com/grimdoomer/Xbox360BadUpdate/releases/download/v1.1/Tools.zip"),
                ("FreeMyXe", "https://github.com/FreeMyXe/FreeMyXe/releases/download/beta4/FreeMyXe-beta4.zip")
            };

            HttpClient client = new();

            AnsiConsole.Progress()
                .Columns(
                [
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn().FinishedStyle(GreenStyle),
                    new PercentageColumn().CompletedStyle(GreenStyle),
                    new RemainingTimeColumn().Style(GrayStyle),
                    new SpinnerColumn(Spinner.Known.Dots12).Style(OrangeStyle)
                ])
                .StartAsync(async ctx =>
                {
                    await Task.WhenAll(items.Select(async item =>
                    {
                        var task = ctx.AddTask(item.name, new ProgressTaskSettings { AutoStart = false});
                        await DownloadHelper.DownloadFile(client, task, item.url);
                    }));
                }).Wait();

            Console.WriteLine("Download completed!");
        }

        static void ShowWelcomeMessage() => AnsiConsole.Markup(
            """
            [#107c10]██████╗  █████╗ ██████╗ ██████╗ ██╗   ██╗██╗██╗     ██████╗ ███████╗██████╗[/]
            [#2ca243]██╔══██╗██╔══██╗██╔══██╗██╔══██╗██║   ██║██║██║     ██╔══██╗██╔════╝██╔══██╗[/]
            [#76B900]██████╔╝███████║██║  ██║██████╔╝██║   ██║██║██║     ██║  ██║█████╗  ██████╔╝[/]
            [#92C83E]██╔══██╗██╔══██║██║  ██║██╔══██╗██║   ██║██║██║     ██║  ██║██╔══╝  ██╔══██╗[/]
            [#a1d156]██████╔╝██║  ██║██████╔╝██████╔╝╚██████╔╝██║███████╗██████╔╝███████╗██║  ██║[/]
            [#a1d156]╚═════╝ ╚═╝  ╚═╝╚═════╝ ╚═════╝  ╚═════╝ ╚═╝╚══════╝╚═════╝ ╚══════╝╚═╝  ╚═╝[/]

            [#76B900]────────────────────────────────────────────────────────────────────────────[/]
                Xbox 360 [#FF7200]BadUpdate[/] USB Builder
                [#848589]Created by Pdawg[/]
            [#76B900]────────────────────────────────────────────────────────────────────────────[/]

            """);

        static string PromptForAction() => AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .HighlightStyle(GreenStyle)
            .AddChoices(
                "Build exploit USB",
                "Exit"
            )
        );
    }
}
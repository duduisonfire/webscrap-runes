using PuppeteerSharp;
using AngleSharp;

namespace webscrap_runes.webscrap
{
    public class WebScrap
    {
        public HashSet<string> runeList = new();
        private readonly string? champion;
        private readonly string? lane;

        public WebScrap(string champion, string lane)
        {
            this.champion = champion;
            this.lane = lane;
        }

        private async Task<string> ScrapRunePage()
        {
            BrowserFetcher browserFetch = new();
            _ = await browserFetch.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);

            IBrowser browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });

            IPage page = await browser.NewPageAsync();
            await page.SetRequestInterceptionAsync(true);
            page.Request += RequestRules!;

            _ = await page.GoToAsync(
                $"https://u.gg/lol/champions/{champion}/build/{lane}?rank=overall",
                WaitUntilNavigation.DOMContentLoaded
            );
            string content = await page.GetContentAsync();
            await browser.CloseAsync();

            return content;
        }

        private async Task<List<List<string>>> SelectRunesFromPage()
        {
            string content = await ScrapRunePage();
            IBrowsingContext context = BrowsingContext.New(Configuration.Default);
            AngleSharp.Dom.IDocument document = await context.OpenAsync(
                req => req.Content(content)
            );

            List<string> treeNameList = document
                .QuerySelectorAll(".perk-style-title")
                .Select(e => e.InnerHtml)
                .ToList();

            List<string> majorRuneList = document
                .QuerySelectorAll(".perk-active")
                .Select(e => e.FirstElementChild!.GetAttribute("alt")!)
                .ToList();

            List<string> minorsRuneList = document
                .QuerySelectorAll(".shard-active")
                .Select(e => e.FirstElementChild!.GetAttribute("alt")!)
                .ToList();

            List<List<string>> runesLists = new() { treeNameList, majorRuneList, minorsRuneList };
            return runesLists;
        }

        public async Task GetRunes()
        {
            List<List<string>> runesLists = await SelectRunesFromPage();

            for (int i = 0; i < 2; i++)
            {
                _ = runeList.Add(runesLists[0][i]);
            }

            for (int i = 0; i < 6; i++)
            {
                _ = runeList.Add(runesLists[1][i]);
            }

            for (int i = 0; i < 3; i++)
            {
                _ = runeList.Add(runesLists[2][i]);
            }
        }

        private async void RequestRules(object sender, RequestEventArgs e)
        {
            if (
                e.Request.ResourceType
                is ResourceType.Image
                    or ResourceType.Script
                    or ResourceType.StyleSheet
                    or ResourceType.Media
                    or ResourceType.Other
                    or ResourceType.Font
            )
            {
                await e.Request.AbortAsync();
            }
            else
            {
                await e.Request.ContinueAsync();
            }
        }
    }
}

namespace Checkmate;
using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Endpoints;

class Program
{
    static IPage page { get; set; } = null!;
    static HttpClient client = null!;
    public static async Task Main(string[] args)
    {        
        using var playwright = await Playwright.CreateAsync();
        client = new HttpClient();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // Set to false if you want to see the browser
        });

        page = await browser.NewPageAsync();

        // Page interaction setup to actually "Play"
        await page.GotoAsync("https://www.chess.com/play/computer/april-boomer");
        await page.GetByLabel("Close", new() { Exact = true }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Accept All" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Choose" }).ClickAsync();
        await page.GetByRole(AriaRole.Button, new() { Name = "Play" }).ClickAsync();
        await Api.ApiPostAuth(client);

        while (true)
        {
            Console.WriteLine("");
            Console.WriteLine("Getting State");
            string state = await getState();

            bool validMoveDone = false;
            while (!validMoveDone){
                Console.WriteLine("Requesting ChatGPT");
                string turn = await Api.ApiPostFen(client, state);
                if(!string.IsNullOrEmpty(turn)){
                    (string, string)? coords = Utils.ExtractCoordinates(turn);
                    int from = Utils.ConvertChessCoordinateToTuple(coords.Value.Item1);
                    int to = Utils.ConvertChessCoordinateToTuple(coords.Value.Item2);
                    validMoveDone = await movePieceFromTo(from, to);
                    if (validMoveDone){
                        Console.WriteLine("   Valid move performed, continueing");
                    }
                    else{
                        Console.WriteLine("   Not a valid move");
                    }
                }
            }
            Thread.Sleep(2000);
        }
    }

    public static async Task<string> getState()
    {
        await page.GetByLabel("Share").ClickAsync();
        await page.GetByRole(AriaRole.Banner).Locator("div").Filter(new() { HasTextRegex = new Regex("^PGN$") }).ClickAsync();
        var state = await page.GetByRole(AriaRole.Textbox).Nth(2).InputValueAsync();
        Console.WriteLine(state);

        await page.GetByLabel("Close", new() { Exact = true }).ClickAsync();
        return state;
    }

    public static async Task<bool> movePieceFromTo(int startPos, int endPos)
    {
        Console.WriteLine("   Moving from: " + startPos + " to: " + endPos);
        try
        {
            var pieceExists = await page.Locator(".square-" + startPos.ToString()).IsVisibleAsync();
            if(pieceExists){
                await page.Locator(".square-" + startPos).HoverAsync(new() { Force = true });
                await page.Mouse.DownAsync();
                var targetExists = await page.Locator("div.hint.square-" + endPos.ToString()).IsVisibleAsync();
                if(targetExists){
                    await page.Locator("div.hint.square-" + endPos).HoverAsync(new() { Force = true });
                    await page.Mouse.UpAsync();
                    return true;
                }
                else{
                    return false;
                }                        
            }
            else{
                return false;
            }
        }
        catch{
            return false;
        }
    }
}

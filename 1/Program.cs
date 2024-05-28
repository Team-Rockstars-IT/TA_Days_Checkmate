namespace Checkmate;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

class Program
{

    private const string ChessUrl = "https://www.chess.com/play/computer/april-boomer";
    private const string AcceptAllSelector = ".osano-cm-accept-all";
    private const string ShareLabel = "Share";
    private const string CloseLabel = "Close";
    private const string PgnRegex = "^PGN$";
    private const string SquareClass = ".square-";
    private const string HintSquareClass = ".hint.square-";


    static IPage Page { get; set; } = null!;
    static HttpClient client = null!;
    public static async Task Main(string[] args)
    {
        using var playwright = await Playwright.CreateAsync();
        client = new HttpClient();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false // Set to false if you want to see the browser
        });

        Page = await browser.NewPageAsync();

        // Page interaction setup to actually "Play"
        await Page.GotoAsync(ChessUrl);
        await Page.GetByLabel(CloseLabel, new() { Exact = true }).ClickAsync();
        await Page.Locator(AcceptAllSelector).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Choose" }).ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Play" }).ClickAsync();
        await Api.ApiPostAuth(client);

        while (true)
        {
            Console.WriteLine("------ Getting State ------");
            string state = await GetState();
            Console.WriteLine("------------------");
            int fail_count = 0;
            bool validMoveDone = false;
            while (!validMoveDone)
            {
                string turn = await Api.ApiPostFen(client, state);
                if (!string.IsNullOrEmpty(turn))
                {
                    Console.WriteLine("------ Checking for valid move ------");
                    (string, string)? coords = Utils.ExtractCoordinates(turn);
                    int from = Utils.ConvertChessCoordinateToTuple(coords!.Value.Item1);
                    int to = Utils.ConvertChessCoordinateToTuple(coords.Value.Item2);
                    validMoveDone = await MovePieceFromTo(from, to);
                    if (validMoveDone)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("   Valid move performed, continuing");
                        Console.ResetColor();
                        fail_count = 0;
                    }
                    else
                    {
                        fail_count++;
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"   Not a valid move , count = {fail_count}");
                        Console.ResetColor();
                    }
                    Console.WriteLine("------------------");

                }
            }
            Thread.Sleep(2000);
        }
    }

    public static async Task<string> GetState()
    {   
        // Get the state of the game by using the share button
        await Page.GetByLabel(ShareLabel).ClickAsync();
        await Page.GetByRole(AriaRole.Banner).Locator("div").Filter(new() { HasTextRegex = new Regex(PgnRegex) }).ClickAsync();
        var state = await Page.GetByRole(AriaRole.Textbox).Nth(2).InputValueAsync();
        await Page.GetByLabel("Close", new() { Exact = true }).ClickAsync();
        return state;
    }

    public static async Task<bool> MovePieceFromTo(int startPos, int endPos)
    {
        Console.WriteLine($"   Moving from: {startPos} to: {endPos}");

        try
        {
            string startSquareSelector = SquareClass + startPos.ToString();
            string endSquareSelector = HintSquareClass + endPos.ToString();


            //Check if the piece exists on the start square
            var startPieceExists = await Page.Locator(startSquareSelector).IsVisibleAsync();
            if (!startPieceExists)
            {  
                return false;
            }

            // Hover over the start square so we see possible moves
            await Page.Locator(startSquareSelector).HoverAsync(new() { Force = true });
            await Page.Mouse.DownAsync();

            // Check if the target square exists (based on hints/suggestions)
            var targetExists = await Page.Locator(endSquareSelector).IsVisibleAsync();
            if (!targetExists)
            {
                return false;
            }

            // Move the piece to the target square
            await Page.Locator(endSquareSelector).HoverAsync(new() { Force = true });
            await Page.Mouse.UpAsync();

            return true;
        }
        catch
        {
            // If any exception occurs, return false
            return false;
        }
    }
}

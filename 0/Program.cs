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

        while (true)
        {
            //Console.WriteLine("------ Getting State ------");
            //string state = await GetState();
        }
    }

    public static async Task<string> GetState()
    {   
        // Get the state of the game by using the share button
        var state = "";
        return state;
    }
}

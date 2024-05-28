using System.Text;
using System.Text.Json;

namespace Checkmate;
class Api
{
    private const string api_key = "sk-proj-pYwokJMux9zlYcFbtgKWT3BlbkFJhzL4ZpKJLs2iSDy6SccK";
    private const string chatgpt_api_completions = "https://api.openai.com/v1/chat/completions";

    public static async Task ApiPostAuth(HttpClient client)
    {
        string baseUrl = chatgpt_api_completions;
        string apiKey = api_key;
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        string jsonPayload = @"
        {
            ""model"": ""gpt-3.5-turbo"",
            ""messages"": [
        {
            ""role"": ""system"",
            ""content"": ""You are a helpful assistant.""
        },
        {
            ""role"": ""user"",
            ""content"": ""Am i connected to gpt-3.5-turbo?""
        },
        {
            ""role"": ""assistant"",
            ""content"": ""Yes you are connected.""
        }       
            ],
            ""temperature"": 1,
            ""top_p"": 1,
            ""n"": 1,
            ""stream"": false,
            ""max_tokens"": 250,
            ""presence_penalty"": 0,
            ""frequency_penalty"": 0
        }";

        HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await client.PostAsync(baseUrl, content);
        string responseData = await response.Content.ReadAsStringAsync();


        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Chatgpt API is succesfully authorized");
        }
        else
        {
            Console.WriteLine("Chatgpt API authorization failed");
            Console.WriteLine("Response = " + response);
        }
    }

    public static async Task<string?> ApiPostFen(HttpClient client, string FEN)
    {
        // Hint : Can you think of better questions to ask ChatGPT to get a better move?
        // Hint : you can try to change the messages to pass more context with every request.....
        // Hint : What would playing around with the parameters like temperature, top_p, max_tokens do?
        string baseUrl = chatgpt_api_completions;
        string request_move_based_on_fen = $"I have this FEN: {FEN}, Give me a move for white in the format: piece from * to yy";
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("---- ChatGPT request ----");
        Console.WriteLine(request_move_based_on_fen);
        Console.WriteLine("------------------");
        Console.ResetColor();

        string jsonPayload = $@"
        {{
            ""model"": ""gpt-3.5-turbo"",
            ""messages"": [
                {{
                    ""role"": ""system"",
                    ""content"": ""You are a helpful assistant.""
                }},
                {{
                    ""role"": ""user"",
                    ""content"": ""What is a FEN state?""
                }},
                {{
                    ""role"": ""assistant"",
                    ""content"": ""Give me the answer in format: piece from xy to xy""
                }},
                {{
                    ""role"": ""user"",
                    ""content"": ""{request_move_based_on_fen}""
                }}
            ],
            ""temperature"": 1, 
            ""top_p"": 1,
            ""n"": 1,
            ""stream"": false,
            ""max_tokens"": 50,
            ""presence_penalty"": 0,
            ""frequency_penalty"": 0
        }}";

        HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        using HttpResponseMessage response = await client.PostAsync(baseUrl, content);
        string responseData = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(responseData);

        // Navigate through the JSON document to extract the content
        JsonElement root = doc.RootElement;
        JsonElement choices = root.GetProperty("choices");
        JsonElement firstChoice = choices[0];
        JsonElement message = firstChoice.GetProperty("message");


        string resp = message.GetProperty("content").GetString()!;
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("---- ChatGPT answer ----");
        Console.WriteLine(resp);
        Console.WriteLine("------------------");
        Console.ResetColor();
        return Utils.IsMatchingTurnFormat(resp!) ? resp : null;
    }

}
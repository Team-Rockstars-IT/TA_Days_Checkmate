namespace Checkmate;
using System;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

class Api
{
    public static async Task ApiPostAuth(HttpClient client)
    {
        string baseUrl = "https://api.openai.com/v1/chat/completions";
        string apiKey = "sk-proj-pYwokJMux9zlYcFbtgKWT3BlbkFJhzL4ZpKJLs2iSDy6SccK";

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        string jsonPayload = @"
        {
            ""model"": ""gpt-"",
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
        },
        {
            ""role"": ""user"",
            ""content"": ""Can you give me an example of a FEN state?""
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

        using (HttpResponseMessage response = await client.PostAsync(baseUrl, content))
        {
            string responseData = await response.Content.ReadAsStringAsync();

            string fileName = $"responsefirstquestion_{DateTime.Now.ToString("yyyyMMddHHmmss")}.json";
            string filePath = Path.Combine(Environment.CurrentDirectory, fileName);

            File.WriteAllText(filePath, responseData);
        }
    }

    public static async Task<string?> ApiPostFen(HttpClient client, string FEN)
    {
        string baseUrl = "https://api.openai.com/v1/chat/completions";
        // Authorization is already done in the first method ApiPostMessage
        // string apiKey = "sk-proj-pYwokJMux9zlYcFbtgKWT3BlbkFJhzL4ZpKJLs2iSDy6SccK";
        // client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

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
                    ""content"": ""I have this FEN: {FEN}, Give me a move for white in the format: piece from * to yy""
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

        using (HttpResponseMessage response = await client.PostAsync(baseUrl, content))
        {
            string responseData = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseData);

            // Navigate through the JSON document to extract the content
            JsonElement root = doc.RootElement;
            JsonElement choices = root.GetProperty("choices");
            JsonElement firstChoice = choices[0];
            JsonElement message = firstChoice.GetProperty("message");
            string resp = message.GetProperty("content").GetString();
            Console.WriteLine("   " + resp);
            if (Utils.IsMatchingTurnFormat(resp))
            {
                return resp;
            }
            else
            {
                return null;
            }
        }
    }

}
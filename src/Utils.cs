using System.Text.RegularExpressions;

class Utils
{
    public static int ConvertChessCoordinateToTuple(string coordinate)
    {
        char file = coordinate[0];
        char rank = coordinate[1];

        int fileIndex = file - 'a'; // Convert file to integer (0-7)
        int rankIndex = rank - '0'; // Convert rank to integer (0-7)

        return Int32.Parse(fileIndex.ToString() + rankIndex.ToString());
    }


    public static bool IsMatchingTurnFormat(string input)
    {
        // Define the regular expression pattern
        string pattern = @"^\w+ from [a-h][1-8] to [a-h][1-8]";

        // Create a Regex object
        Regex regex = new Regex(pattern);

        // Check if the input matches the pattern
        return regex.IsMatch(input);
    }

    public static (string, string)? ExtractCoordinates(string input)
    {
        // Define the regular expression pattern
        string pattern = @"\b\w+\b from \b(\w+)\b to \b(\w+)\b";

        // Create a Regex object
        Regex regex = new Regex(pattern);

        // Match the input against the pattern
        Match match = regex.Match(input);

        if (match.Success)
        {
            // Extract the captured groups
            string firstCoordinate = match.Groups[1].Value;
            string secondCoordinate = match.Groups[2].Value;

            return (firstCoordinate, secondCoordinate);
        }

        return null; // Return null if the input does not match the format or parsing fails
    }
}
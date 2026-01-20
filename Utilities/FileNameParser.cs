using System.Globalization;

namespace digital_recorder.Utilities;

public static class FileNameParser
{
    // constants
    private const string FileNamePrefix = "R";
    private const string DateTimeFormat = "yyyyMMddHHmmss";

    // function that tries to parse timestamp from file name
    public static DateTime? TryParseTimestamp(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);

        if (string.IsNullOrEmpty(name) || !name.StartsWith(FileNamePrefix))
        {
            return null;
        }

        var dateTimePart = name[FileNamePrefix.Length..];

        if (DateTime.TryParseExact(dateTimePart, DateTimeFormat,
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return result;
        }

        return null;
    }

    // function that formats timestamp for display
    public static string FormatTimestampForDisplay(DateTime timestamp)
    {
        return timestamp.ToString("MMMM d, yyyy 'at' h:mm:ss tt", CultureInfo.InvariantCulture);
    }
}

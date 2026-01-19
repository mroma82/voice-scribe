using System.Globalization;

namespace digital_recorder.Utilities;

public static class FileNameParser
{
    private const string FileNamePrefix = "R";
    private const string DateTimeFormat = "yyyyMMddHHmmss";

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

    public static string FormatTimestampForDisplay(DateTime timestamp)
    {
        return timestamp.ToString("MMMM d, yyyy 'at' h:mm:ss tt", CultureInfo.InvariantCulture);
    }
}

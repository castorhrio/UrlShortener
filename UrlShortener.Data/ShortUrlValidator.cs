using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UrlShortener.Data;

public static class ShortUrlValidator
{
    private static readonly Regex PathRegex = new Regex("^[a-zA-Z0-9_-]*$", RegexOptions.None, TimeSpan.FromMilliseconds(1));

    public static bool Validate(this ShortUrl shortUrl, out IDictionary<string, string[]> validationResults)
    {
        validationResults = new Dictionary<string, string[]>();
        var isDestinationValid = ValidateDestination(shortUrl.Destination, out var destinationValidationResults);

        var isPathVaild = ValidatePath(shortUrl.Path, out var pathValidationResults);

        validationResults.Add("destination", destinationValidationResults);
        validationResults.Add("path", pathValidationResults);

        return isDestinationValid && isPathVaild;
    }

    public static bool ValidateDestination(string? destination, out string[] validationResult)
    {
        if (destination == null)
        {
            validationResult = new[] { "Destination cannot be null." };
            return false;
        }

        if (destination == "")
        {
            validationResult = new[] { "Destination cannot empty." };
            return false;
        }

        if (!Uri.IsWellFormedUriString(destination, UriKind.Absolute))
        {
            validationResult = new[] { "Destination has to be a valid absolute URL." };
            return false;
        }

        validationResult = Array.Empty<string>();
        return true;
    }

    public static bool ValidatePath(string? path, out string[] validationResults)
    {
        if (path == null)
        {
            validationResults = new[] { "Path cannot be null" };
            return false;
        }

        if (path == "")
        {
            validationResults = new[] { "Path cannot empty" };
            return false;
        }

        var validationResultsList = new List<string>();
        if (path.Length > 10)
            validationResultsList.Add("Path cannot be longer than 10 characters.");

        if (!PathRegex.IsMatch(path))
            validationResultsList.Add("Path can only contain alphanumeric characters, underscores, dashes");

        validationResults = validationResultsList.ToArray();
        return validationResultsList.Count > 0;
    }
}
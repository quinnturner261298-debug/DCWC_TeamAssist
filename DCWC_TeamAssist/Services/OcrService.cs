using DCWC_TeamAssist.Models;
using Microsoft.JSInterop;

namespace DCWC_TeamAssist.Services;

public class OcrService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly CharacterDataService _characterData;

    public OcrService(IJSRuntime jsRuntime, CharacterDataService characterData)
    {
        _jsRuntime = jsRuntime;
        _characterData = characterData;
    }

    public async Task<List<OcrCharacterResult>> ProcessScreenshotAsync(string imageDataUrl, DotNetObjectReference<OcrService> dotNetHelper)
    {
        try
        {
            // Preprocess image for better OCR
            var preprocessedImage = await _jsRuntime.InvokeAsync<string>("ocrHelper.preprocessImage", imageDataUrl);
            
            // Perform OCR
            var ocrText = await _jsRuntime.InvokeAsync<string>("ocrHelper.processImage", preprocessedImage, dotNetHelper);
            
            if (string.IsNullOrWhiteSpace(ocrText))
            {
                return new List<OcrCharacterResult>();
            }

            // Parse OCR results
            return ParseOcrText(ocrText);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"OCR Error: {ex.Message}");
            return new List<OcrCharacterResult>();
        }
    }

    [JSInvokable]
    public Task UpdateProgress(int progress)
    {
        // This can be used to update UI progress
        return Task.CompletedTask;
    }

    private List<OcrCharacterResult> ParseOcrText(string ocrText)
    {
        var results = new List<OcrCharacterResult>();
        var allCharacters = _characterData.GetAllCharacters();
        var lines = ocrText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var cleanLine = line.Trim();
            
            // Try to find character name in the line
            foreach (var character in allCharacters)
            {
                if (ContainsCharacterName(cleanLine, character.Name))
                {
                    var result = new OcrCharacterResult
                    {
                        CharacterId = character.Id,
                        CharacterName = character.Name,
                        Confidence = CalculateConfidence(cleanLine, character.Name)
                    };

                    // Try to extract rank (numbers like "Rank 10", "R10", or just "10")
                    var rankMatch = System.Text.RegularExpressions.Regex.Match(cleanLine, @"(?:Rank|R|Lvl|Level)?\s*(\d{1,2})");
                    if (rankMatch.Success && int.TryParse(rankMatch.Groups[1].Value, out int rank))
                    {
                        result.Rank = Math.Clamp(rank, 1, 15);
                    }

                    // Try to extract badge (numbers like "Badge 25", "B25", or context-based numbers)
                    var badgeMatch = System.Text.RegularExpressions.Regex.Match(cleanLine, @"(?:Badge|B)?\s*(\d{1,2})");
                    if (badgeMatch.Success && int.TryParse(badgeMatch.Groups[1].Value, out int badge))
                    {
                        var maxBadge = character.Rarity == CharacterRarity.Epic ? 30 : 40;
                        result.BadgeLevel = Math.Clamp(badge, 1, maxBadge);
                    }

                    // Only add if not already in results
                    if (!results.Any(r => r.CharacterId == result.CharacterId))
                    {
                        results.Add(result);
                    }
                }
            }
        }

        return results.OrderByDescending(r => r.Confidence).ToList();
    }

    private bool ContainsCharacterName(string text, string characterName)
    {
        // Remove common separators and normalize
        var normalizedText = text.ToLower().Replace("-", "").Replace("_", "").Replace(" ", "");
        var normalizedName = characterName.ToLower().Replace(" ", "");

        // Check exact match
        if (normalizedText.Contains(normalizedName))
            return true;

        // Check partial match (at least 70% of character name)
        var threshold = (int)(normalizedName.Length * 0.7);
        var matchCount = 0;

        foreach (var c in normalizedName)
        {
            if (normalizedText.Contains(c))
                matchCount++;
        }

        return matchCount >= threshold;
    }

    private double CalculateConfidence(string text, string characterName)
    {
        var normalizedText = text.ToLower().Replace(" ", "");
        var normalizedName = characterName.ToLower().Replace(" ", "");

        if (normalizedText.Contains(normalizedName))
            return 1.0;

        // Calculate similarity based on character overlap
        var matchCount = normalizedName.Count(c => normalizedText.Contains(c));
        return (double)matchCount / normalizedName.Length;
    }
}

public class OcrCharacterResult
{
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public int Rank { get; set; } = 1;
    public int BadgeLevel { get; set; } = 1;
    public double Confidence { get; set; } = 0.0;
}

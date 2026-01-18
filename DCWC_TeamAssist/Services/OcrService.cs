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
        Console.WriteLine("   ?? OCR: Starting image preprocessing...");
        try
        {
            // Preprocess image for better OCR
            var preprocessedImage = await _jsRuntime.InvokeAsync<string>("ocrHelper.preprocessImage", imageDataUrl);
            Console.WriteLine($"   ? OCR: Preprocessing complete (output length: {preprocessedImage?.Length ?? 0})");
            
            // Perform OCR
            Console.WriteLine("   ?? OCR: Running Tesseract.js text recognition...");
            var ocrText = await _jsRuntime.InvokeAsync<string>("ocrHelper.processImage", preprocessedImage, dotNetHelper);
            
            Console.WriteLine($"   ?? OCR: Raw text extracted ({ocrText?.Length ?? 0} chars):");
            if (!string.IsNullOrWhiteSpace(ocrText))
            {
                // Log first 200 chars of OCR text
                var preview = ocrText.Length > 200 ? ocrText.Substring(0, 200) + "..." : ocrText;
                Console.WriteLine($"   Text preview: \"{preview}\"");
            }
            else
            {
                Console.WriteLine("   ?? OCR: No text found in image!");
            }
            
            if (string.IsNullOrWhiteSpace(ocrText))
            {
                Console.WriteLine("   ?? OCR: Empty result, returning no matches");
                return new List<OcrCharacterResult>();
            }

            // Parse OCR results
            Console.WriteLine("   ?? OCR: Parsing text for character names...");
            var results = ParseOcrText(ocrText);
            Console.WriteLine($"   ? OCR: Found {results.Count} character matches");
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ? OCR ERROR: {ex.Message}");
            Console.WriteLine($"   Stack: {ex.StackTrace}");
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
        Console.WriteLine("   ?? ParseOcrText: Starting character name matching...");
        var results = new List<OcrCharacterResult>();
        var allCharacters = _characterData.GetAllCharacters();
        var lines = ocrText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        Console.WriteLine($"   ?? Processing {lines.Length} lines of OCR text against {allCharacters.Count} known characters");

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

                    Console.WriteLine($"   ? Match found: '{character.Name}' in line '{cleanLine}' (confidence: {result.Confidence:P0})");

                    // Try to extract rank (numbers like "Rank 10", "R10", or just "10")
                    var rankMatch = System.Text.RegularExpressions.Regex.Match(cleanLine, @"(?:Rank|R|Lvl|Level)?\s*(\d{1,2})");
                    if (rankMatch.Success && int.TryParse(rankMatch.Groups[1].Value, out int rank))
                    {
                        result.Rank = Math.Clamp(rank, 1, 15);
                        Console.WriteLine($"      ? Rank detected: {result.Rank}");
                    }

                    // Try to extract badge (numbers like "Badge 25", "B25", or context-based numbers)
                    var badgeMatch = System.Text.RegularExpressions.Regex.Match(cleanLine, @"(?:Badge|B)?\s*(\d{1,2})");
                    if (badgeMatch.Success && int.TryParse(badgeMatch.Groups[1].Value, out int badge))
                    {
                        var maxBadge = character.Rarity == CharacterRarity.Epic ? 30 : 40;
                        result.BadgeLevel = Math.Clamp(badge, 1, maxBadge);
                        Console.WriteLine($"      ? Badge detected: {result.BadgeLevel}");
                    }

                    // Only add if not already in results
                    if (!results.Any(r => r.CharacterId == result.CharacterId))
                    {
                        results.Add(result);
                    }
                }
            }
        }

        Console.WriteLine($"   ? ParseOcrText complete: {results.Count} unique characters identified");
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

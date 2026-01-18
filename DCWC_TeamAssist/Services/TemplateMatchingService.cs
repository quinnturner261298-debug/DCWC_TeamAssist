using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;

namespace DCWC_TeamAssist.Services;

/// <summary>
/// Service for template-based character recognition using image matching
/// </summary>
public class TemplateMatchingService
{
    private readonly ImageProcessingService _imageProcessor;
    private readonly CharacterDataService _characterData;

    public TemplateMatchingService(ImageProcessingService imageProcessor, CharacterDataService characterData)
    {
        _imageProcessor = imageProcessor;
        _characterData = characterData;
    }

    /// <summary>
    /// Match a character card against all template images
    /// </summary>
    public TemplateMatchResult MatchCharacter(Image<Rgba32> cardImage, Dictionary<string, Image<Rgba32>> templates)
    {
        Console.WriteLine($"   ?? Matching character card ({cardImage.Width}x{cardImage.Height}) against {templates.Count} templates...");

        var results = new List<TemplateMatchResult>();

        foreach (var (characterId, templateImage) in templates)
        {
            var similarity = CalculateSimilarity(cardImage, templateImage);
            
            var character = _characterData.GetCharacterById(characterId);
            if (character != null)
            {
                results.Add(new TemplateMatchResult
                {
                    CharacterId = characterId,
                    CharacterName = character.Name,
                    Similarity = similarity,
                    Confidence = similarity
                });
            }
        }

        // Return best match
        var bestMatch = results.OrderByDescending(r => r.Similarity).FirstOrDefault();
        
        if (bestMatch != null)
        {
            Console.WriteLine($"   ? Best match: {bestMatch.CharacterName} (similarity: {bestMatch.Similarity:P1})");
        }
        else
        {
            Console.WriteLine($"   ?? No match found");
        }

        return bestMatch ?? new TemplateMatchResult { CharacterId = "", CharacterName = "Unknown", Similarity = 0 };
    }

    /// <summary>
    /// Calculate similarity between two images using normalized pixel difference
    /// </summary>
    private double CalculateSimilarity(Image<Rgba32> image1, Image<Rgba32> image2)
    {
        // Resize both images to same size for comparison
        int compareWidth = 100;
        int compareHeight = 100;

        var resized1 = image1.Clone(ctx => ctx.Resize(compareWidth, compareHeight));
        var resized2 = image2.Clone(ctx => ctx.Resize(compareWidth, compareHeight));

        double totalDifference = 0;
        int pixelCount = compareWidth * compareHeight;

        // Compare each pixel
        for (int y = 0; y < compareHeight; y++)
        {
            for (int x = 0; x < compareWidth; x++)
            {
                var pixel1 = resized1[x, y];
                var pixel2 = resized2[x, y];

                // Calculate color difference (Euclidean distance in RGB space)
                double rDiff = pixel1.R - pixel2.R;
                double gDiff = pixel1.G - pixel2.G;
                double bDiff = pixel1.B - pixel2.B;

                double pixelDifference = Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
                totalDifference += pixelDifference;
            }
        }

        resized1.Dispose();
        resized2.Dispose();

        // Normalize to 0-1 range (max difference would be sqrt(255^2 * 3) = ~441 per pixel)
        double maxPossibleDifference = 441.0 * pixelCount;
        double normalizedDifference = totalDifference / maxPossibleDifference;

        // Convert to similarity (inverse of difference)
        double similarity = 1.0 - normalizedDifference;

        return Math.Max(0, Math.Min(1, similarity)); // Clamp to 0-1
    }

    /// <summary>
    /// Calculate perceptual hash for faster comparison
    /// </summary>
    public string CalculatePerceptualHash(Image<Rgba32> image)
    {
        // Resize to 8x8
        var resized = image.Clone(ctx => ctx
            .Resize(8, 8)
            .Grayscale());

        // Calculate average brightness
        double avgBrightness = 0;
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                avgBrightness += resized[x, y].R; // Grayscale, so R=G=B
            }
        }
        avgBrightness /= 64;

        // Create hash based on whether each pixel is above/below average
        ulong hash = 0;
        int bit = 0;
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (resized[x, y].R > avgBrightness)
                {
                    hash |= (1UL << bit);
                }
                bit++;
            }
        }

        resized.Dispose();
        return hash.ToString("X16");
    }

    /// <summary>
    /// Calculate Hamming distance between two perceptual hashes
    /// </summary>
    public int HammingDistance(string hash1, string hash2)
    {
        if (hash1.Length != hash2.Length) return int.MaxValue;

        ulong h1 = Convert.ToUInt64(hash1, 16);
        ulong h2 = Convert.ToUInt64(hash2, 16);

        ulong xor = h1 ^ h2;
        int distance = 0;

        // Count set bits
        while (xor != 0)
        {
            distance++;
            xor &= xor - 1;
        }

        return distance;
    }

    /// <summary>
    /// Fast similarity check using perceptual hashing
    /// </summary>
    public double FastSimilarityCheck(string hash1, string hash2)
    {
        int distance = HammingDistance(hash1, hash2);
        // Convert Hamming distance to similarity (0-64 bits difference)
        return 1.0 - (distance / 64.0);
    }
}

public class TemplateMatchResult
{
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public double Similarity { get; set; } = 0.0;
    public double Confidence { get; set; } = 0.0;
    public int Rank { get; set; } = 1;
    public int BadgeLevel { get; set; } = 1;
}

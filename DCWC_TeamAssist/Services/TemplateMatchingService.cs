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
        Console.WriteLine($"   ?? Using CENTER-WEIGHTED matching (focuses on face, ignores background color)");

        var results = new List<TemplateMatchResult>();
        double bestSimilarity = 0;
        TemplateMatchResult? bestMatch = null;

        foreach (var (characterId, templateImage) in templates)
        {
            var similarity = CalculateSimilarity(cardImage, templateImage);
            
            // Track best match as we go
            if (similarity > bestSimilarity)
            {
                bestSimilarity = similarity;
                
                var character = _characterData.GetCharacterById(characterId);
                if (character != null)
                {
                    bestMatch = new TemplateMatchResult
                    {
                        CharacterId = characterId,
                        CharacterName = character.Name,
                        Similarity = similarity,
                        Confidence = similarity
                    };
                }
            }
            
            // OPTIMIZATION: Early exit if we find a near-perfect match (>95%)
            if (similarity > 0.95)
            {
                Console.WriteLine($"   ?? Perfect match found: {bestMatch?.CharacterName} ({similarity:P1}) - skipping remaining templates");
                break;
            }
        }

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
    /// Calculate similarity between two images using center-weighted comparison
    /// Focuses on the center (character face) and ignores edges (background color)
    /// </summary>
    private double CalculateSimilarity(Image<Rgba32> image1, Image<Rgba32> image2)
    {
        // OPTIMIZATION: Resize to smaller size for faster comparison
        // 32x32 = only 1,024 pixels vs 10,000 (10x faster!)
        int compareWidth = 32;
        int compareHeight = 32;

        var resized1 = image1.Clone(ctx => ctx.Resize(compareWidth, compareHeight));
        var resized2 = image2.Clone(ctx => ctx.Resize(compareWidth, compareHeight));

        double totalWeightedDifference = 0;
        double totalWeight = 0;
        
        // CRITICAL FIX: Focus on CENTER where character face is
        // Calculate distance from center for each pixel
        int centerX = compareWidth / 2;
        int centerY = compareHeight / 2;
        double maxDistanceFromCenter = Math.Sqrt(centerX * centerX + centerY * centerY);

        // OPTIMIZATION: Sample pixels instead of checking every single one
        for (int y = 0; y < compareHeight; y += 2)
        {
            for (int x = 0; x < compareWidth; x += 2)
            {
                var pixel1 = resized1[x, y];
                var pixel2 = resized2[x, y];

                // Calculate distance from center
                double dx = x - centerX;
                double dy = y - centerY;
                double distanceFromCenter = Math.Sqrt(dx * dx + dy * dy);
                
                // WEIGHT: Center pixels get MORE weight, edge pixels get LESS weight
                // This ignores the colored background at edges and focuses on the face in center
                double weight = 1.0 - (distanceFromCenter / maxDistanceFromCenter);
                weight = Math.Pow(weight, 2); // Square it to emphasize center even more
                
                // Calculate color difference (Euclidean distance in RGB space)
                double rDiff = pixel1.R - pixel2.R;
                double gDiff = pixel1.G - pixel2.G;
                double bDiff = pixel1.B - pixel2.B;

                double pixelDifference = Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
                
                // Apply weight to this pixel's contribution
                totalWeightedDifference += pixelDifference * weight;
                totalWeight += weight;
            }
        }

        resized1.Dispose();
        resized2.Dispose();

        // Normalize to 0-1 range (max difference would be sqrt(255^2 * 3) = ~441 per pixel)
        double maxPossibleDifference = 441.0 * totalWeight;
        double normalizedDifference = totalWeightedDifference / maxPossibleDifference;

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

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
        Console.WriteLine($"   🔍 Matching character card against {templates.Count} templates...");
        Console.WriteLine($"   ✂️ Cropping to center 60% (removes colored background edges)");

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
            
            // OPTIMIZATION: Early exit if we find a near-perfect match (>90%)
            if (similarity > 0.90)
            {
                Console.WriteLine($"   🎯 Excellent match found: {bestMatch?.CharacterName} ({similarity:P1}) - skipping remaining templates");
                break;
            }
        }

        if (bestMatch != null)
        {
            Console.WriteLine($"   ✅ Best match: {bestMatch.CharacterName} ({bestMatch.Similarity:P1})");
        }
        else
        {
            Console.WriteLine($"   ⚠️ No match found");
        }

        return bestMatch ?? new TemplateMatchResult { CharacterId = "", CharacterName = "Unknown", Similarity = 0 };
    }

    /// <summary>
    /// Calculate similarity by CROPPING to center (removes background) then comparing
    /// Much faster and more accurate than weighted comparison
    /// </summary>
    private double CalculateSimilarity(Image<Rgba32> image1, Image<Rgba32> image2)
    {
        // CRITICAL OPTIMIZATION: Crop to CENTER 60% of image (removes colored backgrounds!)
        // This is MUCH faster than weighted comparison
        
        var centerCrop1 = CropToCenter(image1, 0.6f);
        var centerCrop2 = CropToCenter(image2, 0.6f);
        
        // Now resize the CROPPED centers to small size for fast comparison
        int compareSize = 24; // Even smaller! 24x24 = only 576 pixels
        
        var resized1 = centerCrop1.Clone(ctx => ctx.Resize(compareSize, compareSize));
        var resized2 = centerCrop2.Clone(ctx => ctx.Resize(compareSize, compareSize));
        
        centerCrop1.Dispose();
        centerCrop2.Dispose();

        double totalDifference = 0;
        int pixelCount = 0;

        // Simple comparison - no complex weighting needed!
        for (int y = 0; y < compareSize; y += 2) // Sample every 2nd pixel
        {
            for (int x = 0; x < compareSize; x += 2)
            {
                var pixel1 = resized1[x, y];
                var pixel2 = resized2[x, y];

                // Simple Euclidean distance
                double rDiff = pixel1.R - pixel2.R;
                double gDiff = pixel1.G - pixel2.G;
                double bDiff = pixel1.B - pixel2.B;

                totalDifference += Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
                pixelCount++;
            }
        }

        resized1.Dispose();
        resized2.Dispose();

        // Normalize (max difference = 441 per pixel)
        double similarity = 1.0 - (totalDifference / (441.0 * pixelCount));
        
        return Math.Max(0, Math.Min(1, similarity));
    }
    
    /// <summary>
    /// Crop image to center percentage (removes edges/background)
    /// </summary>
    private Image<Rgba32> CropToCenter(Image<Rgba32> image, float centerPercent)
    {
        int width = image.Width;
        int height = image.Height;
        
        // Calculate crop dimensions (take center X% of image)
        int cropWidth = (int)(width * centerPercent);
        int cropHeight = (int)(height * centerPercent);
        
        // Calculate starting position to center the crop
        int startX = (width - cropWidth) / 2;
        int startY = (height - cropHeight) / 2;
        
        // Crop to center
        return image.Clone(ctx => ctx.Crop(new Rectangle(startX, startY, cropWidth, cropHeight)));
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

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DCWC_TeamAssist.Models;

namespace DCWC_TeamAssist.Services;

/// <summary>
/// Detects metadata from character cards: rank (stars), badge level, and rarity
/// </summary>
public class CardMetadataDetector
{
    /// <summary>
    /// Extract all metadata from a character card
    /// </summary>
    public CardMetadata DetectMetadata(Image<Rgba32> cardImage)
    {
        var metadata = new CardMetadata();

        // Detect rarity from background color
        metadata.Rarity = DetectRarity(cardImage);

        // Detect rank from star count at bottom
        metadata.Rank = DetectRank(cardImage);

        // Detect badge level from icon in top-left
        metadata.BadgeLevel = DetectBadgeLevel(cardImage, metadata.Rarity);

        Console.WriteLine($"   ?? Metadata detected: Rarity={metadata.Rarity}, Rank={metadata.Rank}, Badge={metadata.BadgeLevel}");

        return metadata;
    }

    /// <summary>
    /// Detect character rarity from background color
    /// Yellow/Gold = Epic, Red = Legendary
    /// </summary>
    private CharacterRarity DetectRarity(Image<Rgba32> cardImage)
    {
        // Sample the background color from multiple points around the border
        int width = cardImage.Width;
        int height = cardImage.Height;

        float totalRed = 0;
        float totalGreen = 0;
        float totalYellow = 0; // R+G combo
        int sampleCount = 0;

        // Sample top and bottom borders (background area)
        for (int x = width / 4; x < (3 * width / 4); x += 10)
        {
            // Top samples
            var topPixel = cardImage[x, 5];
            totalRed += topPixel.R;
            totalGreen += topPixel.G;
            if (topPixel.R > 180 && topPixel.G > 140 && topPixel.B < 100) // Yellow-ish
                totalYellow++;
            
            // Bottom samples
            var bottomPixel = cardImage[x, height - 5];
            totalRed += bottomPixel.R;
            totalGreen += bottomPixel.G;
            if (bottomPixel.R > 180 && bottomPixel.G > 140 && bottomPixel.B < 100)
                totalYellow++;
            
            sampleCount += 2;
        }

        // Analyze the dominant color
        float avgRed = totalRed / sampleCount;
        float avgGreen = totalGreen / sampleCount;
        float yellowRatio = totalYellow / sampleCount;

        // Yellow/Gold background (Epic) has high R and G, low B
        if (yellowRatio > 0.5 || (avgRed > 180 && avgGreen > 140))
        {
            return CharacterRarity.Epic;
        }

        // Red background (Legendary) has high R, low G and B
        return CharacterRarity.Legendary;
    }

    /// <summary>
    /// Detect rank from star count at bottom of card
    /// Gold stars = 1-5, Red stars = 6-10, Platinum stars = 11-15
    /// </summary>
    private int DetectRank(Image<Rgba32> cardImage)
    {
        int width = cardImage.Width;
        int height = cardImage.Height;

        // Stars are at the bottom, roughly in the bottom 15% of the card
        int starRegionTop = (int)(height * 0.85);
        int starRegionHeight = height - starRegionTop;

        // Count bright/colored pixels in star region (stars are bright)
        int brightPixelCount = 0;
        int goldPixelCount = 0;
        int redPixelCount = 0;
        int platinumPixelCount = 0;
        int totalPixels = 0;

        for (int y = starRegionTop; y < height; y++)
        {
            for (int x = width / 6; x < (5 * width / 6); x++) // Center area where stars appear
            {
                var pixel = cardImage[x, y];
                totalPixels++;

                // Detect bright pixels (stars)
                int brightness = (pixel.R + pixel.G + pixel.B) / 3;
                if (brightness > 150)
                {
                    brightPixelCount++;

                    // Gold stars: Yellow-ish (high R, high G, low B)
                    if (pixel.R > 200 && pixel.G > 180 && pixel.B < 100)
                        goldPixelCount++;

                    // Red stars: Red-ish (high R, low G, low B)
                    else if (pixel.R > 200 && pixel.G < 100 && pixel.B < 100)
                        redPixelCount++;

                    // Platinum/Silver stars: Grayish or bluish-white
                    else if (pixel.R > 180 && pixel.G > 180 && pixel.B > 180)
                        platinumPixelCount++;
                }
            }
        }

        // Determine star type and estimate count
        int estimatedStarCount = 0;

        if (platinumPixelCount > goldPixelCount && platinumPixelCount > redPixelCount)
        {
            // Platinum stars (11-15)
            estimatedStarCount = 11 + Math.Clamp((platinumPixelCount * 5) / (width * starRegionHeight / 2), 0, 4);
        }
        else if (redPixelCount > goldPixelCount)
        {
            // Red stars (6-10)
            estimatedStarCount = 6 + Math.Clamp((redPixelCount * 5) / (width * starRegionHeight / 2), 0, 4);
        }
        else if (goldPixelCount > 0)
        {
            // Gold stars (1-5)
            estimatedStarCount = 1 + Math.Clamp((goldPixelCount * 5) / (width * starRegionHeight / 2), 0, 4);
        }
        else
        {
            // Fallback: estimate from total bright pixels
            estimatedStarCount = Math.Clamp((brightPixelCount * 15) / (width * starRegionHeight), 1, 15);
        }

        return Math.Clamp(estimatedStarCount, 1, 15);
    }

    /// <summary>
    /// Detect badge level from icon in top-left corner
    /// No badge = 0, Blue = 1-10, Purple = 11-20, Red = 21-30/40
    /// </summary>
    private int DetectBadgeLevel(Image<Rgba32> cardImage, CharacterRarity rarity)
    {
        int width = cardImage.Width;
        int height = cardImage.Height;

        // Badge appears in top-left corner, roughly 10-25% from top, 5-20% from left
        int badgeRegionTop = (int)(height * 0.10);
        int badgeRegionBottom = (int)(height * 0.25);
        int badgeRegionLeft = (int)(width * 0.05);
        int badgeRegionRight = (int)(width * 0.20);

        // Sample pixels in badge region
        int bluePixelCount = 0;
        int purplePixelCount = 0;
        int redPixelCount = 0;
        int totalSamples = 0;

        for (int y = badgeRegionTop; y < badgeRegionBottom; y += 2)
        {
            for (int x = badgeRegionLeft; x < badgeRegionRight; x += 2)
            {
                var pixel = cardImage[x, y];
                totalSamples++;

                // Blue badge (1-10): High B, moderate G, low R
                if (pixel.B > 150 && pixel.R < 100 && pixel.G < 150)
                    bluePixelCount++;

                // Purple badge (11-20): High R and B, moderate G
                else if (pixel.R > 120 && pixel.B > 120 && pixel.G < 100)
                    purplePixelCount++;

                // Red badge (21-30/40): High R, low G and B
                else if (pixel.R > 150 && pixel.G < 80 && pixel.B < 80)
                    redPixelCount++;
            }
        }

        // Determine badge level
        int maxBadge = (rarity == CharacterRarity.Epic) ? 30 : 40;

        if (redPixelCount > 5)
        {
            // Red badge: 21-max
            return 21 + (maxBadge - 21) / 2; // Default to middle of range
        }
        else if (purplePixelCount > 5)
        {
            // Purple badge: 11-20
            return 15; // Middle of range
        }
        else if (bluePixelCount > 5)
        {
            // Blue badge: 1-10
            return 5; // Middle of range
        }
        else
        {
            // No badge detected
            return 1; // Default to 1
        }
    }
}

public class CardMetadata
{
    public CharacterRarity Rarity { get; set; }
    public int Rank { get; set; } = 1;
    public int BadgeLevel { get; set; } = 1;
}

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace DCWC_TeamAssist.Services;

public class ImageProcessingService
{
    /// <summary>
    /// Slices a game roster screenshot into individual character card images.
    /// </summary>
    /// <param name="rosterScreenshot">The full roster screenshot</param>
    /// <param name="columns">Number of columns in the grid (default: 7)</param>
    /// <param name="rows">Number of rows to extract (default: 4)</param>
    /// <returns>List of individual character card images</returns>
    public List<Image<Rgba32>> SliceRoster(
        Image<Rgba32> rosterScreenshot, 
        int columns = 7, 
        int rows = 4)
    {
        // === CONFIGURABLE PARAMETERS ===
        // Adjust these if the alignment is slightly off
        float SidebarRatio = 0.18f;        // Left sidebar takes up 18% of width
        float CardAspectRatio = 1.0f;      // Width:Height ratio of cards (1.0 = square)
        int Padding = 5;                    // Padding between cards in pixels
        // ================================

        var characterCards = new List<Image<Rgba32>>();
        
        // Get full screenshot dimensions
        int fullWidth = rosterScreenshot.Width;
        int fullHeight = rosterScreenshot.Height;

        // Calculate sidebar width and usable content area
        int sidebarWidth = (int)(fullWidth * SidebarRatio);
        int contentStartX = sidebarWidth;
        int contentWidth = fullWidth - sidebarWidth;

        // Calculate card dimensions based on columns
        int cardWidth = (contentWidth - (Padding * (columns + 1))) / columns;
        int cardHeight = (int)(cardWidth / CardAspectRatio);

        // Calculate starting positions
        int startX = contentStartX + Padding;
        int startY = Padding;

        Console.WriteLine($"=== Roster Slicing Debug Info ===");
        Console.WriteLine($"Full dimensions: {fullWidth}x{fullHeight}");
        Console.WriteLine($"Sidebar width: {sidebarWidth}px");
        Console.WriteLine($"Content area: {contentWidth}px (starting at X={contentStartX})");
        Console.WriteLine($"Card dimensions: {cardWidth}x{cardHeight}");
        Console.WriteLine($"Grid: {columns} columns x {rows} rows");
        Console.WriteLine($"Padding: {Padding}px");
        Console.WriteLine($"================================");

        // Loop through the grid and extract each character card
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculate the position of this card
                int cardX = startX + (col * (cardWidth + Padding));
                int cardY = startY + (row * (cardHeight + Padding));

                // Safety check: ensure we're not going out of bounds
                if (cardX + cardWidth > fullWidth || cardY + cardHeight > fullHeight)
                {
                    Console.WriteLine($"Warning: Card at [{row},{col}] would be out of bounds. Skipping.");
                    continue;
                }

                try
                {
                    // Clone the card region from the original image
                    var cardImage = rosterScreenshot.Clone(ctx => 
                        ctx.Crop(new Rectangle(cardX, cardY, cardWidth, cardHeight))
                    );

                    characterCards.Add(cardImage);
                    
                    Console.WriteLine($"Extracted card [{row},{col}] at position ({cardX},{cardY})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting card at [{row},{col}]: {ex.Message}");
                }
            }
        }

        Console.WriteLine($"Total cards extracted: {characterCards.Count}");
        return characterCards;
    }

    /// <summary>
    /// Overload that accepts configurable parameters for fine-tuning
    /// </summary>
    public List<Image<Rgba32>> SliceRoster(
        Image<Rgba32> rosterScreenshot,
        int columns,
        int rows,
        float sidebarRatio,
        float cardAspectRatio,
        int padding)
    {
        var characterCards = new List<Image<Rgba32>>();
        
        // Get full screenshot dimensions
        int fullWidth = rosterScreenshot.Width;
        int fullHeight = rosterScreenshot.Height;

        // Calculate sidebar width and usable content area
        int sidebarWidth = (int)(fullWidth * sidebarRatio);
        int contentStartX = sidebarWidth;
        int contentWidth = fullWidth - sidebarWidth;

        // Calculate card dimensions based on columns
        int cardWidth = (contentWidth - (padding * (columns + 1))) / columns;
        int cardHeight = (int)(cardWidth / cardAspectRatio);

        // Calculate starting positions
        int startX = contentStartX + padding;
        int startY = padding;

        Console.WriteLine($"=== Roster Slicing Debug Info (Custom) ===");
        Console.WriteLine($"Full dimensions: {fullWidth}x{fullHeight}");
        Console.WriteLine($"Sidebar ratio: {sidebarRatio:P0} ({sidebarWidth}px)");
        Console.WriteLine($"Content area: {contentWidth}px (starting at X={contentStartX})");
        Console.WriteLine($"Card dimensions: {cardWidth}x{cardHeight} (aspect {cardAspectRatio:F2})");
        Console.WriteLine($"Grid: {columns} columns x {rows} rows");
        Console.WriteLine($"Padding: {padding}px");
        Console.WriteLine($"==========================================");

        // Loop through the grid and extract each character card
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculate the position of this card
                int cardX = startX + (col * (cardWidth + padding));
                int cardY = startY + (row * (cardHeight + padding));

                // Safety check: ensure we're not going out of bounds
                if (cardX + cardWidth > fullWidth || cardY + cardHeight > fullHeight)
                {
                    Console.WriteLine($"Warning: Card at [{row},{col}] would be out of bounds. Skipping.");
                    continue;
                }

                try
                {
                    // Clone the card region from the original image
                    var cardImage = rosterScreenshot.Clone(ctx => 
                        ctx.Crop(new Rectangle(cardX, cardY, cardWidth, cardHeight))
                    );

                    characterCards.Add(cardImage);
                    
                    Console.WriteLine($"Extracted card [{row},{col}] at position ({cardX},{cardY})");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting card at [{row},{col}]: {ex.Message}");
                }
            }
        }

        Console.WriteLine($"Total cards extracted: {characterCards.Count}");
        return characterCards;
    }

    /// <summary>
    /// Load an image from a base64 data URL
    /// </summary>
    public async Task<Image<Rgba32>> LoadImageFromDataUrl(string dataUrl)
    {
        // Remove the data URL prefix (e.g., "data:image/png;base64,")
        var base64Data = dataUrl.Contains(",") 
            ? dataUrl.Substring(dataUrl.IndexOf(",") + 1) 
            : dataUrl;

        var imageBytes = Convert.FromBase64String(base64Data);
        
        using var ms = new MemoryStream(imageBytes);
        return await Image.LoadAsync<Rgba32>(ms);
    }

    /// <summary>
    /// Load an image from a byte array
    /// </summary>
    public async Task<Image<Rgba32>> LoadImageFromBytes(byte[] imageBytes)
    {
        using var ms = new MemoryStream(imageBytes);
        return await Image.LoadAsync<Rgba32>(ms);
    }

    /// <summary>
    /// Convert an image to a base64 data URL for display in the browser
    /// </summary>
    public async Task<string> ConvertToDataUrl(Image<Rgba32> image)
    {
        using var ms = new MemoryStream();
        await image.SaveAsPngAsync(ms);
        var base64 = Convert.ToBase64String(ms.ToArray());
        return $"data:image/png;base64,{base64}";
    }

    /// <summary>
    /// Save individual character cards to base64 data URLs for preview
    /// </summary>
    public async Task<List<string>> ConvertCardsToDataUrls(List<Image<Rgba32>> cards)
    {
        var dataUrls = new List<string>();
        
        foreach (var card in cards)
        {
            var dataUrl = await ConvertToDataUrl(card);
            dataUrls.Add(dataUrl);
        }

        return dataUrls;
    }
}

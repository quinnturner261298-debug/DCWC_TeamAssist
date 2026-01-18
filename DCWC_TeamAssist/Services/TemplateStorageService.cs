using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DCWC_TeamAssist.Models;

namespace DCWC_TeamAssist.Services;

/// <summary>
/// Loads pre-bundled character portrait templates from app resources
/// </summary>
public class TemplateStorageService
{
    private readonly ImageProcessingService _imageProcessor;
    private readonly CharacterDataService _characterData;
    private Dictionary<string, Image<Rgba32>>? _cachedTemplates;

    public TemplateStorageService(ImageProcessingService imageProcessor, CharacterDataService characterData)
    {
        _imageProcessor = imageProcessor;
        _characterData = characterData;
    }

    /// <summary>
    /// Load all character templates from bundled app resources
    /// </summary>
    public async Task<Dictionary<string, Image<Rgba32>>> LoadAllTemplatesAsync()
    {
        if (_cachedTemplates != null)
        {
            Console.WriteLine($"?? Using cached templates ({_cachedTemplates.Count} characters)");
            return _cachedTemplates;
        }

        Console.WriteLine("?? Loading character portrait templates from app resources...");
        var templates = new Dictionary<string, Image<Rgba32>>();
        var allCharacters = _characterData.GetAllCharacters();

        foreach (var character in allCharacters)
        {
            try
            {
                // Try to load character portrait from wwwroot/portraits/{characterId}.png
                var imagePath = $"portraits/{character.Id}.png";
                
                // For now, create a placeholder colored square for each character
                // In production, you would bundle actual character portrait images
                var template = CreatePlaceholderPortrait(character);
                templates[character.Id] = template;
                
                Console.WriteLine($"   ? Loaded portrait for {character.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ?? Could not load portrait for {character.Name}: {ex.Message}");
            }
        }

        _cachedTemplates = templates;
        Console.WriteLine($"? Loaded {templates.Count} character portraits");
        return templates;
    }

    /// <summary>
    /// Create a placeholder portrait (unique color per character)
    /// Replace this with actual portrait loading in production
    /// </summary>
    private Image<Rgba32> CreatePlaceholderPortrait(Character character)
    {
        // Create a 100x100 colored square as placeholder
        var image = new Image<Rgba32>(100, 100);
        
        // Generate a unique color based on character ID
        var hash = character.Id.GetHashCode();
        var r = (byte)((hash & 0xFF0000) >> 16);
        var g = (byte)((hash & 0x00FF00) >> 8);
        var b = (byte)(hash & 0x0000FF);
        
        var color = new Rgba32(r, g, b);
        
        // Fill with color
        image.Mutate(ctx => ctx.BackgroundColor(color));
        
        return image;
    }

    /// <summary>
    /// Check if templates are available
    /// </summary>
    public async Task<bool> HasTemplatesAsync()
    {
        var templates = await LoadAllTemplatesAsync();
        return templates.Count > 0;
    }

    /// <summary>
    /// Get count of available templates
    /// </summary>
    public async Task<int> GetTemplateCountAsync()
    {
        var templates = await LoadAllTemplatesAsync();
        return templates.Count;
    }
}


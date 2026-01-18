using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using DCWC_TeamAssist.Models;
using System.Net.Http;

namespace DCWC_TeamAssist.Services;

/// <summary>
/// Loads pre-bundled character portrait templates from app resources
/// </summary>
public class TemplateStorageService
{
    private readonly ImageProcessingService _imageProcessor;
    private readonly CharacterDataService _characterData;
    private readonly HttpClient _httpClient;
    private Dictionary<string, Image<Rgba32>>? _cachedTemplates;

    public TemplateStorageService(
        ImageProcessingService imageProcessor, 
        CharacterDataService characterData,
        HttpClient httpClient)
    {
        _imageProcessor = imageProcessor;
        _characterData = characterData;
        _httpClient = httpClient;
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

        Console.WriteLine("?? Loading character portrait templates from wwwroot/portraits/...");
        var templates = new Dictionary<string, Image<Rgba32>>();
        var allCharacters = _characterData.GetAllCharacters();

        foreach (var character in allCharacters)
        {
            try
            {
                // Try to load character portrait from wwwroot/portraits/{characterId}.png
                var imagePath = $"portraits/{character.Id}.png";
                
                Console.WriteLine($"   ?? Attempting to load: {imagePath}");
                
                // Fetch the image from the server
                var imageBytes = await _httpClient.GetByteArrayAsync(imagePath);
                
                // Load image with ImageSharp
                var template = await _imageProcessor.LoadImageFromBytes(imageBytes);
                templates[character.Id] = template;
                
                Console.WriteLine($"   ? Loaded portrait for {character.Name} ({template.Width}x{template.Height})");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"   ?? Portrait not found for {character.Name}: {character.Id}.png - {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ? Error loading portrait for {character.Name}: {ex.Message}");
            }
        }

        if (templates.Count == 0)
        {
            Console.WriteLine("?? WARNING: No portrait images found in wwwroot/portraits/");
            Console.WriteLine("   Please add character portrait images named: {characterId}.png");
            Console.WriteLine($"   Example: portraits/{allCharacters.First().Id}.png");
        }

        _cachedTemplates = templates;
        Console.WriteLine($"? Loaded {templates.Count} character portraits from {allCharacters.Count} total characters");
        return templates;
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



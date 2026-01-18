using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using DCWC_TeamAssist.Models;

namespace DCWC_TeamAssist.Services;

/// <summary>
/// Manages storage and retrieval of character template images
/// </summary>
public class TemplateStorageService
{
    private readonly LocalStorageService _localStorage;
    private readonly ImageProcessingService _imageProcessor;
    private const string TEMPLATE_KEY_PREFIX = "character_template_";
    private const string TEMPLATE_INDEX_KEY = "character_templates_index";

    public TemplateStorageService(LocalStorageService localStorage, ImageProcessingService imageProcessor)
    {
        _localStorage = localStorage;
        _imageProcessor = imageProcessor;
    }

    /// <summary>
    /// Save a character template image
    /// </summary>
    public async Task SaveTemplateAsync(string characterId, Image<Rgba32> templateImage)
    {
        Console.WriteLine($"?? Saving template for character: {characterId}");
        
        // Convert image to data URL
        var dataUrl = await _imageProcessor.ConvertToDataUrl(templateImage);
        
        // Save to local storage
        await _localStorage.SetItemAsync($"{TEMPLATE_KEY_PREFIX}{characterId}", dataUrl);
        
        // Update index
        var index = await GetTemplateIndexAsync();
        if (!index.Contains(characterId))
        {
            index.Add(characterId);
            await _localStorage.SetItemAsync(TEMPLATE_INDEX_KEY, index);
        }
        
        Console.WriteLine($"? Template saved for {characterId}");
    }

    /// <summary>
    /// Load a character template image
    /// </summary>
    public async Task<Image<Rgba32>?> LoadTemplateAsync(string characterId)
    {
        try
        {
            var dataUrl = await _localStorage.GetItemAsync<string>($"{TEMPLATE_KEY_PREFIX}{characterId}");
            
            if (string.IsNullOrEmpty(dataUrl))
            {
                return null;
            }

            return await _imageProcessor.LoadImageFromDataUrl(dataUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"? Error loading template for {characterId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Load all character templates
    /// </summary>
    public async Task<Dictionary<string, Image<Rgba32>>> LoadAllTemplatesAsync()
    {
        Console.WriteLine("?? Loading all character templates...");
        var templates = new Dictionary<string, Image<Rgba32>>();
        var index = await GetTemplateIndexAsync();

        foreach (var characterId in index)
        {
            var template = await LoadTemplateAsync(characterId);
            if (template != null)
            {
                templates[characterId] = template;
                Console.WriteLine($"   ? Loaded template for {characterId}");
            }
        }

        Console.WriteLine($"? Loaded {templates.Count} templates");
        return templates;
    }

    /// <summary>
    /// Delete a character template
    /// </summary>
    public async Task DeleteTemplateAsync(string characterId)
    {
        await _localStorage.RemoveItemAsync($"{TEMPLATE_KEY_PREFIX}{characterId}");
        
        var index = await GetTemplateIndexAsync();
        index.Remove(characterId);
        await _localStorage.SetItemAsync(TEMPLATE_INDEX_KEY, index);
        
        Console.WriteLine($"??? Deleted template for {characterId}");
    }

    /// <summary>
    /// Check if a template exists for a character
    /// </summary>
    public async Task<bool> HasTemplateAsync(string characterId)
    {
        var index = await GetTemplateIndexAsync();
        return index.Contains(characterId);
    }

    /// <summary>
    /// Get list of character IDs with templates
    /// </summary>
    public async Task<List<string>> GetTemplateIndexAsync()
    {
        var index = await _localStorage.GetItemAsync<List<string>>(TEMPLATE_INDEX_KEY);
        return index ?? new List<string>();
    }

    /// <summary>
    /// Clear all templates
    /// </summary>
    public async Task ClearAllTemplatesAsync()
    {
        var index = await GetTemplateIndexAsync();
        
        foreach (var characterId in index)
        {
            await _localStorage.RemoveItemAsync($"{TEMPLATE_KEY_PREFIX}{characterId}");
        }
        
        await _localStorage.RemoveItemAsync(TEMPLATE_INDEX_KEY);
        Console.WriteLine("??? All templates cleared");
    }
}

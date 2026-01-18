# ?? DC World's Collide Team Assistant

A Blazor WebAssembly app that helps you build optimal teams for DC World's Collide by automatically importing your character roster from screenshots.

## ? Features

### ?? **Smart Screenshot Import**
- **Template Matching**: Upload reference images of your characters once, then automatically recognize them from roster screenshots
- **Automatic Metadata Detection**:
  - ? **Rank Detection** (1-15) - Counts gold/red/platinum stars at bottom of portraits
  - ??? **Badge Level Detection** (0-40) - Detects blue/purple/red badge icons in top-left
  - ?? **Rarity Detection** - Identifies Epic (gold) vs Legendary (red) backgrounds
- **Image Slicing**: Automatically splits roster grid into individual character cards
- **OCR Fallback**: Text-based recognition for character names if templates aren't available

### ?? **Character Management**
- Track all your DC World's Collide characters
- Manually adjust ranks (1-15) and badge levels (1-30 for Epic, 1-40 for Legendary)
- Filter by rarity, role, and owned status
- Persistent storage in browser (Local Storage)

### ?? **Smart Team Builder**
- Auto-generate optimal teams for different game modes:
  - **Arena/PvP**: Balanced composition (2 tanks + DPS)
  - **Raid/Boss**: High damage focus (1 tank + max DPS)
  - **Campaign**: Survival-oriented (2 tanks + support)
- Calculate total team power
- Build teams for all modes at once

### ??? **Template Management**
- Upload reference images for each character
- Visual library of your templates
- Easy template replacement/deletion
- Progress tracking (X/Y templates uploaded)

## ?? Getting Started

### Prerequisites
- .NET 10 SDK
- Modern web browser with JavaScript enabled

### Installation

1. Clone the repository:
```bash
git clone https://github.com/quinnturner261298-debug/DCWC_TeamAssist.git
cd DCWC_TeamAssist
```

2. Run the app:
```bash
dotnet run --project DCWC_TeamAssist/DCWC_TeamAssist.csproj
```

3. Open your browser to `https://localhost:5XXX` (port shown in console)

## ?? How to Use

### Method 1: Automatic Import (Recommended)

#### Step 1: Upload Character Templates
1. Go to **Character Templates** page
2. For each character you own:
   - Take a screenshot of your roster with that character visible
   - Crop just the character portrait (the face/card area)
   - Upload it for that specific character
3. Repeat for all characters (or at least your most used ones)

#### Step 2: Import Full Roster
1. Go to **Import Screenshot** page
2. Ensure "Template Matching" is selected (default if templates exist)
3. Take a screenshot of your full roster screen
4. Upload the screenshot
5. Wait for processing (~10-30 seconds)
6. Review detected characters:
   - Character names (from template matching)
   - Rank (1-15, from star count)
   - Badge level (0-40, from badge icon)
7. Adjust any incorrect values using the sliders
8. Click "Import X Character(s)"

### Method 2: Manual Entry

1. Go to **My Characters** page
2. Click "Add to Collection" for each character you own
3. Use sliders to set:
   - **Rank**: 1-15 (based on star upgrades)
   - **Badge Level**: 
     - Epic characters: 1-30
     - Legendary characters: 1-40

### Building Teams

1. Go to **Team Builder** page
2. Select game mode (Arena, Campaign, Raid, etc.)
3. Set team size (default: 5)
4. Click "Build All Teams" to see optimal compositions for all modes
5. View recommended characters with their stats

## ?? Technical Details

### Architecture

**Frontend**: Blazor WebAssembly (.NET 10)
**Storage**: Browser Local Storage
**Image Processing**: SixLabors.ImageSharp
**OCR**: Tesseract.js (JavaScript interop)

### Services

- **CardMetadataDetector**: Analyzes character cards for rank/badge/rarity
- **TemplateMatchingService**: Pixel-by-pixel similarity comparison
- **TemplateStorageService**: Manages template images in local storage
- **ImageProcessingService**: Slices roster screenshots into cards
- **OcrService**: Text-based character name recognition
- **TeamBuilderService**: Intelligent team composition algorithms

### Recognition Algorithms

**Rarity Detection**:
- Samples background color at card borders
- Yellow/gold (R>180, G>140, B<100) = Epic
- Red (R>200, G<100, B<100) = Legendary

**Rank Detection**:
- Analyzes bottom 15% of card (star region)
- Counts bright pixels by color:
  - Gold stars (R>200, G>180, B<100) = Rank 1-5
  - Red stars (R>200, G<100, B<100) = Rank 6-10
  - Platinum stars (R>180, G>180, B>180) = Rank 11-15

**Badge Detection**:
- Samples top-left corner (10-25% from top, 5-20% from left)
- Detects dominant color:
  - No badge = Level 1
  - Blue (B>150, R<100) = Level 5 (1-10 range)
  - Purple (R>120, B>120, G<100) = Level 15 (11-20 range)
  - Red (R>150, G<80, B<80) = Level 25+ (21-40 range)

**Template Matching**:
- Resizes images to 100x100 for comparison
- Calculates Euclidean distance in RGB space
- Normalizes to 0-1 similarity score
- Minimum 50% similarity threshold for match

## ?? Tips for Best Results

### Screenshot Quality
- ? Use full-screen mode for clearest images
- ? Ensure good lighting/contrast
- ? Avoid motion blur or overlays
- ? Capture the entire roster grid

### Template Images
- ? Crop tightly around character portrait
- ? Include the entire face/card area
- ? Use clear, well-lit screenshots
- ? One template per character is enough

### Grid Slicing (Advanced)
If automatic slicing doesn't align properly:
1. Go to **Debug Slicer** page
2. Upload a roster screenshot
3. Adjust sliders:
   - **Sidebar Ratio**: Crop left sidebar (default: 18%)
   - **Card Aspect Ratio**: Card height/width (default: 1.0)
   - **Padding**: Space between cards (default: 5px)
   - **Columns**: Number of columns (default: 7)
4. Note the values that work
5. Update defaults in `ImageProcessingService.cs`

## ?? Troubleshooting

### "No characters detected"
- Ensure template matching is enabled
- Upload at least a few character templates
- Check browser console (F12) for detailed logs

### "Wrong character recognized"
- Template similarity might be too low
- Upload a better quality template for that character
- Consider manual entry for similar-looking characters

### "Incorrect rank/badge detected"
- Detection uses heuristics and may not be 100% accurate
- Use sliders to manually adjust before importing
- Check browser console for detection values

### Image slicing misaligned
- Use Debug Slicer page to fine-tune parameters
- Sidebar ratio, padding, or aspect ratio may need adjustment
- Different screen resolutions may require different settings

## ?? Data Storage

All data is stored locally in your browser using Local Storage:
- **Character Templates**: Base64-encoded images
- **User Characters**: Rank and badge levels
- **No server required**: Fully client-side application

To export/backup your data:
```javascript
// In browser console
localStorage.getItem('dcwc_user_characters')
localStorage.getItem('character_templates_index')
```

## ?? Future Enhancements

- [ ] Machine learning model for character recognition
- [ ] Level number OCR extraction (top-right corner)
- [ ] Team preset saving
- [ ] Export/import data functionality
- [ ] Character stat database integration
- [ ] Synergy/combo recommendations
- [ ] Multi-language support

## ?? License

This project is provided as-is for personal use. DC World's Collide and all related characters are property of their respective owners.

## ?? Contributing

Feel free to submit issues or pull requests on GitHub!

## ?? Contact

Created by [@quinnturner261298-debug](https://github.com/quinnturner261298-debug)

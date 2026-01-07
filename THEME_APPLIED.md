# Palimpsest Theme Applied ✨

## What Was Fixed

The CSS file had extensive copy/paste corruption that has been cleaned up:

1. **CSS Custom Properties**: All `\--` escaped sequences corrected to `--`
2. **Comment Syntax**: All `\*` in comments fixed to `*`
3. **Proper Formatting**: Code is now properly indented and readable

## Theme Details: Manuscript Archive

A warm, literary aesthetic inspired by writers' archives and libraries - perfect for authors working with complex fictional universes.

### Key Design Elements

**Color Palette:**
- Primary: Warm gold/bronze tones (#B89968)
- Background: Cream to warm taupe gradient
- Text: Dark charcoal with warm undertones
- Entity types have distinct colors for graph visualization

**Typography:**
- Headings: Crimson Text (serif) - classic, literary feel
- Body: Inter (sans-serif) - clean, readable
- Mono: Fira Code - for code snippets

**Components:**
- Cards have subtle left-border accent
- Buttons use uppercase labels with letter-spacing
- Form inputs have warm backgrounds
- All Shoelace components styled consistently

### Files Modified

1. **`/src/Palimpsest.Web/Themes/manuscript-archive/theme.css`**
   - Fixed all CSS syntax errors
   - Comprehensive design system with custom properties
   - Shoelace component overrides

2. **`/src/Palimpsest.Web/Views/Shared/_Layout.cshtml`**
   - Added Google Fonts (Crimson Text + Inter)
   - Added theme CSS reference
   - Updated header styling to use theme variables

3. **`/src/Palimpsest.Web/wwwroot/Themes/`**
   - Copied theme to static assets folder for serving

## How to Use

The theme is now active! All existing Shoelace components in your views will automatically use the new styling.

### Running the Application

```bash
cd /Users/jdray/Development/palimpsest/src
dotnet run --project Palimpsest.Web
```

Then visit `http://localhost:5000` to see the new theme in action.

### Entity Type Colors

The theme includes predefined colors for different entity types:

- **Character**: Warm amber (#D4A574)
- **Location**: Cool teal (#5A8F7B)
- **Concept**: Deep purple (#6B5B95)
- **Organization**: Primary gold (#B89968)
- **Event**: Burnt sienna (#C85A54)
- **Custom**: Steel blue (#4A8FB8)

Use utility classes like `.entity-character`, `.entity-location`, etc. to apply these colors.

## Next Steps

1. **Test the UI**: Run the application and verify all pages look good
2. **Customize**: Adjust color values in `theme.css` if needed
3. **Add Dark Mode**: Consider creating a dark variant
4. **Entity Badges**: Use the `.entity-badge` class for visual indicators

## Theme Architecture

Located at `/src/Palimpsest.Web/Themes/manuscript-archive/`:
- `theme.css` - All styling and custom properties
- `README.md` - Complete design documentation

The theme follows Shoelace's design token system, making it easy to maintain and extend.

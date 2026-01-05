# Shoelace Web Components Form Submission Fix

## Problem

When submitting forms that use Shoelace web components (`sl-input`, `sl-select`, `sl-textarea`), the form data was not being sent to the server, resulting in empty/null values on the server side. This caused document and universe creation to fail silently.

### Root Cause

**Shoelace web components do NOT automatically participate in standard HTML form submission**, even when they have a `name` attribute. This is a fundamental limitation of web components - they exist in the Shadow DOM and their values are not included in the standard FormData that gets submitted with a POST request.

### Affected Forms

1. **Documents/Create.cshtml** - Document creation form
2. **Universes/Create.cshtml** - Universe creation form

## Solution

The fix uses a **hidden input bridge pattern**:

1. Add hidden `<input type="hidden">` fields with the appropriate `name` attributes for model binding
2. Keep the Shoelace components for UI/UX (they look better and are more accessible)
3. Use JavaScript to intercept form submission and copy values from Shoelace components to hidden inputs
4. Submit the form with proper values

### Implementation Details

#### View Changes (Create.cshtml files)

```html
<!-- Hidden inputs for form submission -->
<input type="hidden" id="hiddenTitle" name="Title" />
<input type="hidden" id="hiddenSubtype" name="Subtype" />
<!-- etc. -->

<!-- Shoelace components for UI (without name attribute) -->
<sl-input id="Title" placeholder="e.g., Emerald Eyes" required></sl-input>
<sl-select id="Subtype" value="Book"></sl-select>
<!-- etc. -->
```

#### JavaScript Bridge

```javascript
document.addEventListener('DOMContentLoaded', async () => {
    const form = document.getElementById('documentForm');
    const submitBtn = document.getElementById('submitBtn');
    
    // Wait for Shoelace components to be defined
    await Promise.all([
        customElements.whenDefined('sl-input'),
        customElements.whenDefined('sl-select'),
        customElements.whenDefined('sl-textarea'),
        customElements.whenDefined('sl-button')
    ]);
    
    // Intercept form submission
    submitBtn.addEventListener('click', (e) => {
        e.preventDefault();
        
        // Copy values from Shoelace components to hidden inputs
        const title = document.getElementById('Title');
        document.getElementById('hiddenTitle').value = title.value || '';
        // ... repeat for all fields
        
        // Submit the form
        form.submit();
    });
});
```

## Alternative Solutions Considered

### 1. Use Native HTML Inputs
- **Pros**: Would work immediately with form submission
- **Cons**: Loss of Shoelace's accessibility features, consistent styling, and better UX

### 2. Use FormData API with AJAX
- **Pros**: More modern approach
- **Cons**: Would require rewriting the entire form handling on server side, breaking ASP.NET Core's model binding

### 3. Wait for Shoelace Form Participation
- Shoelace 2.x does NOT support automatic form participation
- This may change in Shoelace 3.x, but that's not released yet
- Our current version (2.12.0) requires the bridge pattern

## Best Practices for Future Forms

When creating new forms in this application:

1. **Always use the hidden input bridge pattern** for Shoelace components
2. **Keep the `name` attribute on hidden inputs** for ASP.NET Core model binding
3. **Remove the `name` attribute from Shoelace components** to avoid confusion
4. **Add console.log statements** during development to verify values are being captured
5. **Wait for custom elements to be defined** before setting up event handlers

## Testing

To verify the fix works:

1. Navigate to `/universes/create` and create a new universe
2. Navigate to the universe details page
3. Click "Add Document" 
4. Fill in the form with:
   - Title: "Test Document"
   - Document Type: "Book"
   - Paste some text content
5. Submit the form
6. Verify the document is created and appears in the database/list

## References

- [Shoelace Form Documentation](https://shoelace.style/getting-started/form-controls)
- [Web Components and Forms](https://web.dev/more-capable-form-controls/)
- Commits: 
  - `196f438` - Fix document creation form
  - `b6e1d69` - Fix universe creation form

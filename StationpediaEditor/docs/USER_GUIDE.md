# Stationpedia Editor - User Guide

A comprehensive guide to using the Stationpedia Editor for the Stationeers modding community.

## Table of Contents

1. [Getting Started](#getting-started)
2. [UI Overview](#ui-overview)
3. [Editing Devices](#editing-devices)
4. [Working with Content](#working-with-content)
5. [Validation & Preview](#validation--preview)
6. [File Management](#file-management)
7. [Keyboard Shortcuts](#keyboard-shortcuts)
8. [Tips and Tricks](#tips-and-tricks)
9. [Troubleshooting](#troubleshooting)

## Getting Started

### First Launch

1. **Open the Editor** - Launch Stationpedia Editor from your applications
2. **Open a Workspace** - Click "Open Workspace" or press Ctrl+O
3. **Select Your Mod Folder** - Navigate to your Stationpedia Ascended mod folder
4. **Browse Content** - Use the Content Tree to view all devices, guides, and mechanics

### What Gets Loaded

When you open a workspace, the editor loads:

- **descriptions.json** - All device descriptions and generic tooltip text
- **guides/** folder - Game guides and tutorials
- **game-mechanics/** folder - Mechanics explanation pages
- **images/** folder - Asset images used in descriptions

## UI Overview

### Main Interface

The Stationpedia Editor consists of several key areas:

```
┌─────────────────────────────────────────────────────┐
│ Toolbar (Open, Save, Close, Tooltips)              │
├──────────────────────┬────────────────────┬────────┤
│  Content Tree        │  Rich Text Editor  │ Preview│
│  (Left Panel)        │  (Main Panel)      │ (Right)│
│                      │                    │        │
│  • Devices           │  Format Toolbar    │ Live   │
│  • Guides            │  Text Area         │ render │
│  • Mechanics         │  Source View       │        │
├──────────────────────┴────────────────────┴────────┤
│ Status Bar (Dirty state, Path, Validation)         │
└──────────────────────────────────────────────────────┘
```

### Left Panel: Content Tree

- **Devices** - All device descriptions organized by category
- **Guides** - Game guides and documentation
- **Mechanics** - Mechanics and system explanations
- **Search** - Find content by name or device key

**Actions:**
- Click to select a device/guide for editing
- Right-click for context menu options
- Drag to search for specific content

### Main Panel: Content Editing

The main editing area has several tabs and components:

1. **Page Description** - Main description text for the device/guide
2. **Properties** - Edit device metadata (display name, device key)
3. **Operational Details** - Drag-and-drop hierarchical sections
4. **Logic Descriptions** - Input/output port documentation
5. **Tooltips** - Generic hover text (toggle with 💬 Tooltips)

### Right Panel: Live Preview

Shows a real-time preview of how your content will appear in-game:
- Formatted text with proper styling
- Collapsible sections expanded/collapsed
- Logic port specifications in table format
- Device specifications and requirements

## Editing Devices

### Basic Device Information

1. **Select Device** - Click on a device in the Content Tree
2. **Edit Display Name** - Change the user-facing name in Properties panel
3. **Edit Description** - Use the Page Description editor for main content
4. **Format Text** - Use rich text toolbar for styling

### Rich Text Formatting

Use the toolbar or keyboard shortcuts:

- **Bold** - **Ctrl+B** - For emphasis
- **Italic** - **Ctrl+I** - For alternative terms
- **Underline** - **Ctrl+U** - For important details
- **Headings** - H1, H2, H3 buttons - For structure
- **Lists** - Bullet/numbered list buttons
- **Links** - 🔗 Link button to reference other pages
- **Code** - Inline code for technical terms
- **Quotes** - For specifications or quotes

### Adding Device Links

Insert cross-references to other Stationpedia pages:

1. **Highlight Text** - Select the text to turn into a link
2. **Click Link Button** - Click 🔗 in the toolbar
3. **Search** - Type to find the target device/guide
4. **Insert** - Click the result to insert the link

Example: Link "Airlock" to the airlock device page

### Source View

For advanced editing, you can view/edit the raw TMP (Stationpedia markup):

1. Click **</> Source** button
2. Edit the raw markup directly
3. Click **📝 Visual** to return to rich editing

## Working with Content

### Operational Details

Edit hierarchical operational sections (Usage, Features, etc.):

1. **Add Section** - Click "Add Detail" button
2. **Edit Title** - Change section heading
3. **Add Content** - Write description, items, or steps
4. **Nested Sections** - Add sub-sections for hierarchy
5. **Reorder** - Drag sections to rearrange
6. **Delete** - Click delete icon to remove section

**Types of Content:**

- **Description** - Rich text explanation
- **Items** - Bullet point list
- **Steps** - Numbered procedure list
- **Image** - Embed screenshot or diagram
- **Video** - Link to YouTube or video file

### Logic Descriptions

Document device input/output ports:

1. **Add Port** - Click "Add Logic Description"
2. **Port Name** - Use the port variable name
3. **Data Type** - Specify Boolean, Float, Integer, Hash, etc.
4. **Range** - Document valid values (e.g., "0-100" or "Any")
5. **Description** - Explain what the port does

Example:
- **Port**: `Setting`
- **Data Type**: Integer
- **Range**: 0-100
- **Description**: Controls the device sensitivity level

### Mode Descriptions

For devices with multiple modes:

1. **Add Mode** - Click "Add Mode"
2. **Mode Value** - Numeric or named mode identifier
3. **Description** - Explain what this mode does

### Slot Descriptions

Document inventory slots:

1. **Add Slot** - Click "Add Slot"
2. **Slot Number** - Which slot (1, 2, 3, etc.)
3. **Slot Type** - What goes here (Battery, Cartridge, Item, etc.)
4. **Description** - Purpose and requirements

## Validation & Preview

### Real-Time Validation

The editor checks for common errors:

- **❌ Broken Links** - References to non-existent devices
- **⚠️ Warnings** - Potential issues (unlinked references, etc.)
- **✅ Valid** - Content meets all requirements

### Viewing Validation Results

1. **Validation Panel** - Bottom-right shows issues
2. **Status Bar** - 🔴 errors and 🟡 warnings count
3. **Error Details** - Click to jump to problem location

### Live Preview

The right panel updates in real-time as you edit:

- **Format Preview** - See how text will render
- **Collapsible Sections** - Test expand/collapse
- **Layout** - Verify readability

## File Management

### Saving Your Work

**Manual Save:**
- Press **Ctrl+S** or click 💾 Save button
- Status shows "Unsaved" (●) when changes need saving

**Auto-Save:**
- Editor automatically saves every 30 seconds
- Look for the auto-save indicator in the status bar

**Backups:**
- Automatic backups created before each save
- Located in workspace `.backups/` folder with timestamps
- Keep last 3 backups by default

### Opening Workspaces

**From Welcome Screen:**
1. Click "Open Workspace"
2. Select your mod folder
3. Content loads automatically

**Recent Files:**
1. Click 📂 Recent menu
2. Select from recently opened workspaces
3. Or clear recent files list

### Last Workspace

The editor automatically remembers your last workspace:
- Restarts with same workspace loaded
- Quick return to your work after closing

## Keyboard Shortcuts

| Shortcut | Action | When to Use |
|----------|--------|-----------|
| **Ctrl+S** | Save all changes | After making edits |
| **Ctrl+O** | Open workspace | Start new project |
| **Ctrl+W** | Close workspace | Switch projects |
| **Ctrl+N** | New workspace | Start fresh |
| **F5** | Refresh preview | If preview seems stuck |
| **Ctrl+Z** | Undo | Fix mistakes |
| **Ctrl+Y** | Redo | Re-apply changes |
| **Ctrl+B** | Bold text | Emphasis |
| **Ctrl+I** | Italic text | Alternatives |
| **Ctrl+U** | Underline | Important points |
| **Ctrl+Shift+S** | Save As | Save to new location |

**Pro Tip:** Press ⌨️ in the status bar to see all shortcuts any time!

## Tips and Tricks

### General Editing

1. **Use Headings** - Structure with H1, H2, H3 for readability
2. **Link Related** - Connect similar devices with links
3. **Test in Preview** - Always check the preview panel
4. **Backup Often** - Auto-save helps, but save manually for milestones

### Content Organization

1. **Consistent Formatting** - Use same style for similar sections
2. **Clear Language** - Write for new players
3. **Hierarchy** - Use nested operational details for complex devices
4. **Examples** - Include usage examples in descriptions

### Validation

1. **Fix Errors First** - Red indicators need fixing
2. **Review Warnings** - Yellow indicators suggest improvements
3. **Test Links** - Click preview to verify all links work
4. **Check Spelling** - Use spell checker before publishing

### Performance

1. **Close Unused Panels** - Reduces memory usage
2. **Disable Preview** - When editing large descriptions
3. **Clear Recent Files** - If the list gets long
4. **Restart if Slow** - Clears temporary memory

### Collaboration

1. **Backup Before Sharing** - Use .backups folder
2. **Export as HTML** - For sharing with non-editors
3. **Use Guides** - Create guides for complex systems
4. **Version Control** - Consider using Git for changes

## Troubleshooting

### Workspace Won't Open

**Problem:** "Error loading workspace"

**Solutions:**
1. Check folder has `descriptions.json`
2. Verify JSON file is valid (not corrupted)
3. Ensure write permissions on folder
4. Try opening a different workspace first

### Changes Not Saving

**Problem:** "Unsaved" indicator stays on

**Solutions:**
1. Click 💾 Save explicitly
2. Check folder write permissions
3. Verify disk space is available
4. Try saving to different location

### Preview Not Updating

**Problem:** Preview shows old content

**Solutions:**
1. Press F5 to refresh
2. Click different device, then back
3. Close and reopen workspace
4. Restart editor

### Lost Unsaved Changes

**Problem:** "Changes disappeared"

**Solutions:**
1. Check `.backups/` folder for recent backup
2. Use Ctrl+Z to undo if editor still open
3. Check if auto-save created a recent version
4. Restore from last manual save

### Slow Performance

**Problem:** Editor runs slowly

**Solutions:**
1. Close unused panels
2. Disable live preview temporarily
3. Restart the editor
4. Check available system memory
5. Reduce operational detail nesting depth

### Link Issues

**Problem:** "Link not found" or "Broken reference"

**Solutions:**
1. Check target device exists
2. Verify correct device key spelling
3. Use link picker to auto-complete
4. Check if device is in guides/mechanics folder

## Getting Help

- Check the **Status Bar** - Press ⌨️ for shortcuts
- Hover tooltips - Many buttons have descriptions
- Review this guide - Most answers are here
- Check `descriptions.json` format - See README for examples

## Best Practices Summary

✅ **Do:**
- Save frequently (Ctrl+S)
- Use keyboard shortcuts for speed
- Keep preview visible while editing
- Use rich text formatting consistently
- Link related content together
- Test in preview before publishing

❌ **Don't:**
- Edit JSON directly unless necessary
- Forget to save before closing
- Leave validation warnings unreviewed
- Use inconsistent formatting
- Create overly nested sections (>3 levels)
- Forget to document custom logic ports

---

**Happy editing!** The Stationpedia Editor is designed to make documenting Stationeers content quick and intuitive. Happy writing! 📝

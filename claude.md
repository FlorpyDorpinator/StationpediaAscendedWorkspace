# Stationpedia Ascended - Claude Context Document

## Project Overview
**Stationpedia Ascended** is a BepInEx mod for the game Stationeers that enhances the in-game Stationpedia (encyclopedia) with comprehensive tooltips, bug fixes, and quality of life improvements.

## Key Information

### Project Structure
- **StationpediaAscended/** - Main mod source code (C# BepInEx plugin)
- **docs/logic-documentation/** - Wiki documentation for device logic
- **.specstory/** - AI chat history and project context from SpecStory extension
- **Decompiled Stationeers Source** - Reference code from the base game

### Technology Stack
- **Language**: C# (.NET Framework 4.8)
- **Mod Framework**: BepInEx (Harmony patching)
- **Target Game**: Stationeers (Unity-based game)
- **Hot Reload**: Supports F6 in-game reload for rapid development

### Core Features
1. **Enhanced Tooltips** - Hover descriptions for logic types, slots, memory, device modes
2. **Operational Details Section** - Device-specific advanced information with phoenix branding
3. **Page Description Customization** - Replace/append/prepend page content via JSON
4. **Bug Fixes** - Scrollbar handle fix, window dragging crash prevention
5. **Improved Search** - Exact match prioritization, categorical sorting, removed clutter
6. **Custom Branding** - Phoenix logo, orange accents, "Stationpedia Ascended" header

### Configuration System
All customizations stored in `descriptions.json`:
- `genericDescriptions` - Reusable tooltips for logic/slots/memory/modes/connections
- `devices` - Device-specific overrides with operational details and page modifications

### Development Workflow
1. Edit code or descriptions.json
2. Press F6 in-game for instant hot-reload (no restart needed)
3. Test changes immediately in Stationpedia

### Important Notes
- Uses Harmony patches to inject functionality into base game classes
- Must maintain compatibility with base Stationeers assemblies
- Phoenix icon and orange theme (#FF7A18) are brand identity
- Community-driven: descriptions database accepts contributions

## Current State
- Mod is feature-complete and published
- Actively maintained with community feedback
- Ongoing work: expanding descriptions database for more devices

## Common Tasks
- Adding new device tooltips: Update descriptions.json
- Fixing UI bugs: Patch relevant Stationpedia UI classes
- Adding operational details: Update device entries in descriptions.json
- Testing: Use hot-reload (F6) in development environment

## References
- README.md - User-facing documentation
- CONFIGURATION.md - JSON configuration guide
- .specstory/history/ - Previous AI collaboration sessions

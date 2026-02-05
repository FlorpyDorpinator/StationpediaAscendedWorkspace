# Claude Code Configuration

This directory contains hooks and configuration for Claude Code to maintain context across sessions.

## Files

### settings.json
Hook configuration that:
- **SessionStart Hook**: Automatically loads `claude.md` and `continuation.md` at the start of every session and after auto-compaction
- **PreCompact Hook**: Warns when context is about to be compacted (at ~90% capacity)

### claude.md (in project root)
Main project context document containing:
- Project overview and architecture
- Technology stack
- Core features and capabilities
- Development workflow
- Common tasks and patterns

This file is **automatically loaded** at session start and after compaction.

### continuation.md
Tracks the current state of work across context compactions:
- Current task being worked on
- Recent progress made
- Next steps to take
- Files being modified
- Important context to preserve

**IMPORTANT**: Update this file manually before context runs out. The PreCompact hook will warn you when it's time.

### hooks/
Contains the batch scripts that run when hooks trigger:

- **load-context.bat**: Reads and displays claude.md and continuation.md
- **pre-compact.bat**: Warns about imminent compaction and logs the event

### compaction.log
Automatically created log file tracking when PreCompact hook triggers.

## How It Works

### Session Start
When you start a new Claude Code session or after auto-compaction:
1. SessionStart hook triggers
2. `load-context.bat` runs
3. Contents of `claude.md` and `continuation.md` are displayed
4. Claude receives full project context automatically

### Before Compaction
When context usage reaches ~90%:
1. PreCompact hook triggers (auto matcher)
2. `pre-compact.bat` runs
3. You receive a warning to update `continuation.md`
4. Event is logged to `compaction.log`
5. You should manually update `continuation.md` with current task state
6. Run `/compact` when ready

## Maintenance

### Updating Project Context
Edit `../claude.md` (in project root) to update:
- Feature descriptions
- Architecture changes
- New patterns or conventions
- Common workflows

### Updating Continuation Document
Edit `continuation.md` when:
- Starting a new major task
- PreCompact hook warns about context limits
- Completing significant milestones
- Before taking a break from development

Include:
- What you're currently working on
- Which files are being modified
- What's been completed
- What needs to happen next
- Any critical decisions or context

### Testing Hooks
To verify hooks are working:
```bash
# Check if settings.json is valid
cat .claude/settings.json

# Test load-context hook manually
.\.claude\hooks\load-context.bat

# Test pre-compact hook manually
.\.claude\hooks\pre-compact.bat
```

## Notes

- Hooks automatically trigger - no manual intervention needed
- The PreCompact hook gives you time to save your work before compaction
- All context files use markdown for readability
- Hooks are Windows batch scripts (.bat files) since project is on Windows

@echo off
REM Load critical context files at session start or after auto-compact

echo.
echo ===============================================
echo  Loading Stationpedia Ascended Context Files
echo ===============================================
echo.

REM Read claude.md
if exist "%CLAUDE_PROJECT_DIR%\claude.md" (
    echo ## Project Context (claude.md)
    echo.
    type "%CLAUDE_PROJECT_DIR%\claude.md"
    echo.
    echo.
)

REM Read continuation document
if exist "%CLAUDE_PROJECT_DIR%\.claude\continuation.md" (
    echo ## Continuation Document
    echo.
    type "%CLAUDE_PROJECT_DIR%\.claude\continuation.md"
    echo.
    echo.
)

echo ===============================================
echo  Context Loading Complete
echo ===============================================
echo.

exit /b 0

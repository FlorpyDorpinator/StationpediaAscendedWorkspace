@echo off
REM Update continuation document before context compaction

echo.
echo ===============================================
echo  CRITICAL: Context Compaction Imminent
echo ===============================================
echo.
echo Context usage has reached critical levels.
echo Auto-compaction will occur after this hook completes.
echo.
echo IMPORTANT: If you are in the middle of a task,
echo please update .claude\continuation.md with:
echo   - Current task status
echo   - Files being modified
echo   - Next steps to resume work
echo   - Any critical context needed
echo.
echo Then trigger compaction with: /compact
echo.
echo ===============================================

REM Create timestamp
for /f "tokens=2-4 delims=/ " %%a in ('date /t') do (set mydate=%%c-%%a-%%b)
for /f "tokens=1-2 delims=/:" %%a in ('time /t') do (set mytime=%%a%%b)

REM Log the compaction event
echo [%mydate% %mytime%] PreCompact hook triggered - context at critical level >> "%CLAUDE_PROJECT_DIR%\.claude\compaction.log"

exit /b 0

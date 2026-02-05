@echo off
echo Starting Stationpedia Editor in development mode...
echo.
echo Press Ctrl+C to stop the server
echo.
cd /d "%~dp0"
call npm run dev

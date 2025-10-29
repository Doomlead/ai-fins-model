@echo off
setlocal EnableDelayedExpansion

set "tool_path=tools\universal_asset_tool\universal_asset_tool.exe"

if not exist "%tool_path%" (
  echo Universal Asset Tool binary not found. Building a fresh copy...
  call ..\bat\publish_universal_asset_tool.bat /f
  if errorlevel 1 goto :error
)

"%tool_path%" --help | findstr /I "def_jam_fight_for_ny" >nul
if errorlevel 1 (
  echo Def Jam: Fight for NY support is not present in the current Universal Asset Tool build.
  echo Rebuilding the tool to pick up the latest command-line verbs...
  call ..\bat\publish_universal_asset_tool.bat /f
  if errorlevel 1 goto :error
)

"%tool_path%" def_jam_fight_for_ny
if errorlevel 1 goto :error

pause
exit /b 0

:error
echo.
echo Failed to run the Universal Asset Tool for Def Jam: Fight for NY.
echo Please review the build output above for details.
pause
exit /b 1

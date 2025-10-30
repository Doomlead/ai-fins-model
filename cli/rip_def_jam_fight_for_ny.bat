@echo off
setlocal EnableDelayedExpansion

pushd "%~dp0"

set "tool_path=tools\universal_asset_tool\universal_asset_tool.exe"

call :ensure_cli_ready
if errorlevel 1 goto :error

"%tool_path%" def_jam_fight_for_ny
if errorlevel 1 goto :error

goto :success

:ensure_cli_ready
if exist "%tool_path%" goto :check_for_verb

echo Universal Asset Tool binary not found. Building a fresh copy...
call ..\bat\publish_universal_asset_tool.bat /f
if errorlevel 1 exit /b 1

:check_for_verb
set "_verb_probe=def_jam_fight_for_ny def-jam-fight-for-ny"
"%tool_path%" --help | findstr /I "!_verb_probe!" >nul
if errorlevel 1 (
  echo Def Jam: Fight for NY support is not present in the current Universal Asset Tool build.
  echo Rebuilding the tool to pick up the latest command-line verbs...
  call ..\bat\publish_universal_asset_tool.bat /f
  if errorlevel 1 exit /b 1

  "%tool_path%" --help | findstr /I "!_verb_probe!" >nul
  if errorlevel 1 (
    echo.
    echo The rebuilt Universal Asset Tool still does not list the Def Jam: Fight for NY command.
    echo Please verify the repository is up to date and that the .NET build prerequisites are installed.
    exit /b 1
  )
)

exit /b 0

:success
popd
pause
exit /b 0

:error
popd
echo.
echo Failed to run the Universal Asset Tool for Def Jam: Fight for NY.
echo Please review the build output above for details.
pause
exit /b 1

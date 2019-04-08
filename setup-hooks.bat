setlocal
echo off

::---------------------------------------------------------
:: This is a shortcut to the setup-hooks script in Sparta. Check there for more info.
::---------------------------------------------------------

pushd %~dp0
set root_dir=%cd%

set SPARTA_DIR=%root_dir%\Project\Assets\Plugins\Sparta

call %SPARTA_DIR%\setup-hooks.bat

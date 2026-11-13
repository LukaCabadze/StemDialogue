@echo off
rem Installs Node.js LTS (official OpenJS package) with winget and logs the result.
set LOG=%~dp0install-node.log
echo === %DATE% %TIME% === > "%LOG%"
winget install --id OpenJS.NodeJS.LTS --exact --silent --accept-source-agreements --accept-package-agreements >> "%LOG%" 2>&1
echo exit code: %ERRORLEVEL% >> "%LOG%"
"%ProgramFiles%\nodejs\node.exe" -v >> "%LOG%" 2>&1
echo done >> "%LOG%"

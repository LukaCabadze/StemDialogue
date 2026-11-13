@echo off
rem Installs the frontend packages (including the Angular CLI) and logs the result.
set "PATH=%ProgramFiles%\nodejs;%APPDATA%\npm;%PATH%"
set LOG=%~dp0setup-frontend.log
echo === %DATE% %TIME% === > "%LOG%"
node -v >> "%LOG%" 2>&1
call npm -v >> "%LOG%" 2>&1
cd /d "%~dp0..\frontend"
call npm install --no-audit --no-fund >> "%LOG%" 2>&1
echo npm install exit code: %ERRORLEVEL% >> "%LOG%"
call npx ng version >> "%LOG%" 2>&1
echo done >> "%LOG%"

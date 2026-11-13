@echo off
title STEM Dialogue Web - http://localhost:4200
set "PATH=%ProgramFiles%\nodejs;%APPDATA%\npm;%PATH%"
cd /d "%~dp0frontend"
if not exist node_modules (
  echo Installing packages, first run only...
  call npm install --no-audit --no-fund
)
call npm start -- --open
pause

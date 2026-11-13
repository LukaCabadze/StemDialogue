@echo off
title STEM Dialogue API - http://localhost:5080
cd /d "%~dp0backend"
dotnet run --project src\StemDialogue.Api
pause

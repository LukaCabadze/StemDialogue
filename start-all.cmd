@echo off
rem Starts the API and the website in two separate windows.
start "STEM Dialogue API" cmd /c "%~dp0start-backend.cmd"
start "STEM Dialogue Web" cmd /c "%~dp0start-frontend.cmd"

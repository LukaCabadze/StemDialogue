@echo off
rem Stops whatever is serving the website (port 4200 and stray dev servers), clears the Angular cache and starts it again.
set LOG=%~dp0restart-frontend.log
echo === %DATE% %TIME% === > "%LOG%"
for %%P in (4200 59455) do (
  for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":%%P " ^| findstr LISTENING') do (
    echo stopping PID %%p on port %%P >> "%LOG%"
    taskkill /PID %%p /T /F >> "%LOG%" 2>&1
  )
)
if exist "%~dp0..\frontend\.angular\cache" rmdir /s /q "%~dp0..\frontend\.angular\cache"
start "STEM Dialogue Web" cmd /c "%~dp0..\start-frontend.cmd"
echo started >> "%LOG%"

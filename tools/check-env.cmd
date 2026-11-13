@echo off
rem Writes the installed tool versions to check-env.log next to this script.
set LOG=%~dp0check-env.log
echo === %DATE% %TIME% === > "%LOG%"
echo [node] >> "%LOG%"
where node >> "%LOG%" 2>&1
node -v >> "%LOG%" 2>&1
echo [npm] >> "%LOG%"
call npm -v >> "%LOG%" 2>&1
echo [dotnet] >> "%LOG%"
dotnet --version >> "%LOG%" 2>&1
echo [winget] >> "%LOG%"
winget --version >> "%LOG%" 2>&1
echo done >> "%LOG%"

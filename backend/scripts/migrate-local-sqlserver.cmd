@echo off
rem Applies migrations and base content to a local SQL Server (Windows authentication).
rem Usage: scripts\migrate-local-sqlserver.cmd [server]   (default: localhost)
rem Output goes to backend\migrate.log
setlocal
cd /d "%~dp0.."
set SERVER=%~1
if "%SERVER%"=="" set SERVER=localhost
set LOG=%CD%\migrate.log
set ASPNETCORE_ENVIRONMENT=Development
set Database__Provider=SqlServer
set ConnectionStrings__Default=Server=%SERVER%;Database=StemDialogue;Integrated Security=true;TrustServerCertificate=true
echo === migrate %date% %time% against %SERVER% > "%LOG%"
dotnet run --project src\StemDialogue.Api --no-launch-profile -- --migrate >> "%LOG%" 2>&1 || goto :failed
echo === RESULT: SUCCESS >> "%LOG%"
exit /b 0
:failed
echo === RESULT: FAILED >> "%LOG%"
exit /b 1

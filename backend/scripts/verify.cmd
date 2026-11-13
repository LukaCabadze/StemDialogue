@echo off
rem Restores, builds, creates the initial EF migration (first run only) and runs all tests.
rem Output goes to backend\verify.log
setlocal
cd /d "%~dp0.."
set LOG=%CD%\verify.log
echo === verify started %date% %time% > "%LOG%"
dotnet --version >> "%LOG%" 2>&1

echo === restore >> "%LOG%"
dotnet restore StemDialogue.sln >> "%LOG%" 2>&1 || goto :failed
dotnet tool restore >> "%LOG%" 2>&1 || goto :failed

echo === build >> "%LOG%"
dotnet build StemDialogue.sln --no-restore -v minimal >> "%LOG%" 2>&1 || goto :failed

if not exist "src\StemDialogue.Infrastructure\Persistence\Migrations" (
  echo === migration >> "%LOG%"
  dotnet ef migrations add InitialCreate --project src\StemDialogue.Infrastructure --startup-project src\StemDialogue.Api --output-dir Persistence\Migrations >> "%LOG%" 2>&1 || goto :failed
  dotnet build StemDialogue.sln --no-restore -v minimal >> "%LOG%" 2>&1 || goto :failed
)

echo === test >> "%LOG%"
dotnet test StemDialogue.sln --no-build -v minimal >> "%LOG%" 2>&1 || goto :failed

echo === RESULT: SUCCESS >> "%LOG%"
exit /b 0

:failed
echo === RESULT: FAILED >> "%LOG%"
exit /b 1

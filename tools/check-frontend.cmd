@echo off
rem Confirms the running dev server serves the new styles (notebook grid + torn band).
set LOG=%~dp0check-frontend.log
echo === %DATE% %TIME% === > "%LOG%"
echo [web status] >> "%LOG%"
curl -s -o NUL -w "%%{http_code}" http://localhost:4200/ >> "%LOG%" 2>&1
echo. >> "%LOG%"
echo [grid in styles.css] >> "%LOG%"
curl -s http://localhost:4200/styles.css | findstr /i /c:"eaeaff" /c:"sd-paper" >> "%LOG%" 2>&1
echo [api via proxy] >> "%LOG%"
curl -s -o NUL -w "%%{http_code}" http://localhost:4200/api/v1/projects/school2university/home >> "%LOG%" 2>&1
echo. >> "%LOG%"
echo [api direct] >> "%LOG%"
curl -s -o NUL -w "%%{http_code}" http://localhost:5080/api/v1/projects/school2university >> "%LOG%" 2>&1
echo. >> "%LOG%"
echo done >> "%LOG%"

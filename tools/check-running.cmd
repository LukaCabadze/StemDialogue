@echo off
rem Checks that the API and the website respond and logs the HTTP status codes.
set LOG=%~dp0check-running.log
echo === %DATE% %TIME% === > "%LOG%"
echo [api] >> "%LOG%"
curl -s -o NUL -w "%%{http_code}" http://localhost:5080/api/v1/projects/school2university >> "%LOG%" 2>&1
echo. >> "%LOG%"
echo [web] >> "%LOG%"
curl -s -o NUL -w "%%{http_code}" http://localhost:4200/ >> "%LOG%" 2>&1
echo. >> "%LOG%"
echo [web proxy to api] >> "%LOG%"
curl -s -o NUL -w "%%{http_code}" http://localhost:4200/api/v1/projects/school2university/home >> "%LOG%" 2>&1
echo. >> "%LOG%"
echo done >> "%LOG%"

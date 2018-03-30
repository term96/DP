@echo off

set FRONTEND_PROCESS_TITLE=lw2-frontend
set BACKEND_PROCESS_TITLE=lw2-frontend
for /f "tokens=1,2 delims==" %%a in (config\config.ini) do (
	if %%a==FRONTEND_PROCESS_TITLE set FRONTEND_PROCESS_TITLE=%%b
	if %%a==BACKEND_PROCESS_TITLE set BACKEND_PROCESS_TITLE=%%b
)

echo STOPPING FRONTEND
taskkill /FI "WINDOWTITLE eq %FRONTEND_PROCESS_TITLE%"

echo STOPPING BACKEND
taskkill /FI "WINDOWTITLE eq %BACKEND_PROCESS_TITLE%"
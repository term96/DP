@echo off

set FRONTEND_PROCESS_TITLE=lw2-frontend
set BACKEND_PROCESS_TITLE=lw2-frontend
for /f "tokens=1,2 delims==" %%a in (config\config.ini) do (
	if %%a==FRONTEND_PROCESS_TITLE set FRONTEND_PROCESS_TITLE=%%b
	if %%a==BACKEND_PROCESS_TITLE set BACKEND_PROCESS_TITLE=%%b
)

echo RUNNING FRONTEND
cd frontend
start "%FRONTEND_PROCESS_TITLE%" dotnet frontend.dll
cd ..

echo RUNNING BACKEND
cd backend
start "%BACKEND_PROCESS_TITLE%" dotnet backend.dll
cd ..
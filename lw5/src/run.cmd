@echo off

set FRONTEND_PROCESS_TITLE=dp-frontend
set BACKEND_PROCESS_TITLE=dp-backend
set TEXTLISTENER_PROCESS_TITLE=dp-textlistener
set TEXTRANKCALC_PROCESS_TITLE=dp-textrankcalc
for /f "tokens=1,2 delims==" %%a in (config\config.ini) do (
	if %%a==FRONTEND_PROCESS_TITLE set FRONTEND_PROCESS_TITLE=%%b
	if %%a==BACKEND_PROCESS_TITLE set BACKEND_PROCESS_TITLE=%%b
	if %%a==TEXTLISTENER_PROCESS_TITLE set TEXTLISTENER_PROCESS_TITLE=%%b
	if %%a==TEXTRANKCALC_PROCESS_TITLE set TEXTRANKCALC_PROCESS_TITLE=%%b
)

echo RUNNING FRONTEND
cd frontend
start "%FRONTEND_PROCESS_TITLE%" dotnet frontend.dll
cd ..

echo RUNNING BACKEND
cd backend
start "%BACKEND_PROCESS_TITLE%" dotnet backend.dll
cd ..

echo RUNNING TEXTLISTENER
cd textlistener
start "%TEXTLISTENER_PROCESS_TITLE%" dotnet textlistener.dll
cd ..

echo RUNNING TEXTRANKCALC
cd textrankcalc
start "%TEXTRANKCALC_PROCESS_TITLE%" dotnet textrankcalc.dll
cd ..

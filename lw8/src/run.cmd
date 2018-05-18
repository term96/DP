@echo off

set FRONTEND_PROCESS_TITLE=dp-frontend
set BACKEND_PROCESS_TITLE=dp-backend
set TEXTLISTENER_PROCESS_TITLE=dp-textlistener
set TEXTRANKCALC_PROCESS_TITLE=dp-textrankcalc
set VOWELCONSCOUNTER_PROCESS_TITLE=dp-vowelconscounter
set VOWELCONSCOUNTER_PROCESS_NUMBER=1
set VOWELCONSRATER_PROCESS_TITLE=dp-vowelconsrater
set VOWELCONSRATER_PROCESS_NUMBER=1
set TEXTSTATISTICS_PROCESS_TITLE=dp-textstatistics
set TEXTPROCESSINGLIMITER_PROCESS_TITLE=dp-textprocessinglimiter
set TEXTSUCCESSMARKER_PROCESS_TITLE=dp-textsuccessmarker
for /f "tokens=1,2 delims==" %%a in (config\config.ini) do (
	if %%a==FRONTEND_PROCESS_TITLE set FRONTEND_PROCESS_TITLE=%%b
	if %%a==BACKEND_PROCESS_TITLE set BACKEND_PROCESS_TITLE=%%b
	if %%a==TEXTLISTENER_PROCESS_TITLE set TEXTLISTENER_PROCESS_TITLE=%%b
	if %%a==TEXTRANKCALC_PROCESS_TITLE set TEXTRANKCALC_PROCESS_TITLE=%%b
	if %%a==VOWELCONSCOUNTER_PROCESS_TITLE set VOWELCONSCOUNTER_PROCESS_TITLE=%%b
	if %%a==VOWELCONSCOUNTER_PROCESS_NUMBER set VOWELCONSCOUNTER_PROCESS_NUMBER=%%b
	if %%a==VOWELCONSRATER_PROCESS_TITLE set VOWELCONSRATER_PROCESS_TITLE=%%b
	if %%a==VOWELCONSRATER_PROCESS_NUMBER set VOWELCONSRATER_PROCESS_NUMBER=%%b
	if %%a==TEXTSTATISTICS_PROCESS_TITLE set TEXTSTATISTICS_PROCESS_TITLE=%%b
	if %%a==TEXTPROCESSINGLIMITER_PROCESS_TITLE set TEXTPROCESSINGLIMITER_PROCESS_TITLE=%%b
	if %%a==TEXTSUCCESSMARKER_PROCESS_TITLE set TEXTSUCCESSMARKER_PROCESS_TITLE=%%b
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

echo RUNNING VOWELCONSCOUNTER (%VOWELCONSCOUNTER_PROCESS_NUMBER% instances)
cd vowelconscounter
for /l %%i in (1, 1, %VOWELCONSCOUNTER_PROCESS_NUMBER%) do (
	start "%VOWELCONSCOUNTER_PROCESS_TITLE%-%%i" dotnet vowelconscounter.dll
)
cd ..

echo RUNNING VOWELCONSRATER (%VOWELCONSRATER_PROCESS_NUMBER% instances)
cd vowelconsrater
for /l %%i in (1, 1, %VOWELCONSRATER_PROCESS_NUMBER%) do (
	start "%VOWELCONSRATER_PROCESS_TITLE%-%%i" dotnet vowelconsrater.dll
)
cd ..

echo RUNNING TEXTSTATISTICS
cd textstatistics
start "%TEXTSTATISTICS_PROCESS_TITLE%" dotnet textstatistics.dll
cd ..

echo RUNNING TEXTPROCESSINGLIMITER
cd textprocessinglimiter
start "%TEXTPROCESSINGLIMITER_PROCESS_TITLE%" dotnet textprocessinglimiter.dll
cd ..

echo RUNNING TEXTSUCCESSMARKER
cd textsuccessmarker
start "%TEXTSUCCESSMARKER_PROCESS_TITLE%" dotnet textsuccessmarker.dll
cd ..
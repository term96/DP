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

echo STOPPING FRONTEND
taskkill /FI "WINDOWTITLE eq %FRONTEND_PROCESS_TITLE%"

echo STOPPING BACKEND
taskkill /FI "WINDOWTITLE eq %BACKEND_PROCESS_TITLE%"

echo STOPPING TEXTLISTENER
taskkill /FI "WINDOWTITLE eq %TEXTLISTENER_PROCESS_TITLE%"

echo STOPPING TEXTRANKCALC
taskkill /FI "WINDOWTITLE eq %TEXTRANKCALC_PROCESS_TITLE%"

echo STOPPING VOWELCONSCOUNTER (%VOWELCONSCOUNTER_PROCESS_NUMBER% instances)
for /l %%i in (1, 1, %VOWELCONSCOUNTER_PROCESS_NUMBER%) do (
	taskkill /FI "WINDOWTITLE eq %VOWELCONSCOUNTER_PROCESS_TITLE%-%%i"
)

echo STOPPING VOWELCONSRATER (%VOWELCONSRATER_PROCESS_NUMBER% instances)
for /l %%i in (1, 1, %VOWELCONSRATER_PROCESS_NUMBER%) do (
	taskkill /FI "WINDOWTITLE eq %VOWELCONSRATER_PROCESS_TITLE%-%%i"
)

echo STOPPING TEXTSTATISTICS
taskkill /FI "WINDOWTITLE eq %TEXTSTATISTICS_PROCESS_TITLE%"

echo STOPPING TEXTPROCESSINGLIMITER
taskkill /FI "WINDOWTITLE eq %TEXTPROCESSINGLIMITER_PROCESS_TITLE%"

echo STOPPING TEXTSUCCESSMARKER
taskkill /FI "WINDOWTITLE eq %TEXTSUCCESSMARKER_PROCESS_TITLE%"

@echo off

if "%~1"=="" (
	echo Build version must be passed as argument
	pause
	exit
)

if exist build-%1 rmdir build-%1 /S /Q
mkdir build-%1

echo BUILDING FRONTEND
cd src/frontend/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/Frontend
cd ../../
echo:

echo BUILDING BACKEND
cd src/backend/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/Backend
cd ../../
echo:

echo BUILDING TEXTLISTENER
cd src/textlistener/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/TextListener
cd ../../
echo:

echo BUILDING TEXTRANKCALC
cd src/textrankcalc/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/TextRankCalc
cd ../../
echo:

echo BUILDING VOWELCONSCOUNTER
cd src/vowelconscounter/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/VowelConsCounter
cd ../../
echo:

echo BUILDING VOWELCONSRATER
cd src/vowelconsrater/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/VowelConsRater
cd ../../
echo:

echo BUILDING TEXTSTATISTICS
cd src/textstatistics/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/TextStatistics
cd ../../
echo:

echo BUILDING TEXTPROCESSINGLIMITER
cd src/textprocessinglimiter/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/TextProcessingLimiter
cd ../../
echo:

echo BUILDING TEXTSUCCESSMARKER
cd src/textsuccessmarker/
dotnet clean
dotnet publish -c Release -o build-%1
if errorlevel 1 goto failed
move build-%1 ../../build-%1/TextSuccessMarker
cd ../../
echo:

echo COPIYNG CONFIGS
cd src/
@echo off
xcopy config ..\build-%1\config\
xcopy run.cmd ..\build-%1\
xcopy stop.cmd ..\build-%1\
cd ..

echo BUILD FINISHED
goto success

:failed
echo BUILD FAILED

:success

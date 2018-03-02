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
dotnet publish -c Release -o build-%1
move build-%1 ../../build-%1/Frontend
cd ../../
echo:

echo BUILDING BACKEND
cd src/backend/
dotnet publish -c Release -o build-%1
move build-%1 ../../build-%1/Backend
cd ../../
echo:

echo COPIYNG CONFIGS
cd src/
@echo off
xcopy config ..\build-%1\config\
xcopy run.cmd ..\build-%1\
xcopy stop.cmd ..\build-%1\

echo BUILD FINISHED
pause
@echo off
     
set PROJECT=-projectPath 
set PROJECT_PATH=%~dp0
    
set WIN_PATH=%USERPROFILE%\Desktop\SciXR\
set FILE_NAME=SciXR.exe

@REM Common options
set BATCH=-batchmode
set QUIT=-quit
     
@REM Builds:
set WIN=-buildWindows64Player "%WIN_PATH%%FILE_NAME%"
     
@REM Win build
   
echo Running Win Build for: %PROJECT_PATH%
echo "%PROGRAMFILES%\Unity\Editor\Unity.exe" %BATCH% %QUIT% %PROJECT% %PROJECT_PATH% %WIN%
"%ProgramFiles%\Unity\Editor\Unity.exe" %BATCH% %QUIT% %PROJECT% %PROJECT_PATH% %WIN%

md %WIN_PATH%\Data
xcopy %PROJECT_PATH%\Data %WIN_PATH%\Data /s /e /h /i /y

PAUSE
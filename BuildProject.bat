@echo off
echo ================================================
echo Unity Match 3 Project Build Script
echo ================================================
echo.

REM Try to find Unity installation
set UNITY_PATH=

REM Check common Unity Hub locations
if exist "C:\Program Files\Unity\Hub\Editor\6000.2.9f1\Editor\Unity.exe" (
    set UNITY_PATH=C:\Program Files\Unity\Hub\Editor\6000.2.9f1\Editor\Unity.exe
)

if exist "C:\Program Files\Unity Hub\Editor\6000.2.9f1\Editor\Unity.exe" (
    set UNITY_PATH=C:\Program Files\Unity Hub\Editor\6000.2.9f1\Editor\Unity.exe
)

if exist "%USERPROFILE%\Unity\Hub\Editor\6000.2.9f1\Editor\Unity.exe" (
    set UNITY_PATH=%USERPROFILE%\Unity\Hub\Editor\6000.2.9f1\Editor\Unity.exe
)

REM If Unity not found, try to find any Unity 6 version
if not defined UNITY_PATH (
    for /d %%i in ("C:\Program Files\Unity\Hub\Editor\6000.*") do (
        if exist "%%i\Editor\Unity.exe" (
            set UNITY_PATH=%%i\Editor\Unity.exe
            goto :found
        )
    )
)

:found
if not defined UNITY_PATH (
    echo ERROR: Unity 6000.2.9f1 (or compatible version) not found!
    echo Please ensure Unity is installed through Unity Hub.
    echo.
    echo Checked locations:
    echo - C:\Program Files\Unity\Hub\Editor\6000.2.9f1\Editor\Unity.exe
    echo - C:\Program Files\Unity Hub\Editor\6000.2.9f1\Editor\Unity.exe
    echo.
    echo Please open Unity manually and check for compilation errors.
    pause
    exit /b 1
)

echo Found Unity at: %UNITY_PATH%
echo.

REM Get project path
set PROJECT_PATH=%~dp0
set PROJECT_PATH=%PROJECT_PATH:~0,-1%

echo Project path: %PROJECT_PATH%
echo.

REM First, validate and compile
echo Step 1: Validating and compiling scripts...
echo ------------------------------------------------
"%UNITY_PATH%" -quit -batchmode -projectPath "%PROJECT_PATH%" -logFile "%PROJECT_PATH%\Logs\BuildLog.txt" -executeMethod BuildScript.ValidateScripts

echo.
echo Step 2: Building Windows Standalone...
echo ------------------------------------------------
"%UNITY_PATH%" -quit -batchmode -projectPath "%PROJECT_PATH%" -logFile "%PROJECT_PATH%\Logs\BuildLog.txt" -executeMethod BuildScript.BuildGameCommandLine

if errorlevel 1 (
    echo.
    echo ERROR: Build failed!
    echo Check the log file at: %PROJECT_PATH%\Logs\BuildLog.txt
    echo.
    type "%PROJECT_PATH%\Logs\BuildLog.txt" | findstr /i "error exception"
    echo.
    pause
    exit /b 1
)

echo.
echo ================================================
echo BUILD SUCCESSFUL!
echo ================================================
echo.
echo Build output: %PROJECT_PATH%\Build\Match3Game.exe
echo Build log: %PROJECT_PATH%\Logs\BuildLog.txt
echo.
echo You can now run the game from the Build folder.
echo.
pause

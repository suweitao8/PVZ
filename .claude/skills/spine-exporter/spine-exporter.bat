@echo off
REM Spine 导出工具 - Windows 批处理版本
REM 自动检测 Godot 并运行

setlocal EnableDelayedExpansion

REM 获取脚本目录
set "SCRIPT_DIR=%~dp0"
set "PROJECT_DIR=%SCRIPT_DIR%..\..\..\..\"

REM 默认参数
set "INPUT=animations"
set "OUTPUT=animations_spritesheet"
set "FILTER="
set "FPS=8"
set "SHEET_SIZE=2048"

REM 解析参数
:parse_args
if "%~1"=="" goto :done_args
if /i "%~1"=="--input" (set "INPUT=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="-i" (set "INPUT=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="--output" (set "OUTPUT=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="-o" (set "OUTPUT=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="--filter" (set "FILTER=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="-f" (set "FILTER=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="--fps" (set "FPS=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="--sheet-size" (set "SHEET_SIZE=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="-s" (set "SHEET_SIZE=%~2" & shift & shift & goto :parse_args)
if /i "%~1"=="--help" goto :show_help
if /i "%~1"=="-h" goto :show_help
echo Unknown option: %~1
exit /b 1

:show_help
echo Usage: %~nx0 [options]
echo.
echo Options:
echo   -i, --input DIR      Input directory (default: animations)
echo   -o, --output DIR     Output directory (default: animations_spritesheet)
echo   -f, --filter PATTERN Filter animations by path
echo   --fps N              Frames per second (default: 8)
echo   -s, --sheet-size N   Sprite sheet size (default: 2048)
echo   -h, --help           Show this help
exit /b 0

:done_args

REM 查找 Godot
set "GODOT="

REM 检查 PATH
where godot >nul 2>&1
if !errorlevel!==0 set "GODOT=godot"

if "!GODOT!"=="" (
    where godot4 >nul 2>&1
    if !errorlevel!==0 set "GODOT=godot4"
)

REM 检查常见位置
if "!GODOT!"=="" (
    if exist "D:\Godot\Godot_v4.5.exe" set "GODOT=D:\Godot\Godot_v4.5.exe"
)
if "!GODOT!"=="" (
    if exist "C:\Godot\Godot_v4.5.exe" set "GODOT=C:\Godot\Godot_v4.5.exe"
)
if "!GODOT!"=="" (
    if exist "C:\Program Files\Godot\Godot_v4.5.exe" set "GODOT=C:\Program Files\Godot\Godot_v4.5.exe"
)
if "!GODOT!"=="" (
    if exist "%USERPROFILE%\Godot\Godot_v4.5.exe" set "GODOT=%USERPROFILE%\Godot\Godot_v4.5.exe"
)
if "!GODOT!"=="" (
    if exist "%USERPROFILE%\Downloads\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64.exe" set "GODOT=%USERPROFILE%\Downloads\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64.exe"
)

REM 构建 Godot 参数
set "GODOT_ARGS=--headless --script res://.claude/skills/spine-exporter/godot/spine_exporter.gd -- --input !INPUT! --output !OUTPUT! --fps !FPS! --sheet-size !SHEET_SIZE!"
if not "!FILTER!"=="" set "GODOT_ARGS=!GODOT_ARGS! --filter !FILTER!"

if not "!GODOT!"=="" (
    echo Using Godot: !GODOT!
    cd /d "%PROJECT_DIR%"
    "!GODOT!" !GODOT_ARGS!
) else (
    echo Godot not found. Falling back to .NET placeholder renderer...
    set "DOTNET_DLL=!SCRIPT_DIR!src\bin\Release\net8.0\SpineExporter.dll"

    REM 构建 DLL 参数
    set "DOTNET_ARGS=--input !INPUT! --output !OUTPUT! --fps !FPS! --sheet-size !SHEET_SIZE!"
    if not "!FILTER!"=="" set "DOTNET_ARGS=!DOTNET_ARGS! --filter !FILTER!"

    dotnet "!DOTNET_DLL!" !DOTNET_ARGS!
)

endlocal

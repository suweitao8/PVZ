#!/bin/bash
# Spine 导出工具 - 自动检测 Godot 并运行

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$(dirname "$(dirname "$(dirname "$(dirname "$SCRIPT_DIR")")")")"

# 默认参数
INPUT="animations"
OUTPUT="animations_spritesheet"
FILTER=""
FPS=8
SHEET_SIZE=2048

# 解析参数
while [[ $# -gt 0 ]]; do
    case $1 in
        --input|-i)
            INPUT="$2"
            shift 2
            ;;
        --output|-o)
            OUTPUT="$2"
            shift 2
            ;;
        --filter|-f)
            FILTER="$2"
            shift 2
            ;;
        --fps)
            FPS="$2"
            shift 2
            ;;
        --sheet-size|-s)
            SHEET_SIZE="$2"
            shift 2
            ;;
        --help|-h)
            echo "Usage: $0 [options]"
            echo ""
            echo "Options:"
            echo "  -i, --input DIR      Input directory (default: animations)"
            echo "  -o, --output DIR     Output directory (default: animations_spritesheet)"
            echo "  -f, --filter PATTERN Filter animations by path"
            echo "  --fps N              Frames per second (default: 8)"
            echo "  -s, --sheet-size N   Sprite sheet size (default: 2048)"
            echo "  -h, --help           Show this help"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

# 查找 Godot
GODOT=""
if command -v godot &> /dev/null; then
    GODOT="godot"
elif command -v godot4 &> /dev/null; then
    GODOT="godot4"
elif [ -f "$PROJECT_DIR/../Godot_v4.5.exe" ]; then
    GODOT="$PROJECT_DIR/../Godot_v4.5.exe"
elif [ -f "/c/Program Files/Godot/Godot_v4.5.exe" ]; then
    GODOT="/c/Program Files/Godot/Godot_v4.5.exe"
elif [ -f "$HOME/Downloads/Godot_v4.5.1-stable_mono_win64/Godot_v4.5.1-stable_mono_win64.exe" ]; then
    GODOT="$HOME/Downloads/Godot_v4.5.1-stable_mono_win64/Godot_v4.5.1-stable_mono_win64.exe"
fi

# 构建 Godot 参数
GODOT_ARGS="--headless --script res://.claude/skills/spine-exporter/godot/spine_exporter.gd"
GODOT_ARGS="$GODOT_ARGS -- --input $INPUT --output $OUTPUT --fps $FPS --sheet-size $SHEET_SIZE"
if [ -n "$FILTER" ]; then
    GODOT_ARGS="$GODOT_ARGS --filter $FILTER"
fi

if [ -n "$GODOT" ]; then
    echo "Using Godot: $GODOT"
    cd "$PROJECT_DIR"
    $GODOT $GODOT_ARGS
else
    echo "Godot not found. Falling back to .NET placeholder renderer..."
    DOTNET_DLL="$SCRIPT_DIR/src/bin/Release/net8.0/SpineExporter.dll"

    # 构建 DLL 参数
    DOTNET_ARGS="--input $INPUT --output $OUTPUT --fps $FPS --sheet-size $SHEET_SIZE"
    if [ -n "$FILTER" ]; then
        DOTNET_ARGS="$DOTNET_ARGS --filter $FILTER"
    fi

    dotnet "$DOTNET_DLL" $DOTNET_ARGS
fi

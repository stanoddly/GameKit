#!/bin/bash
set -e

if ! command -v import &>/dev/null; then
    echo "Error: import not found. Install ImageMagick." >&2
    exit 1
fi

if [ ! -f headless_display.tmp ]; then
    echo "Error: headless_display.tmp not found. Is headless_run.sh running?" >&2
    exit 1
fi

DISPLAY=$(cat headless_display.tmp) import -window root "${1:-screenshot.png}"
echo "Screenshot saved to ${1:-screenshot.png}"

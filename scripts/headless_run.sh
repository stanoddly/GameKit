#!/bin/bash
set -e

if ! command -v xvfb-run &>/dev/null; then
    echo "Error: xvfb-run not found. Install Xvfb (e.g. xorg-x11-server-Xvfb)." >&2
    exit 1
fi

if [ $# -eq 0 ]; then
    echo "Usage: headless_run.sh <command> [args...]" >&2
    exit 1
fi

xvfb-run -a --server-args="-screen 0 1920x1080x24" bash -c '
    echo "$DISPLAY" > headless_display.tmp
    exec "$@"
' _ "$@"

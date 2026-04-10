#!/bin/bash
set -eo pipefail

OS="$(uname)"

if [[ "$OS" == "Linux" ]]; then
    IP=$(ip route get 1 | awk '{print $7; exit}' | sed 's/\.[0-9]*$/.104/')
    rsync -avz --delete "$1" "pi@$IP:netcore/$2/"
elif [[ "$OS" == "Darwin" ]]; then
    # rsync -avz --delete "$1" "csa-3:netcore/$2";
    # rsync -avz --delete "$1" "csa-5:netcore/$2";
    rsync -avz --delete "$1" "csa-5-home:netcore/$2";

    echo "✅ Uploaded current build";
else
    echo "Unsupported OS: $OS"
    exit 1
fi
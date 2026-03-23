#!/bin/bash

OS="$(uname)"

if [[ "$OS" == "Linux" ]]; then
    IP=$(ip route get 1 | awk '{print $7; exit}' | sed 's/\.[0-9]*$/.104/')
    rsync -avz --delete "$1" "pi@$IP:netcore/$2/"
elif [[ "$OS" == "Darwin" ]]; then
    rsync -avz --delete "$(TargetDir)" "csa:netcore/$(ProjectName)"
else
    echo "Unsupported OS: $OS"
    exit 1
fi
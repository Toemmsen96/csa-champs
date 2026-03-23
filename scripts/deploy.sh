#!/bin/bash

IP=$(ip route get 1 | awk '{print $7; exit}' | sed 's/\.[0-9]*$/.104/')

rsync -avz --delete "$1" "pi@$IP:netcore/$2/"
ssh pi@$(ip -4 addr show | awk '/inet / && !/127.0.0.1/ {print $2}' | cut -d/ -f1 | head -n1 | sed 's/\.[0-9]*$/.104/')

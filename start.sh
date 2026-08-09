#!/bin/bash

# Kill background processes if you press Ctrl+C
trap 'kill $(jobs -p)' EXIT

# Start .NET backend in the background
dotnet run --project ./backend/src/AssignmentSystem.Api &

# Start frontend in the foreground
cd ./frontend && npm run dev
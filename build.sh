#!/bin/bash

find . -iname "bin" -o -iname "obj" | xargs rm -rf

dotnet run --project targets --no-launch-profile

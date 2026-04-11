#!/bin/bash

find . -iname "bin" -o -iname "obj" | xargs rm -rf

dotnet test -f net10.0 --verbosity quiet
dotnet test -f net9.0 --verbosity quiet
dotnet test -f net8.0 --verbosity quiet

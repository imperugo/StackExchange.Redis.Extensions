#!/bin/bash

find . -iname "bin" -o -iname "obj" | xargs rm -rf

dotnet test -f net7.0 --verbosity quiet
dotnet test -f net6.0 --verbosity quiet
dotnet test -f netcoreapp3.1 --verbosity quiet

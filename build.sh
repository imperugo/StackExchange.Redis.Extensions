#!/bin/bash

find . -iname "bin" -o -iname "obj" | xargs rm -rf

dotnet test -f net7.0
dotnet test -f net6.0
dotnet test -f netcoreapp3.1

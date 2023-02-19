#!/bin/bash

find . -iname "bin" -o -iname "obj" | xargs rm -rf

dotnet test

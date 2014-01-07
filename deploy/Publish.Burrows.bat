@echo off

powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\publish.ps1 -PackageName 'Burrows'; exit $error.Count}"

pause
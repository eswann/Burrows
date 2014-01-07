@echo off

powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\package.ps1 -PackageName 'Burrows'; exit $error.Count}"

pause
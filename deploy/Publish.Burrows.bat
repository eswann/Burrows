@echo off


powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\archive.ps1; exit $error.Count}"


powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\publish.ps1 -PackageName 'Burrows'; exit $error.Count}"


powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\publish.ps1 -PackageName 'Burrows.Autofac'; exit $error.Count}"


powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\publish.ps1 -PackageName 'Burrows.NLog'; exit $error.Count}"


powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\publish.ps1 -PackageName 'Burrows.Log4Net'; exit $error.Count}"


powershell -NoProfile -ExecutionPolicy unrestricted -Command "& {.\publish.ps1 -PackageName 'Burrows.NHib'; exit $error.Count}"

pause
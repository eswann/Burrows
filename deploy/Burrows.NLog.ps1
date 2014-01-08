properties {
	$BaseDir = Resolve-Path "..\"
	$SolutionFile = "$BaseDir\src\Burrows.sln"
	$ProjectPath = "$BaseDir\src\Loggers\Burrows.NLog\Burrows.NLog.csproj"	
	$ArchiveDir = "$BaseDir\Deploy\Archive"
	
	$NuGetPackageName = "Burrows.NLog"
}

. .\common.ps1

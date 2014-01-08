properties {
	$BaseDir = Resolve-Path "..\"
	$SolutionFile = "$BaseDir\src\Burrows.sln"
	$ProjectPath = "$BaseDir\src\Loggers\Burrows.Log4Net\Burrows.Log4Net.csproj"	
	$ArchiveDir = "$BaseDir\Deploy\Archive"
	
	$NuGetPackageName = "Burrows.Log4Net"
}

. .\common.ps1

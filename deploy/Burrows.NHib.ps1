properties {
	$BaseDir = Resolve-Path "..\"
	$SolutionFile = "$BaseDir\src\Burrows.sln"
	$ProjectPath = "$BaseDir\src\Persistence\Burrows.NHib\Burrows.NHib.csproj"	
	$ArchiveDir = "$BaseDir\Deploy\Archive"
	
	$NuGetPackageName = "Burrows.NHib"
}

. .\common.ps1

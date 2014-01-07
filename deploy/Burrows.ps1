properties {
	$BaseDir = Resolve-Path "..\"
	$SolutionFile = "$BaseDir\src\Burrows.sln"
	$SpecsForOutput = "$BaseDir\src\Burrows\Burrows.nuspec"
	$ProjectPath = "$BaseDir\src\Burrows\Burrows.csproj"	
	$ArchiveDir = "$BaseDir\Deploy\Archive"
	
	$NuGetPackageName = "Burrows"
}

. .\common.ps1

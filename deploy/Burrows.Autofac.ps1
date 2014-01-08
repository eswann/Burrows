properties {
	$BaseDir = Resolve-Path "..\"
	$SolutionFile = "$BaseDir\src\Burrows.sln"
	$ProjectPath = "$BaseDir\src\Containers\Burrows.Autofac\Burrows.Autofac.csproj"	
	$ArchiveDir = "$BaseDir\Deploy\Archive"
	
	$NuGetPackageName = "Burrows.Autofac"
}

. .\common.ps1

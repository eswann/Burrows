#Common NuGet/Archiving logic, not meant ot be executed directly.

$framework = '4.5.1'

task default -depends Pack

task Init {
        cls
}

task Clean -depends Init {
        
        if (Test-Path $ArchiveDir) {
                ri $ArchiveDir -Recurse
        }
        
        ri Burrows*.nupkg
}

task Build -depends Init,Clean {
        exec { msbuild $SolutionFile /p:Configuration=Release }
}

#This function can be overriden to add additional logic to the archive process.
function OnArchiving {
}

task Pack -depends Build {

        exec { nuget pack "$ProjectPath" -OutputDirectory Output -Properties Configuration=Release }
}

task Publish -depends Pack {
        $PackageName = gci "$NuGetPackageName.nupkg" 
        exec { nuget push "Output\$PackageName" }
}
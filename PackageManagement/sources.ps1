# Find all files
$x = dir -recurse *.cs | ForEach-Object { Resolve-Path -Relative $_ } | ForEach-Object { echo $_.Replace( ".\", "`t").Replace(".cs",".cs \`n") }

# Load sources script
$sources = [System.IO.File]::ReadAllText(".\sources");

# set source files
$x = "#region sourcefiles`nSOURCES=\`n$x	`$(GENERATED_RESOURCES_SOURCE)`n`n#endregion"

# Replace the region
$newSources = [System.Text.RegularExpressions.Regex]::Replace($sources, "#region\s*sourcefiles.*?#endregion", $x  ,[System.Text.RegularExpressions.RegexOptions]::SingleLine )

#Skip files:
$newSources = $newSources.Replace("`n 	Resources\Messages.Designer.cs \","")
$newSources = $newSources.Replace("`n 	Properties\AssemblyInfo.cs \","")
$newSources = $newSources.Replace("`n	Resources\Messages.Designer.cs \","")
$newSources = $newSources.Replace("`n	Properties\AssemblyInfo.cs \","")

# Write out file
[System.IO.File]::WriteAllText(".\sources", $newSources );
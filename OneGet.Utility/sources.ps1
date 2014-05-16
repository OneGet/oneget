$x = dir -recurse *.cs | ForEach-Object { Resolve-Path -Relative $_ } | ForEach-Object { echo $_.Replace( ".\", "	").Replace(".cs", ".cs \`n") }
 
$sources = [System.IO.File]::ReadAllText(".\sources");
 
$x = "#region sourcefiles`nSOURCES=\`n$x `n`n#endregion`n"

$newSources = [System.Text.RegularExpressions.Regex]::Replace($sources, "#region\s*sourcefiles.*?#endregion", $x  ,[System.Text.RegularExpressions.RegexOptions]::SingleLine )

[System.IO.File]::WriteAllText(".\sources", $newSources );
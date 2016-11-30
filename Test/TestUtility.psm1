$script:appVeyorConstants =  @{ 
    AccountName = 'powershell'
    ApiUrl = 'https://ci.appveyor.com/api'
    PowerShellRepoName='powershell-f975h'  # daily run has this name
}


function Get-PowerShellCoreBuild {
    [CmdletBinding()]
    param(

        [string]$repoName = $script:appVeyorConstants.PowerShellRepoName,
        [string]$branch = 'master',
        [string]$downloadFolder = 'C:\projects'
    )

    $foundGood = $false
    $records = 20
    $lastBuildId = $null
    $project = $null

    while(!$foundGood)
    {
        $startBuildIdString = [string]::Empty
        if($lastBuildId)
        {
            $startBuildIdString = "&startBuildId=$lastBuildId"
        }


        $project = Invoke-RestMethod -Method Get -Uri "$($appVeyorConstants.ApiUrl)/projects/$($appVeyorConstants.AccountName)/$repoName/history?recordsNumber=$records$startBuildIdString&branch=$branch"

        foreach($build in $project.builds)
        {
            $version = $build.version
            $status = $build.status
            if($status -ieq 'success')
            {
                Write-Verbose "Using PowerShell Version: $version"

                $foundGood = $true

                Write-Verbose "Uri = $($appVeyorConstants.ApiUrl)/projects/$($appVeyorConstants.AccountName)/$repoName/build/$version"
                $project = Invoke-RestMethod -Method Get -Uri "$($appVeyorConstants.ApiUrl)/projects/$($appVeyorConstants.AccountName)/$repoName/build/$version" 
                break
            }
            else 
            {
                Write-Warning "There is a newer SDK build, $version, which is in status: $status"
            }
        }
    }

    # get project with last build details
    if (-not $project) {

        throw "Cannot find a good build for $branch"
    }

    # we assume here that build has a single job
    # get this job id

    $jobId = $project.build.jobs[0].jobId
    Write-Verbose "jobId=$jobId"
    
    Write-Verbose "$project.build.jobs[0]"

    $artifactsUrl = "$($appVeyorConstants.ApiUrl)/buildjobs/$jobId/artifacts"

    Write-Verbose "Uri=$artifactsUrl"
    $artifacts = Invoke-RestMethod -Method Get -Uri $artifactsUrl 

    if (-not $artifacts) {
        throw "Cannot find artifacts in $artifactsUrl"
    }

    # Get PowerShellCore.msi artifacts for Windows  
 
    $artifacts = $artifacts | where-object { $_.filename -like '*powershell*.msi'}
    $returnArtifactsLocation = @{}

    #download artifacts to a temp location

    foreach($artifact in $artifacts)
    {
        $artifactPath = $artifact[0].fileName
        $artifactFileName = Split-Path -Path $artifactPath -Leaf

        # artifact will be downloaded as 
        $tempLocalArtifactPath = "$downloadFolder\Temp-$artifactFileName-$jobId.msi"
        $localArtifactPath = "$downloadFolder\$artifactFileName-$jobId.msi"
        if(!(Test-Path $localArtifactPath))
        {
            # download artifact
            # -OutFile - is local file name where artifact will be downloaded into

            try 
            {
                $ProgressPreference = 'SilentlyContinue'
                Invoke-WebRequest -Method Get -Uri "$($appVeyorConstants.ApiUrl)/buildjobs/$jobId/artifacts/$artifactPath" `
                    -OutFile $tempLocalArtifactPath  -UseBasicParsing -DisableKeepAlive

                Move-Item -Path $tempLocalArtifactPath -Destination $localArtifactPath   
            } 
            finally
            {
                $ProgressPreference = 'Continue'
                if(test-path $tempLocalArtifactPath)
                {
                    remove-item $tempLocalArtifactPath
                }
            } 
        }

        #$lastDotIndex = $artifactFileName.LastIndexOf('.')
        #$returnArtifactsLocation.add($artifactFileName.Substring(0,$lastDotIndex),$localArtifactPath)

    }

    Write-Verbose $localArtifactPath
    return $localArtifactPath
}

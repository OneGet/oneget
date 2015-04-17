#
#  Copyright (c) Microsoft Corporation. All rights reserved.
#  Licensed under the Apache License, Version 2.0 (the "License");
#  you may not use this file except in compliance with the License.
#  You may obtain a copy of the License at
#  http://www.apache.org/licenses/LICENSE-2.0
#
#  Unless required by applicable law or agreed to in writing, software
#  distributed under the License is distributed on an "AS IS" BASIS,
#  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
#  See the License for the specific language governing permissions and
#  limitations under the License.
#

$global:restart = $false
$sleep = 100
$counter = 0

$base = "$PSScriptRoot\..\websites"

Get-EventSubscriber | Unregister-Event

# track if this script changes so we can restart.
$fsw = New-Object IO.FileSystemWatcher $PSScriptRoot, $MyInvocation.MyCommand.Name -Property @{IncludeSubdirectories = $false;NotifyFilter = [IO.NotifyFilters]'FileName, LastWrite';EnableRaisingEvents = $true }
$null = Register-ObjectEvent $fsw Changed -SourceIdentifier FileChanged -Action {
    $global:restart = $true
}

Write-Host "`n---------------------[Listening on http://localhost]-----------------"

$listener = New-Object System.Net.HttpListener
$listener.Prefixes.Add('http://*:80/')
$listener.Prefixes.Add('https://*:443/')
$listener.Start()

function Make-Safe {
    param ([string]$str )

    return ($str  -replace '/','\' -replace '\\\\','\' -replace '[^\d\w\[\]_\-\.\\]','-' -replace '--','-').Trim("\/- ")
}

$md5 = new-object -TypeName System.Security.Cryptography.MD5CryptoServiceProvider
$utf8 = new-object -TypeName System.Text.UTF8Encoding

function Get-Hash {
    param( [string]$str )
    return [System.BitConverter]::ToString($md5.ComputeHash($utf8.GetBytes($str))) -replace "-",""
}

function Get-LocalFilename {
    param(  [Uri]$url )
    $h = make-safe $url.host
    $path = make-safe $url.LocalPath
    $query = make-safe $url.Query

    # if the path is longer than 128 bytes, let's trim it and append a hash
    if( $path.Length -gt 128 )  {
        $path = $path.substring( 0, 32 ) + "..." + (get-hash $path)
    }


    # if the query part is over 64 bytes, let's just make a hash from it.
    if( $query.Length -gt 64 ) {
        $query = (get-hash $query)
    }

    $local = "$base\$h\$path{$query}"
    $local = $local.Trim("\");

    if( test-path $local ) {
        return (resolve-path $local)
    }
    return $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($local)
}

function PullThru-File {
    param(  [Uri]$url, [string]$localFile )
    Write-Progress -Activity "Listening..." -PercentComplete $counter -CurrentOperation "$url => $localFile" -Status "Downloading remote file."
    if( -not (test-path $localFile )) {
        #create the parent folder first.
        $null = mkdir $localFile
        $null = rmdir $localFile
    }

    if( $url.Host -match "localhost" ) {
        write-host "Avoiding loopback [$url]"
        return
    }

    write-host "Downloading remote file into local cache [$url]"

    try {
        $r = wget -Uri $url -OutFile $localFile -passthru
        if( $r.Headers ) {
            $ct = $r.Headers["Content-Type"]
            if( $ct ) {
                set-content -Value $ct -Path "$localFile.ContentType"
            }
        }
    } catch {

    }
}

function Send-TextResponse {
    param ($response, $content)

    $buffer = [System.Text.Encoding]::UTF8.GetBytes($content)
    $response.ContentLength64 = $buffer.Length
    $len = $buffer.length
    Write-Progress -Activity "Listening..." -PercentComplete $counter -CurrentOperation "Sending $len bytes " -Status "Sending Response."

    $response.OutputStream.Write($buffer, 0, $buffer.Length)
}

function Send-FileResponse {
  param ($response, $file)
    $content =  (get-content $file -raw -encoding byte)
    $response.ContentLength64 = $content.Length
    if( test-path "$file.ContentType" )  {
        $response.ContentType=(get-content "$file.ContentType")
    }
    $len = $buffer.length
    Write-Progress -Activity "Listening..." -PercentComplete $counter -CurrentOperation "Sending $len bytes " -Status "Sending Response."

    if( $content ) {
        $response.OutputStream.Write($content, 0, $content.Length)
    }
}

$task = $null

while ($listener.IsListening)
{
    Write-Progress -Activity "Listening..." -PercentComplete $counter -CurrentOperation "not busy" -Status "Waiting For Request."
    if( $task -eq $null ) {
        $task = $listener.GetContextAsync()
    }

    if( $task.IsCompleted ) {
        Write-Progress -Activity "Listening..." -PercentComplete $counter -CurrentOperation "processing" -Status "Request Accepted."
        $context = $task.Result
        $task = $null
        #$task = $listener.GetContextAsync()
        $requestUrl = $context.Request.Url
        $response = $context.Response

        Write-Host ''
        Write-Host "> $requestUrl"

        $localPath = $requestUrl.LocalPath
        $localPath = ($localPath) -replace '//','/'

        if ($localPath -eq "/about-sandbox" ) {
            Send-TextResponse $response "pm-test-sandbox"
            $response.Close()
            continue;
        }

        if ($localPath -eq "/quit" ) {
            Write-Host "`nQuitting..."
            $response.Close()

            break;
        }

        $filePath = Get-LocalFilename $requestUrl

        Write-Host ">>>> $filePath "
        if( -not (test-path $filePath )) {
            # go pull the file down first.
            PullThru-File $requestUrl $filePath
        }


        if( test-path $filePath ) {
            $filePath = (resolve-path $filePath).Path
            Send-FileResponse  $response $filePath
        } else {
            $response.StatusCode = 404
        }

        $response.Close()

        $responseStatus = $response.StatusCode
        Write-Host "< $responseStatus"
    }


    if( $global:restart )  {
        break;
    } else {
        $counter += 0.1
        if( $counter -gt 100 ){
            $counter = 0
        }
        Sleep -Milliseconds $sleep
    }
}

Get-EventSubscriber | Unregister-Event
$listener.Stop()

if( $global:restart ) {
    Write-Host "`nRestarting"
    . "$PSScriptRoot\webserver.ps1"
} else {
    Write-Host "`nFinished."
}

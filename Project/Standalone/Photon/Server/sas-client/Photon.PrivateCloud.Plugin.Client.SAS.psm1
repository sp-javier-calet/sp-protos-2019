Set-StrictMode -Version 2

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath Microsoft.WindowsAzure.Storage.dll) 4>&1 | Write-Debug
Import-Module (Join-Path -Path $PSScriptRoot -ChildPath Microsoft.WindowsAzure.Configuration.dll) 4>&1 | Write-Debug

function Get-7zAlias {
    [CmdletBinding()]
    param()

    if (-not(Get-Alias -Name sz -ErrorAction SilentlyContinue)) {
        Get-Command 7z `
            -ErrorAction SilentlyContinue | % { Set-Alias sz ($_) -Scope 1 }
    }
    if (-not(Get-Alias -Name sz -ErrorAction SilentlyContinue)) {
        Get-ChildItem (Join-Path -Path $PSScriptRoot -ChildPath "7z.exe") `
            -ErrorAction SilentlyContinue | % { Set-Alias sz ($_) -Scope 1 }
    }
    if (-not(Get-Alias -Name sz -ErrorAction SilentlyContinue)) {
        Get-ChildItem "C:\Program*\7-Zip\7z.exe" `
            -ErrorAction SilentlyContinue | % { Set-Alias sz ($_) -Scope 1 }
    }
    if (-not(Get-Alias -Name sz -ErrorAction SilentlyContinue)) {
        Write-Error "7z.exe needed"
    }
}

Function Get-PhotonPluginList {
    <#
    .SYNOPSIS
        List private Photon cloud plugins available in the Azure BLOB storage
        
    .PARAMETER Customer
        Plugin customer name, i.e. "ExitGames".

    .PARAMETER Plugin
        Full length exact plugin name, i.e. "ExitGames.Plugin"
        
    .PARAMETER Key
        Customer personal auth token.
        
    .EXAMPLE
        Get-PhotonPluginList -Customer ExitGames -Key 12345
    #>

    [CmdletBinding()]
    param(
        <# PHOTON CLOUD #>
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(Mandatory)]
        [string]$Customer,

        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [string]$Plugin,
        
        [ValidateNotNullOrEmpty()]
        [parameter(Mandatory)]
        [string]$Key,

        <# AZURE BLOB STORAGE PATHS #>
        [ValidateNotNullOrEmpty()]
        [string]$Base = "plugins",

        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[a-z\d\-.]+$", Options = [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)]
        [string]$Container = "public",
        
        <# Plugin service REST API endpoint #>
        [System.Uri]$Uri = "https://plugins.photonengine.com")
    
    try {
        $Container = $Container.ToLowerInvariant()

        $Url = $Uri.Scheme + "://" + $Uri.Host + ":" + $Uri.Port + "/plugin/list/$Customer"
        if ($Plugin) {
            $Url += "/$Plugin"
        }
        $Url += "?key=$Key"

        $request = @{
            Uri = New-Object System.Uri($url)
            Method = [Microsoft.PowerShell.Commands.WebRequestMethod]::Get
        }

        if (![System.String]::IsNullOrEmpty($Uri.UserInfo)) {
            $request.Add("Headers", @{
                Authorization=("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($Uri.UserInfo)))
            })
        } 

        $items = @()
        $data = Invoke-RestMethod @request
        foreach ($item in $data.Blobs) {
            $r = New-Object PSObject -Property @{
                'Name' = $item.Name.Substring($Base.Length + 1)
                'Length' = $item.ContentLength
                'LastModified' = $item.LastModified
                'MD5' = $item.ContentMD5
            }       
            if ($r.Name -match "^([^/]+)/([^/]+)/([^/]+)/.+$") {
                Add-Member -InputObject $r -MemberType NoteProperty -Name Customer -Value $Matches[1]
                Add-Member -InputObject $r -MemberType NoteProperty -Name Plugin -Value $Matches[2]
                Add-Member -InputObject $r -MemberType NoteProperty -Name Version -Value $Matches[3]
            }
            $items += $r 
        }

        $items | Sort -Descending -Property LastModified
    } catch {
        throw $_
    }
}

Function Add-PhotonPlugin {
    <#
    .SYNOPSIS
        Upload private Photon cloud plugin to the Azure BLOB storage

    .PARAMETER Customer
        Plugin customer name, i.e. "ExitGames".

    .PARAMETER Plugin
        Full length exact plugin name, i.e. "ExitGames.Plugin".

    .PARAMETER Key
        Customer personal auth token.

    .PARAMETER File
        Plugin container it self, i.e. plugin.zip

    .EXAMPLE
        Add-PhotonPlugin -Customer ExitGames -Plugin ExitGames.Plugin -File plugin.zip -Key 12345

    .EXAMPLE
        Add-PhotonPlugin -File plugin.zip -SAS https://...
    #>

    [CmdletBinding()]
    param(
        <# PHOTON CLOUD #> 
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(ParameterSetName='RequestSAS', Mandatory)]
        [string]$Customer,
                
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(ParameterSetName='RequestSAS', Mandatory)]
        [string]$Plugin,

        [ValidateNotNullOrEmpty()]
        [parameter(ParameterSetName='RequestSAS', Mandatory)]
        [string]$Key,
                
        [ValidateNotNullOrEmpty()]
        [parameter(Mandatory)]
        [string]$File,
        
        <# AZURE BLOB STORAGE CONNECTIVITY #>
        [ValidateNotNullOrEmpty()]
        [parameter(ParameterSetName='UseSAS', Mandatory)]        
        [string]$SAS,

        [System.TimeSpan]$ServerTimeout,
        [System.TimeSpan]$MaximumExecutionTime,
        [Microsoft.WindowsAzure.Storage.RetryPolicies.IRetryPolicy]$RetryPolicy =
            (New-Object Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry ([System.TimeSpan]::FromSeconds(1)), 3),
        
        <# AZURE BLOB STORAGE PATHS #>
        [ValidateNotNullOrEmpty()]
        [string]$Base = "plugins",

        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[a-z\d\-.]+$", Options = [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)]
        [string]$Container = "public",

        <# Plugin service REST API endpoint #>
        [System.Uri]$Uri = "https://plugins.photonengine.com")

    if (-not($SAS)) {
        if ($Customer -ieq $Plugin) {
            throw [System.ArgumentException] "Plugin and Customer name can't be the same."
        }
    }

    if (-not(Test-Path -Path $File -PathType Leaf)) {
        throw [System.IO.FileNotFoundException] "Plugin $File not found."
    }

    $Container = $Container.ToLowerInvariant()

    # Check archive
    Get-7zAlias    
    $fd = Get-ChildItem -File -Path $File
    $szOutput = sz t $fd.FullName | Out-String
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "7-zip result: $LASTEXITCODE"
        Write-Warning $szOutput        
	    throw [System.ArgumentOutOfRangeException] "Plugin $File have invalid format (must be valid archive)."
    }

    # Obtain SAS
    if (-not($SAS)) {
        try {
            $SAS = Get-PhotonPluginUploadSASUri -Action "upload" -Customer $Customer -Plugin $Plugin `
                -File (Split-Path -Leaf -Path $File) -Key $Key -Uri $Uri
            Write-Verbose "Uploading using SAS: $SAS"
        } catch {
            throw $_
        }
    }
        
    # Upload with SAS
    try {
        $item = New-Object -TypeName Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob `
            -ArgumentList (New-Object -TypeName System.Uri -ArgumentList $SAS)

        # Create the blob client.
        $client = $item.ServiceClient;
        if ($ServerTimeout) { $client.DefaultRequestOptions.ServerTimeout = $ServerTimeout } 
        if ($MaximumExecutionTime) { $client.DefaultRequestOptions.MaximumExecutionTime = $MaximumExecutionTime }
        if ($RetryPolicy) { $client.DefaultRequestOptions.RetryPolicy = $RetryPolicy }            
                        
        try {        
            $stream = [System.IO.File]::OpenRead($fd.FullName)
            $item.UploadFromStream($stream);

            $r = New-Object PSObject -Property @{
                'Name' = $item.Name.Substring($Base.Length + 1)
                'Length' = $item.Properties.Length
                'LastModified' = $item.Properties.LastModified
                'MD5' = $item.Properties.ContentMD5
            } 
            if ($r.Name -match "^([^/]+)/([^/]+)/([^/]+)/.+$") {
                Add-Member -InputObject $r -MemberType NoteProperty -Name Customer -Value $Matches[1]
                Add-Member -InputObject $r -MemberType NoteProperty -Name Plugin -Value $Matches[2]
                Add-Member -InputObject $r -MemberType NoteProperty -Name Version -Value $Matches[3]
            }
            
            Write-Output $r
        } finally {
            $stream.Close()
        }
    } catch {
        throw $_
    }
}

Function Remove-PhotonPluginRange {
    <#
    .SYNOPSIS
        Remove private Photon cloud plugin from the Azure BLOB storage

    .PARAMETER Customer
        Plugin customer name, i.e. "ExitGames".

    .PARAMETER Plugin
        Full length exact plugin name, i.e. "ExitGames.Plugin".

    .PARAMETER Version
        Plugin version to delete.

    .PARAMETER Key
        Customer personal auth token.

    .PARAMETER File
        Plugin container it self, i.e. plugin.zip

    .EXAMPLE
        Remove-PhotonPluginRange -Customer ExitGames -Plugin ExitGames.Plugin -Range 1,10 -Key 12345
    #>

    [CmdletBinding()]
    param(
        <# PHOTON CLOUD #> 
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(Mandatory)]
        [string]$Customer,
                
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(Mandatory)]
        [string]$Plugin,

        [ValidateCount(1,2)]
        [parameter(Mandatory, ValueFromPipeline)]
        [int[]]$Range,
        
        [ValidateNotNullOrEmpty()]
        [parameter(Mandatory)]
        [string]$Key,
        
        [System.TimeSpan]$ServerTimeout,
        [System.TimeSpan]$MaximumExecutionTime,
        [Microsoft.WindowsAzure.Storage.RetryPolicies.IRetryPolicy]$RetryPolicy =
            (New-Object Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry ([System.TimeSpan]::FromSeconds(1)), 3),
        
        <# AZURE BLOB STORAGE PATHS #>
        [ValidateNotNullOrEmpty()]
        [string]$Base = "plugins",

        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[a-z\d\-.]+$", Options = [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)]
        [string]$Container = "public",

        <# Plugin service REST API endpoint #>
        [System.Uri]$Uri = "https://plugins.photonengine.com")

    $Arg = @{
        Customer = $Customer
        Plugin = $Plugin
        Key = $Key
    }
    
    @("ServerTimeout", "MaximumExecutionTime", "RetryPolicy", "Base", "Container", "Uri") | ? {
        $PSBoundParameters.ContainsKey($_)
    } | % {
        $Arg.Add($_, $PSBoundParameters[$_])
    }
    
    if ($Range.Count -notin @(1, 2)) {
        Write-Error "Invalid Range argument: $Range"
        return
    }
    
    Get-PhotonPluginList @Arg | ? {
        if ($Range.Count -eq 1 -and $_.Version -eq $Range[0]) {
            return $_
        } elseif ($Range.Count -eq 2 -and $_.Version -ge $Range[0] -and $_.Version -le $Range[1]) {
            return $_
        }
    } | % {
        Remove-PhotonPlugin @Arg -Version $_.Version
    }
}

Function Remove-PhotonPlugin {
    <#
    .SYNOPSIS
        Remove private Photon cloud plugin from the Azure BLOB storage

    .PARAMETER Customer
        Plugin customer name, i.e. "ExitGames".

    .PARAMETER Plugin
        Full length exact plugin name, i.e. "ExitGames.Plugin".

    .PARAMETER Version
        Plugin version to delete.

    .PARAMETER Key
        Customer personal auth token.

    .PARAMETER File
        Plugin container it self, i.e. plugin.zip

    .EXAMPLE
        Remove-PhotonPlugin -Customer ExitGames -Plugin ExitGames.Plugin -Version 1 -Key 12345

    .EXAMPLE
        Remove-PhotonPlugin -File plugins.zip -SAS https://...
    #>

    [CmdletBinding()]
    param(
        <# PHOTON CLOUD #> 
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(ParameterSetName='RequestSAS', Mandatory)]
        [string]$Customer,
                
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(ParameterSetName='RequestSAS', Mandatory)]
        [string]$Plugin,

        [ValidateRange(1,[int]::MaxValue)]
        [parameter(ParameterSetName='RequestSAS', Mandatory)]
        [parameter(Mandatory)]
        [int]$Version,

        [parameter(ParameterSetName='RequestSAS')]
        [string]$File,

        [ValidateNotNullOrEmpty()]
        [parameter(ParameterSetName='RequestSAS', Mandatory)]
        [string]$Key,
        
        <# AZURE BLOB STORAGE CONNECTIVITY #>
        [ValidateNotNullOrEmpty()]
        [parameter(ParameterSetName='UseSAS', Mandatory)]        
        [string]$SAS,

        [System.TimeSpan]$ServerTimeout,
        [System.TimeSpan]$MaximumExecutionTime,
        [Microsoft.WindowsAzure.Storage.RetryPolicies.IRetryPolicy]$RetryPolicy =
            (New-Object Microsoft.WindowsAzure.Storage.RetryPolicies.ExponentialRetry ([System.TimeSpan]::FromSeconds(1)), 3),
        
        <# AZURE BLOB STORAGE PATHS #>
        [ValidateNotNullOrEmpty()]
        [string]$Base = "plugins",

        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[a-z\d\-.]+$", Options = [System.Text.RegularExpressions.RegexOptions]::CultureInvariant)]
        [string]$Container = "public",

        <# Plugin service REST API endpoint #>
        [System.Uri]$Uri = "https://plugins.photonengine.com")

    if (-not($SAS)) {
        if ($Customer -ieq $Plugin) {
            throw [System.ArgumentException] "Plugin and Customer name can't be the same."
        }
    }
    
    $Container = $Container.ToLowerInvariant()

    # Obtain SAS
    if (-not($SAS)) {
        try {
            $SAS = Get-PhotonPluginUploadSASUri -Action "remove" `
                -Customer $Customer -Plugin $Plugin -Version $Version `
                -File $File -Key $Key -Uri $Uri
            Write-Verbose "Removing using SAS: $SAS"
        } catch {
            throw $_
        }
    }
        
    # Upload with SAS
    try {
        $item = New-Object -TypeName Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob `
            -ArgumentList (New-Object -TypeName System.Uri -ArgumentList $SAS)

        # Create the blob client.
        $options = New-Object Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions
        if ($ServerTimeout) { $options.ServerTimeout = $ServerTimeout } 
        if ($MaximumExecutionTime) { $options.MaximumExecutionTime = $MaximumExecutionTime }
        if ($RetryPolicy) { $options.RetryPolicy = $RetryPolicy }            
        
        $result = $item.DeleteIfExists(
            [Microsoft.WindowsAzure.Storage.Blob.DeleteSnapshotsOption]::IncludeSnapshots,
            [Microsoft.WindowsAzure.Storage.AccessCondition]::GenerateEmptyCondition(),
            $options,
            (New-Object Microsoft.WindowsAzure.Storage.OperationContext))
                        
        $r = New-Object PSObject -Property @{
            'Name' = $item.Name.Substring($Base.Length + 1)
            'Result' = $result
        } 
        if ($r.Name -match "^([^/]+)/([^/]+)/([^/]+)/.+$") {
            Add-Member -InputObject $r -MemberType NoteProperty -Name Customer -Value $Matches[1]
            Add-Member -InputObject $r -MemberType NoteProperty -Name Plugin -Value $Matches[2]
            Add-Member -InputObject $r -MemberType NoteProperty -Name Version -Value $Matches[3]
        }
            
        Write-Output $r
    } catch {
        throw $_
    }
}

Function Get-PhotonPluginUploadSASUri { 
<#
    .SYNOPSIS
        Get plugin upload Azure BLOB storage SAS Uri

    .PARAMETER Action
        SAS action: upload, remove.

    .PARAMETER Customer
        Plugin customer name, i.e. "ExitGames".

    .PARAMETER Plugin
        Full length exact plugin name, i.e. "ExitGames.Plugin".

    .PARAMETER File
        Plugin container it self, i.e. plugins.zip

    .PARAMETER Key
        Customer personal auth token.

    .EXAMPLE
        Get-PhotonPluginUploadSASUri -Customer ExitGames -Plugin ExitGames.Plugin -Key 12345

    #>

    [CmdletBinding()]
    param(
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d]+$")]
        [string]$Action = "upload",

        <# PHOTON CLOUD #> 
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(Mandatory)]
        [string]$Customer,
                
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(Mandatory)]
        [string]$Plugin,

        [ValidateRange(1,[int]::MaxValue)]
        [int]$Version,

        [ValidatePattern("^(|[\w\d][\w\d\-_.]+)$")]
        [string]$File,

        [ValidateNotNullOrEmpty()]
        [parameter(Mandatory)]
        [string]$Key,
                
        <# Plugin service REST API endpoint #>
        [System.Uri]$Uri = "https://plugins.photonengine.com")

    $Url = $Uri.Scheme + "://" + $Uri.Host + ":" + $Uri.Port + "/plugin/$Action/$Customer/$Plugin"
    if ($Version) { $Url += "/$Version" }
    if ($File) { $Url += "/$File" }
    $Url += "?key=$Key"

    $request = @{
        Uri = New-Object System.Uri($Url)
        Method = [Microsoft.PowerShell.Commands.WebRequestMethod]::POST
    }

    if (![System.String]::IsNullOrEmpty($Uri.UserInfo)) {
    Write-Host $Uri
        $request.Add("Headers", @{
            Authorization=("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($Uri.UserInfo)))
        })
    } 
        
    return Invoke-RestMethod @request
}

Function Get-PhotonPluginStatus {
    <#
    .SYNOPSIS
        Get status of specific plugin synchronization along the Photon cloud.
            
    .PARAMETER Customer
        Plugin customer name, i.e. "ExitGames".

    .PARAMETER Plugin
        Full length exact plugin name, i.e. "ExitGames.Plugin"

    .PARAMETER Version
        Plugin version number, i.e. 10, 11, 12, ...

    .PARAMETER Key
        Customer personal auth token.

    .EXAMPLE
        Get-PhotonPluginStatus -Customer ExitGames -Plugin ExitGames.Plugin -Version 10 -Key 12345
    #>

    [CmdletBinding()]
    param(
        <# PHOTON CLOUD #>
        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(Mandatory)]
        [string]$Customer,

        [ValidateNotNullOrEmpty()]
        [ValidatePattern("^[\w\d][\w\d\-_.]+$")]
        [parameter(Mandatory)]
        [string]$Plugin,

        [ValidateRange(1,[int]::MaxValue)]
        [parameter(Mandatory)]
        [int]$Version,

        [ValidateNotNullOrEmpty()]
        [parameter(Mandatory)]
        [string]$Key,
        
        <# Plugin service REST API endpoint #>
        [System.Uri]$Uri = "https://plugins.photonengine.com")

    $request = @{
        Uri = New-Object System.Uri($Uri.Scheme + "://" + $Uri.Host + ":" + $Uri.Port + "/plugin/status/$Customer/$Plugin/$Version`?key=$Key")
        Method = [Microsoft.PowerShell.Commands.WebRequestMethod]::Get
    }

    if (![System.String]::IsNullOrEmpty($Uri.UserInfo)) {
        $request.Add("Headers", @{
            Authorization=("Basic {0}" -f [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($Uri.UserInfo)))
        })
    }

    Invoke-RestMethod @request
}

Export-ModuleMember -Function Add-PhotonPlugin
Export-ModuleMember -Function Get-PhotonPluginList
Export-ModuleMember -Function Get-PhotonPluginStatus
Export-ModuleMember -Function Remove-PhotonPlugin
Export-ModuleMember -Function Remove-PhotonPluginRange
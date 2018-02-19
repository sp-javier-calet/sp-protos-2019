Import-Module ..\..\..\Assets\Plugins\Sparta\Hidden~\Standalone\Photon\Server\sas-client\Photon.PrivateCloud.Plugin.Client.SAS.psm1

$msbuildPath = "C:\Windows\Microsoft.NET\Framework\v4.0.30319\"
$basePath = "."
$solutionName = "LockstepGameLogic"
$solutionPath = "$basePath\$solutionName.sln"
$buildPath = "$basePath\bin"
$customer = "Socialpoint"
$key = "66QFur7XJcyRsCXxuk7yrtwDHw8Xsne9"
$plugin = "LockstepPlugin"

Write-Host "building server solution..."
$p = Start-Process -FilePath $msbuildPath\MSBuild.exe -ArgumentList $solutionPath,"/t:Clean /p:Configuration=Release" -PassThru -NoNewWindow
$null = $p.WaitForExit(-1)
$p = Start-Process -FilePath $msbuildPath\MSBuild.exe -ArgumentList $solutionPath,"/t:Rebuild /p:Configuration=Release" -PassThru -NoNewWindow
$null = $p.WaitForExit(-1)
Write-Host ""
Write-Host "server solution ready!"
Write-Host "creating plugin package..."
..\..\..\Assets\Plugins\Sparta\Hidden~\Standalone\Photon\Server\sas-client\7z.exe a -tzip archive.zip "$buildPath" -xr!"*.meta"
Write-Host "package created!"
Write-Host "uploading package..."
Add-PhotonPlugin -Customer $customer -Plugin $plugin -File .\archive.zip -Key $key
Write-Host "package uploaded!"

Import-Module ..\Server\sas-client\Photon.PrivateCloud.Plugin.Client.SAS.psm1

$vsPath = "C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE"
$basePath = "."
$solutionName = "AuthoritativeGameLogic"
$solutionPath = "$basePath\$solutionName.sln"
$buildPath = "$basePath\bin"
$customer = "Socialpoint"
$key = "66QFur7XJcyRsCXxuk7yrtwDHw8Xsne9"
$plugin = "AuthoritativePlugin"

Write-Host "building server solution..."
$p = Start-Process -FilePath $vsPath\devenv.exe -ArgumentList $solutionPath,"/Rebuild Release" -PassThru
$null = $p.WaitForExit(-1)
Write-Host ""
Write-Host "server solution ready!"
Write-Host "creating plugin package..."
Compress-Archive -Path $buildPath -DestinationPath archive.zip -Force
Write-Host "package created!"
Write-Host "uploading package..."
Add-PhotonPlugin -Customer $customer -Plugin $plugin -File .\archive.zip -Key $key
Write-Host "package uploaded!"

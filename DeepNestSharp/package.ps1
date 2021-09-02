Remove-Item -LiteralPath "bin\Release\net5.0-windows" -Force -Recurse

& dotnet tool install --global GitVersion.Tool

& dotnet restore
$str = dotnet-gitversion /updateprojectfiles | out-string
$json = ConvertFrom-Json $str
$fullSemVer = $json.FullSemVer

#Write-Host $fullSemVer
& dotnet publish -r win-x64 -c Release --self-contained -p:PublishSingleFile=true
Compress-Archive -Path bin\Release\net5.0-windows\win-x64\publish\* -DestinationPath "bin\Release\net5.0-windows\DeepNestSharp.x64-v${fullSemVer}.zip"
& dotnet publish -r win-x86 -c Release --self-contained -p:PublishSingleFile=true
Compress-Archive -Path bin\Release\net5.0-windows\win-x86\publish\* -DestinationPath "bin\Release\net5.0-windows\DeepNestSharp.x86-v${fullSemVer}.zip"
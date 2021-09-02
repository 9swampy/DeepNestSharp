Remove-Item -LiteralPath "DeepNestSharp\bin\Release\net5.0-windows" -Force -Recurse

& dotnet tool install --global GitVersion.Tool

$str = dotnet-gitversion /updateprojectfiles | out-string
$json = ConvertFrom-Json $str
$semVer = $json.SemVer
$fullSemVer = $json.FullSemVer

#Write-Host $fullSemVer
& dotnet restore -r win-x64
& dotnet build -c Release
& dotnet publish -r win-x64 -c Release --self-contained -p:PublishSingleFile=true
Compress-Archive -Path DeepNestSharp\bin\Release\net5.0-windows\win-x64\publish\* -DestinationPath "DeepNestSharp\bin\Release\net5.0-windows\DeepNestSharp.x64-v${semVer}.zip"
& dotnet restore -r win-x86
& dotnet build -c Release
& dotnet publish -r win-x86 -c Release --self-contained -p:PublishSingleFile=true
Compress-Archive -Path DeepNestSharp\bin\Release\net5.0-windows\win-x86\publish\* -DestinationPath "DeepNestSharp\bin\Release\net5.0-windows\DeepNestSharp.x86-v${semVer}.zip"
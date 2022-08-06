set-location C:\Git\DeepNestSharp

Remove-Item -LiteralPath "DeepNestSharp\bin\Release\net6.0-windows" -Force -Recurse

& dotnet tool install --global GitVersion.Tool

#$str = dotnet-gitversion /updateprojectfiles | out-string
$str = dotnet-gitversion | out-string
$json = ConvertFrom-Json $str
$semVer = $json.SemVer
$fullSemVer = $json.FullSemVer

Write-Host $fullSemVer
& dotnet restore -r win-x64
& dotnet build -c Release
cd DeepNestSharp
& dotnet publish -r win-x64 -c Release --self-contained #-p:PublishSingleFile=true
cd ..
compress-Archive -Path DeepNestSharp\bin\Release\net6.0-windows\win-x64\publish\* -DestinationPath "DeepNestSharp\bin\Release\DeepNestSharp.x64-v${fullSemVer}.zip" -Force
& dotnet restore -r win-x86
& dotnet build -c release
cd deepnestsharp
& dotnet publish -r win-x86 -c release --self-contained #-p:publishsinglefile=true
cd ..
compress-Archive -Path DeepNestSharp\bin\Release\net6.0-windows\win-x86\publish\* -DestinationPath "DeepNestSharp\bin\Release\DeepNestSharp.x86-v${fullSemVer}.zip" -Force

#https://unix.stackexchange.com/questions/155046/determine-if-git-working-directory-is-clean-from-a-script
#if [ -z "$(git status --porcelain)" ]; then 
#  # Working directory clean
#else 
#  # Uncommitted changes
#fi
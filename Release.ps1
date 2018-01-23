$scriptpath = $MyInvocation.MyCommand.Path
$dir = Split-Path $scriptpath

Compress-Archive -Path $dir\Hadooper\bin\Release\Hadooper.exe -DestinationPath .\..\..\..\Hadooper.zip
Compress-Archive -Path $dir\Hadooper\bin\Release\Renci.SshNet.dll -Update -DestinationPath .\..\..\..\Hadooper.zip
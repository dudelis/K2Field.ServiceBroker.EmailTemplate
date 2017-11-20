Import-Module "$PSScriptRoot\K2HelperModule.psm1" -Force

$guid = [guid]"99b22dd4-50a1-4ab4-aa91-80819e953a05" 
$brokerName = "K2Field.ServiceBroker.EmailTemplate"
$brokerSystemName = $brokerName
$brokerDLL = "K2Field.ServiceBroker.EmailTemplate.dll"
$displayName = $brokerName -replace '\.','_'
$brokerDescription = "The Email Template Service Broker. For more information, see https://github.com/dudelis/K2Field.ServiceBroker.EmailTemplate"
$k2ServerName = "localhost"
$port = 5555
$targetPath = GetK2InstallPath -machine $k2ServerName
$packageName = "K2Field.EmailTemplate.PND.kspx"
$packageXml = "K2Field.EmailTemplate.PND.xml"

StopK2Service
Write-Host "`n `n Copying $brokerDLL to the ServiceBroker folder `n"
Copy-Item "$PSScriptRoot\$brokerDLL" -Destination "$targetPath\ServiceBroker"
StartK2Service

Write-Host "`n `n Registering Service Type with name $displayName `n"
RegisterServiceType -ServiceTypeDLL $brokerDLL -guid $guid -systemName $brokerSystemName -displayName $displayName -description $brokerDescription -k2server $k2ServerName -port $port

Write-Host "`n `n Starting deployment of the package `n"
DeployPackage -PackageName $packageName -XmlName $packageXml -k2server $k2ServerName -port $port
Write-Host "`n Deployment of the package finished. You can check the deployment status in the log, created in the script folder.`n"
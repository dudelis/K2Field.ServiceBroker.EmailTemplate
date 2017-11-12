#If a GUID is found that is the same, it will "overwrite" the service type.
$guid = [guid]"4103e312-6620-4911-af9f-9d8af8816ab1"
$brokerName = "K2Field.ServiceBroker.EmailTemplate"
$brokerSystemName = $brokerName
$brokerDLL = "K2Field.ServiceBroker.EmailTemplate.dll"
$displayName = $brokerName -replace '\.',' '
$brokerDescription = "The Email Template Service Broker. For more information, see https://github.com/dudelis/K2Field.ServiceBroker.EmailTemplate"

Function StartK2Service() {
	Write-Host  "Starting K2 blackpearl service"
	$job = Start-Job -ScriptBlock {
		Get-Service -DisplayName 'K2 blackpearl Server' | where-object {$_.Status -ne "Running"} | Start-Service
	}
	Wait-Job $job
	Receive-Job $job
}

Function StopK2Service() {
    Write-Host "Stopping K2 blackpearl service"

	$job = Start-Job -ScriptBlock {
		Get-Service -DisplayName 'K2 blackpearl Server' | where-object {$_.Status -eq "Running"} | Stop-Service -Confirm:$false -Force 
		Stop-Process -ProcessName "K2HostServer"  -force -ErrorAction SilentlyContinue -Confirm:$false
	}
	Wait-Job $job
	Receive-Job $job
}

Function GetK2InstallPath([string]$machine = $env:computername) {
    $registryKeyLocation = "SOFTWARE\SourceCode\BlackPearl\blackpearl Core\"
    $registryKeyName = "InstallDir"

	Write-Debug "Getting K2 install path from $machine "
    
    $reg = [Microsoft.Win32.RegistryKey]::OpenRemoteBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, $machine)
    $regKey= $reg.OpenSubKey($registryKeyLocation)
    $installDir = $regKey.GetValue($registryKeyName)
    return $installDir
}

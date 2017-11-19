# Starts K2 Blackpearl Server Service
# Added new name for the K2 Service, valid for K2 Five
Function StartK2Service() {
	Write-Host  "Starting K2 blackpearl service"
	$job = Start-Job -ScriptBlock {
		Get-Service | where-object {($_.DisplayName -eq "K2 blackpearl Server" -or $_.DisplayName -eq "K2 Server") -and $_.Status -ne "Running"} | Start-Service
	}
	Wait-Job $job
	Receive-Job $job
}

# Stops K2 Blackpearl Server Service
# Added new name for the K2 Service, valid for K2 Five
Function StopK2Service() {
    Write-Host "Stopping K2 blackpearl service"

	$job = Start-Job -ScriptBlock {
		Get-Service | where-object {($_.DisplayName -eq "K2 blackpearl Server" -or $_.DisplayName -eq "K2 Server") -and $_.Status -eq "Running"} | Stop-Service -Confirm:$false -Force 
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
# Returns the version number of the installer of K2
Function GetK2VersionNumber([string]$machine = $env:computername) {
    $registryKeyLocation = "SOFTWARE\SourceCode\Installer\"
    $registryKeyName = "Version"

	Write-Debug "Getting the version of the K2 Installer from $machine "
    
    $reg = [Microsoft.Win32.RegistryKey]::OpenRemoteBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine, $machine)
    $regKey= $reg.OpenSubKey($registryKeyLocation)
    $installDir = $regKey.GetValue($registryKeyName)
    return $installDir
}

Function GetK2ConnectionString([string]$k2Server = "localhost", [int]$port = 5555) {
    Write-Debug "Creating connectionstring for machine '$k2Server' and port '$port'";
	$hostClientApiPath = "$PSScriptRoot\Assemblies\SourceCode.HostClientAPI.dll"
	[Reflection.Assembly]::LoadFile($hostClientApiPath) | Out-Null
		
    $connString = New-Object -TypeName "SourceCode.Hosting.Client.BaseAPI.SCConnectionStringBuilder";
    $connString.Host = $k2Server;
    $connString.Port = $port;
	$connString.Integrated = $true;
    $connString.IsPrimaryLogin = $true;
	$connString.CachePassword = $false;
	
	return $connString.ConnectionString;    
}

Function RegisterServiceType([string]$ServiceTypeDLL, [guid]$guid, [string]$systemName, [string]$displayName, [string]$description = "", [string]$k2Server = "localhost", [int]$port = 5555) {
    # Get Paths for local environment and for the remote machine, we might run this installer from a simple windows 7 host, while we deploy to a server that has a different drive...
    $k2Path = GetK2InstallPath
    $remK2Path = GetK2InstallPath -machine $k2Server
    $smoManServiceAssembly = "$PSScriptRoot\Assemblies\SourceCode.SmartObjects.Services.Management.dll"
    $serviceBrokerAssembly = $remK2Path + "ServiceBroker\$ServiceTypeDLL"
	$className = "K2Field.ServiceBroker.EmailTemplate.EmailTemplateServiceBroker";
	
    Write-Debug "Adding/Updating ServiceType $serviceBrokerAssembly with guid $guid"
	Write-Debug  "ServiceBrokerAssembly: $serviceBrokerAssembly"
    
    [Reflection.Assembly]::LoadFile($smoManServiceAssembly) | Out-Null
    $smoManService = New-Object SourceCode.SmartObjects.Services.Management.ServiceManagementServer

    #Create connection and capture output (methods return a bool)
    $tmpOut = $smoManService.CreateConnection()
	$k2ConnectionString = GetK2ConnectionString -k2Server $k2Server -port $port;
    $tmpOut = $smoManService.Connection.Open($k2ConnectionString);
    Write-Debug "Connected to K2 host server"

    # Check if we need to update or register a new one...
    if ([string]::IsNullOrEmpty($smoManService.GetServiceType($guid)) ) {
        Write-Debug "Registering new service type..."
        $tmpOut = $smoManService.RegisterServiceType($guid, $systemName, $displayName, $description, $serviceBrokerAssembly, $className);
        write-debug "Registered new service type..."
    } else {
        Write-Debug "Updating service type..."
        $tmpOut = $smoManService.UpdateServiceType($guid, $systemName, $displayName, $description, $serviceBrokerAssembly, $className);
        Write-Debug "Updated service type..."
    }
    $smoManService.Connection.Close();
    write-host "Deployed service-type"
}

Function DeployPackage ([string]$PackageName, [string]$XmlName, [string]$serverName = "localhost", [int]$port=5555){
	Add-PSSnapin SourceCode.Deployment.PowerShell
	$k2ConnectionString = GetK2ConnectionString -k2Server $k2Server -port $port;
	Deploy-Package -FileName "$PSScriptRoot\$PackageName" -ConfigFile "$PSScriptRoot\$XmlName" -ConnectionString $k2ConnectionString

}

Export-ModuleMember -Function StartK2Service, StopK2Service, GetK2InstallPath, RegisterServiceType, GetK2VersionNumber
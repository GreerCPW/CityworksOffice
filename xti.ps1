Import-Module PowershellForXti -Force

function Xti-AddExpandedCityworksDbMigration {
	param ([Parameter(Mandatory)]$Name)
	$env:DOTNET_ENVIRONMENT="Development"
	dotnet ef --startup-project ./CityworksOfficeWebApp/Internal/CPW_ExpandedCityworksDbTool migrations add $Name --project ./CityworksOfficeWebApp/Internal/CPW_ExpandedCityworksDB.SqlServer
}

function Xti-UpdateExpandedCityworksDb {
	param (
        [ValidateSet("Production", "Development", "Staging", "Test")]
        [Parameter(Mandatory, ValueFromPipelineByPropertyName = $true)]
        $EnvName
	)
	dotnet run --project ./CityworksOfficeWebApp/Internal/CPW_ExpandedCityworksDbTool -- --environment $EnvName --command Update
}

function Xti-RemoveLastExpandedCityworksDbMigration {
	param ()
	$env:DOTNET_ENVIRONMENT="Development"
	dotnet ef --startup-project ./CityworksOfficeWebApp/Internal/CPW_ExpandedCityworksDBTool migrations remove --project ./CityworksOfficeWebApp/Internal/CPW_ExpandedCityworksDB.SqlServer
}

function Xti-UpdateNpm {
	Start-Process -FilePath "cmd.exe" -WorkingDirectory CityworksOfficeWebApp/Apps/CityworksOfficeWebApp -ArgumentList "/c", "npm install @jasonbenfield/sharedwebapp@latest"
}
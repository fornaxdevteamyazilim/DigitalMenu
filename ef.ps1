param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("add", "update", "remove", "list")]
    [string]$Action,

    [Parameter(Position = 1)]
    [string]$Name
)

$project = "src/DigitalMenu.Infrastructure"
$startup = "src/DigitalMenu.API"

switch ($Action) {
    "add" {
        if ([string]::IsNullOrWhiteSpace($Name)) {
            Write-Error "Migration adi gerekli. Ornek: .\ef.ps1 add AddProductsTable"
            exit 1
        }
        dotnet ef migrations add $Name --project $project --startup-project $startup
    }
    "update" {
        dotnet ef database update --project $project --startup-project $startup
    }
    "remove" {
        dotnet ef migrations remove --project $project --startup-project $startup
    }
    "list" {
        dotnet ef migrations list --project $project --startup-project $startup
    }
}

# Script khởi chạy Backend Souvenir POS với .NET 10
$dotnet10 = "C:\Users\Admin\dotnet10\dotnet.exe"
if (Test-Path $dotnet10) {
    Write-Host ">>> Khởi chạy Backend với .NET 10 SDK ($dotnet10)..." -ForegroundColor Cyan
    & $dotnet10 run --project src/Souvenir.Api/Souvenir.Api.csproj
} else {
    Write-Host ">>> Khởi chạy Backend với dotnet mặc định..." -ForegroundColor Yellow
    dotnet run --project src/Souvenir.Api/Souvenir.Api.csproj
}

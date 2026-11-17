@echo off
if exist "C:\Users\Admin\dotnet10\dotnet.exe" (
    echo >>> Dang khoi chay Backend bang .NET 10 SDK...
    "C:\Users\Admin\dotnet10\dotnet.exe" run --project src/Souvenir.Api/Souvenir.Api.csproj
) else (
    echo >>> Dang khoi chay Backend bang dotnet he thong...
    dotnet run --project src/Souvenir.Api/Souvenir.Api.csproj
)
pause

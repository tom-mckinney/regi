cd src\Regi

dotnet pack -c Release --output ./

dotnet nuget push ./ --source https://artifactory.platform.manulife.io/artifactory/api/nuget/nuget/

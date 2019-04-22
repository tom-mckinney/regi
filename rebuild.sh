cd src/Regi

dotnet pack --output ./ --source https://artifactory.platform.manulife.io/artifactory/api/nuget/nuget

dotnet tool uninstall -g regi

dotnet tool install -g regi --add-source ./

exit 0
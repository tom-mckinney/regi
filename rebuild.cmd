cd src\Regi

del Regi.*.nupkg

dotnet pack --output ./

dotnet tool uninstall -g regi

dotnet tool install -g regi --add-source ./

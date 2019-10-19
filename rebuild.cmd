cd src\Regi

del Regi.*.nupkg

dotnet pack --version-suffix dev --output ./

dotnet tool uninstall -g regi

dotnet tool install -g --version 1.0.0-dev --add-source .\ --ignore-failed-sources regi

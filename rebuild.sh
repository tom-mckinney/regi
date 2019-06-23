cd src/Regi

rm Regi.*.nupkg

dotnet pack --output ./

dotnet tool uninstall -g regi

dotnet tool install -g regi --add-source ./ --ignore-failed-sources

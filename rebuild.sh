cd src/Regi

dotnet pack --output ./

dotnet tool uninstall -g regi

dotnet tool install -g regi --add-source ./

exit 0
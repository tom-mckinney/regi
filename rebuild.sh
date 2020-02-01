rm -r ./nupkgs/ &> /dev/null

export VERSION="1.0.0-dev"

dotnet pack --output ./nupkgs/ ./src/Regi.CommandLine

dotnet tool uninstall -g regi

dotnet tool install -g regi --add-source ./nupkgs/ --ignore-failed-sources --version $VERSION

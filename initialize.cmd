dotnet pack --output nupkgs .\src\Regi

dotnet tool uninstall -g regi

dotnet tool install -g regi --add-source .\

regi install --configuration .\src\Regi.Test\_SampleProjects_

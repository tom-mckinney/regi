cd src\Regi
del Regi.*.nupkg
dotnet pack --output ./
copy Regi.*.nupkg ..\Regi.Test\_SampleProjects_\

cd ..\Regi.Test\_SampleProjects_
docker rmi regi-e2e || echo "regi-e2e image not present"
docker build -t regi-e2e .
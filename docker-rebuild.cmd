del src\Regi\Regi.*.nupkg
del src\Regi.Test\_SampleProjects_\Regi.*.nupkg

cd src\Regi
dotnet pack --output ./
copy Regi.*.nupkg ..\Regi.Test\_SampleProjects_\

cd ..\Regi.Test\_SampleProjects_
docker rmi regi-sample || echo "regi-sample image not present"
docker build -t regi-sample .
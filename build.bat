@echo off
set /p version="Enter Version Number to Build With: "

@echo on
dotnet pack ".\EnumerableAsyncProcessorNs\EnumerableAsyncProcessorNs.csproj"  --configuration Release /p:Version=%version%

pause
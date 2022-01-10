@echo off
dotnet clean
dotnet pack -o %LocalNugetPath%
rmdir /Q /S %USERPROFILE%\.nuget\packages\compiletimeproxygenerator
dotnet clean
dotnet test
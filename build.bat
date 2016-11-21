@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

REM Restore NuGet
call ".nuget\nuget.exe" restore TextAnalytics.sln

REM Build
"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" TextAnalytics.sln /p:Configuration="%config%" /p:Platform="Any CPU" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false

REM Package
mkdir Build
call ".nuget\nuget.exe" pack "Zanganeh.EPiServer.Addon.TextAnalytics\Zanganeh.EPiServer.Addon.TextAnalytics.csproj" -IncludeReferencedProjects -o Build -p Configuration=%config% %version%
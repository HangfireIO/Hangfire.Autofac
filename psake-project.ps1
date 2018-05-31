Framework 4.5.1
Include "packages\Hangfire.Build.0.2.6\tools\psake-common.ps1"

Task Default -Depends Collect

Task RestoreCore {
    Exec { 
        dotnet clean
        dotnet restore 
    }
}

Task CompileCore -Depends RestoreCore {
    Exec { dotnet build -c Release }
}

Task Test -Depends CompileCore -Description "Run unit and integration tests." {
    Exec { dotnet test "tests\Hangfire.Autofac.Tests\Hangfire.Autofac.Tests.csproj" }
}

Task Collect -Depends Test -Description "Copy all artifacts to the build folder." {
    Collect-Assembly "Hangfire.Autofac" "net45"
    Collect-Assembly "Hangfire.Autofac" "netstandard1.3"
    Collect-File "LICENSE"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-PackageVersion

    Create-Archive "Hangfire.Autofac-$version"
    Create-Package "Hangfire.Autofac" $version
}
Include "packages\Hangfire.Build.0.3.3\tools\psake-common.ps1"

Task Default -Depends Pack

Task Test -Depends Compile -Description "Run unit and integration tests." {
    Exec { dotnet test -c release --no-build "tests\Hangfire.Autofac.Tests" }
}

Task Collect -Depends Test -Description "Copy all artifacts to the build folder." {
    Collect-Assembly "Hangfire.Autofac" "net45"
    Collect-Assembly "Hangfire.Autofac" "netstandard1.3"
    Collect-Assembly "Hangfire.Autofac" "netstandard2.0"
    Collect-File "LICENSE"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-PackageVersion

    Create-Archive "Hangfire.Autofac-$version"
    Create-Package "Hangfire.Autofac" $version
}
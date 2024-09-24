Include "packages\Hangfire.Build.0.5.0\tools\psake-common.ps1"

Task Default -Depends Pack

Task Test -Depends Compile -Description "Run unit and integration tests." {
    Exec { dotnet test -c release --no-build "tests\Hangfire.Autofac.Tests" }
}

Task Collect -Depends Test -Description "Copy all artifacts to the build folder." {
    Collect-Assembly "Hangfire.Autofac" "net45"
    Collect-Assembly "Hangfire.Autofac" "netstandard1.3"
    Collect-Assembly "Hangfire.Autofac" "netstandard2.0"
    Collect-File "LICENSE"
    Collect-File "README.md"
}

Task Pack -Depends Collect -Description "Create NuGet packages and archive files." {
    $version = Get-PackageVersion

    Create-Package "Hangfire.Autofac" $version
    Create-Archive "Hangfire.Autofac-$version"
}

Task Sign -Depends Pack -Description "Sign artifacts." {
    $version = Get-PackageVersion

    Sign-ArchiveContents "Hangfire.Autofac-$version" "hangfire"
}
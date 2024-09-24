
# Hangfire.Autofac

[![NuGet](https://img.shields.io/nuget/v/Hangfire.Autofac.svg)](https://www.nuget.org/packages/Hangfire.Autofac) [![Build status](https://ci.appveyor.com/api/projects/status/oncvxlqtnake9c86?svg=true)](https://ci.appveyor.com/project/hangfireio/hangfire-autofac) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=HangfireIO_Hangfire.Autofac&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=HangfireIO_Hangfire.Autofac) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=HangfireIO_Hangfire.Autofac&metric=bugs)](https://sonarcloud.io/summary/new_code?id=HangfireIO_Hangfire.Autofac) [![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=HangfireIO_Hangfire.Autofac&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=HangfireIO_Hangfire.Autofac) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=HangfireIO_Hangfire.Autofac&metric=coverage)](https://sonarcloud.io/summary/new_code?id=HangfireIO_Hangfire.Autofac)

[Autofac](https://autofac.org) integration for [Hangfire](https://www.hangfire.io). Provides an implementation of the `JobActivator` class and registration extensions, allowing you to use Autofac container to **resolve job type instances** as well as **control the lifetime** of the all related dependencies.

*Hangfire.Autofac* resolves service instances using a child, tagged [lifetime scope](https://docs.autofac.org/en/latest/lifetime/index.html). A child scope is created and disposed each time when background job processing takes place, so you have precise control of your service's lifetime, including **shared instances** and **deterministic disposal**.

## Installation

*Hangfire.Autofac* is available as a NuGet Package. Type the following command into NuGet Package Manager Console window to install it:

```
> dotnet add package Hangfire.Autofac
```

## Usage

The package provides an extension methods for the `IGlobalConfiguration` interface, so you can enable Autofac integration using the `GlobalConfiguration` class:

```csharp
var builder = new ContainerBuilder();
// builder.Register...

GlobalConfiguration.Configuration.UseAutofacActivator(builder.Build());
```

After invoking the `UseAutofacActivator` method, Autofac-based implementation of the `JobActivator` class will be used to resolve job type instances during the background job processing.

### Shared Components

Sometimes it is required to share the same service instance for different components, such as database connection, unit of work, etc. *Hangfire.Autofac* allows you to share them in a scope, limited to the **current** background job processing, just call the `InstancePerBackgroundJob` method in your component registration logic:

```csharp
builder.RegisterType<Database>().InstancePerBackgroundJob();
```

### Non-tagged scopes

Whenever the scopes in `AutofacActivator` are created, by default they are created using tag `BackgroundJobScope`. There might be a scenario when it is needed not to use tagged scopes though. This might be a case if it is required to have a new instance of every service for each lifetime scope (in Hangfire it would be for every job). To disable tagged scopes you can use a flag `useTaggedLifetimeScope` during initialization of `AutofacActivator` for Hangfire.

```csharp
var builder = new ContainerBuilder();
// builder.Register...

GlobalConfiguration.Configuration.UseAutofacActivator(builder.Build(), false);
```

Then you can register services by using `InstancePerLifetimeScope` and expect them to work like intended.

```csharp
builder.RegisterType<Database>().InstancePerLifetimeScope();
```

### Deterministic Disposal

The *child lifetime scope* is disposed as soon as current background job is performed, successfully or with an exception. Since *Autofac* automatically disposes all the components that implement the `IDisposable` interface (if this feature not disabled), all of the resolved components will be disposed *if appropriate*.

For example, the following components will be **disposed automatically**:

```csharp
builder.RegisterType<SmsService>();
builder.RegisterType<EmailService>().InstancePerDependency();
builder.RegisterType<Database>().InstancePerBackgroundJob();
```

And the following components **will not be disposed**:

```csharp
builder.RegisterType<BackgroundJobClient>().SingleInstance();
builder.RegisterType<MyService>().ExternallyOwned();
```

Please refer to the Autofac documentation to learn more about [Automatic Disposal](https://docs.autofac.org/en/latest/lifetime/disposal.html#automatic-disposal) feature.

### Registering With Multiple Lifetime Scopes

Services registered with tagged lifetime scopes (eg `InstancePerBackgroundJob`, Autofac's `InstancePerRequest` or a scope your specific application requires) will not resolve outside of these named scopes, a common situation is when using Hangfire in an ASP.NET web application. In these situations you must register all your lifetimescopes together if you want the services to be resolved from any of the scopes. Hangfire.Autofac exposes it's lifetime tag and an overload of `InstancePerBackgroundJob` to help you do this.

To register a service with both Autofac's PerRequest and Hangfire's PerBackgroundJob you could do any of the following:

Passing Hangfire's scope tag to Autofac's `InstancePerHttpRequest`:
```csharp
builder.RegisterType<SharedService>().InstancePerHttpRequest(AutofacJobActivator.LifetimeScopeTag);
```

From Autofac 3.4.0 Autofac exposed their lifetime tag, `MatchingScopeLifetimeTags.RequestLifetimeScopeTag`, which can be used with `InstancePerBackgroundJob`:
```csharp
builder.RegisterType<SharedService>().InstancePerBackgroundJob(MatchingScopeLifetimeTags.RequestLifetimeScopeTag);
```

Both scope tags can also be used directly with Autofac's `InstancePerMatchingLifetimeScope`
```csharp
var requestTag = MatchingScopeLifetimeTags.RequestLifetimeScopeTag;
var jobTag = AutofacJobActivator.LifetimeScopeTag;
builder.RegisterType<SharedService>().InstancePerMatchingLifetimeScope(requestTag, jobTag);
```

### Mixed Lifetime Scopes

Beaware that if you are using multiple lifetime scopes to share services that all dependencies of those services need to be similarly regustered. For example:

```csharp
public class WebOnlyService(){ ... }
public class SharedService(WebOnlyService){ ... }

builder.RegisterType<SharedService>().InstancePerRequest();
builder.RegisterType<SharedService>().InstancePerMatchingLifetimeScope(requestTag, jobTag);
```

Attempting to resolve `SharedService` from a background job will throw an exception as Autofac will need to resolve `WebOnlyService` outside of a `RequestLifetimeScope`. 

Also be aware that many web related properties that you may be using such as `HttpContext.Current` **will be unavailable**.

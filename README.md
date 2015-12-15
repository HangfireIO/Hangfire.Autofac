Hangfire.Autofac [![Build status](https://ci.appveyor.com/api/projects/status/oncvxlqtnake9c86)](https://ci.appveyor.com/project/odinserj/hangfire-autofac)
================

[Autofac](http://autofac.org) integration for [Hangfire](http://hangfire.io). Provides an implementation of the `JobActivator` class and registration extensions, allowing you to use Autofac container to **resolve job type instances** as well as **control the lifetime** of the all related dependencies.

*Hangfire.Autofac* resolves service instances using a child, tagged [lifetime scope](http://docs.autofac.org/en/latest/lifetime/index.html). A child scope is created and disposed each time when background job processing takes place, so you have precise control of your service's lifetime, including **shared instances** and **deterministic disposal**.

Installation
--------------

*Hangfire.Autofac* is available as a NuGet Package. Type the following command into NuGet Package Manager Console window to install it:

```
Install-Package Hangfire.Autofac
```

Usage
------

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

Please refer to the Autofac documentation to learn more about [Automatic Disposal](http://docs.autofac.org/en/latest/lifetime/disposal.html#automatic-disposal) feature.

HTTP Request Warnings
----------------------

Services registered with `InstancePerHttpRequest()` method **will be unavailable** during job activation, you should re-register these services without this hint. If you have components registered for the HTTP request scope, please add an additional scope for them:

```csharp
builder.RegisterType<Database>()
    .InstancePerBackgroundJob()
    .InstancePerHttpRequest();
```

**`HttpContext.Current` is also not available during the job performance. Don't use it!**


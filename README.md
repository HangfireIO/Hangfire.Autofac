Hangfire.Autofac
================

[![Build status](https://ci.appveyor.com/api/projects/status/oncvxlqtnake9c86)](https://ci.appveyor.com/project/odinserj/hangfire-autofac)

[Hangfire](http://hangfire.io) background job activator based on 
[Autofac](http://autofac.org) IoC Container. It allows you to use instance
methods of classes that define parametrized constructors:

```csharp
public class EmailService
{
	private DbContext _context;
    private IEmailSender _sender;
	
	public EmailService(DbContext context, IEmailSender sender)
	{
		_context = context;
		_sender = sender;
	}
	
	public void Send(int userId, string message)
	{
		var user = _context.Users.Get(userId);
		_sender.Send(user.Email, message);
	}
}	

// Somewhere in the code
BackgroundJob.Enqueue<EmailService>(x => x.Send(1, "Hello, world!"));
```

Improve the testability of your jobs without static factories!

Installation
--------------

Hangfire.Autofac is available as a NuGet Package. Type the following
command into NuGet Package Manager Console window to install it:

```
Install-Package Hangfire.Autofac
```

Usage
------

The package provides an extension method for [OWIN bootstrapper](http://docs.hangfire.io/en/latest/users-guide/getting-started/owin-bootstrapper.html):

```csharp
app.UseHangfire(config =>
{
    var builder = new ContainerBuilder();
    config.UseAutofacActivator(builder.Build());
});
```

In order to use the library outside of web application, set the static JobActivator.Current property:

```csharp
var builder = new ContainerBuilder();
JobActivator.Current = new AutofacJobActivator(builder.Build());
```

HTTP Request warnings
-----------------------

Services registered with `InstancePerHttpRequest()` directive **will be unavailable**
during job activation, you should re-register these services without this
hint.

`HttpContext.Current` is also **not available** during the job performance. 
Don't use it!

HangFire.Autofac
================

[![Build status](https://ci.appveyor.com/api/projects/status/0xf9wv6crdy1tl6y)](https://ci.appveyor.com/project/odinserj/hangfire-autofac)

[HangFire](http://hangfire.io) background job activator based on 
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

HangFire.Autofac is available as a NuGet Package. Type the following
command into NuGet Package Manager Console window to install it:

```
Install-Package HangFire.Autofac
```

Usage
------

### Web application

Update your OWIN Startup class with the following lines:

```csharp
public class Startup
{
    public void Configure(IAppBuilder app)
    {
        app.UseHangFire(config =>
        {
            var builder = new ContainerBuilder();
            /* Register types */
            /* builder.RegisterType<MyDbContext>(); */
        
            var container = builder.Build();
            config.UseAutofacActivator(container);

            // Other configuration actions
        });
    }
}
```

### Other application types

Pass an instance of the `AutofacJobActivator` class to the global job activator somewhere in application initialization logic:

```csharp
var builder = new ContainerBuilder();
/* Register types */
/* builder.RegisterType<MyDbContext>(); */
        
var container = builder.Build();

JobActivator.Current = new AutofacJobActivator(container);
```

HTTP Request warnings
-----------------------

Services registered with `InstancePerHttpRequest()` directive **will be unavailable**
during job activation, you should re-register these services without this
hint.

`HttpContext.Current` is also **not available** during the job performance. 
Don't use it!

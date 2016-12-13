﻿using System;
using Autofac;
using Xunit;

namespace Hangfire.Autofac.Tests
{
	public class AutofacJobActivatorTests
	{
		private ContainerBuilder _builder;

		public AutofacJobActivatorTests()
		{
			_builder = new ContainerBuilder();
		}

		[Fact]
		public void Ctor_ThrowsAnException_WhenLifetimeScopeIsNull()
		{
			Assert.Throws<ArgumentNullException>(() => new AutofacJobActivator(null));
		}

		[Fact]
		public void Class_IsBasedOnJobActivator()
		{
			var activator = CreateActivator();

			Assert.IsType<AutofacJobActivator>(activator);
		}

		[Fact]
		public void ActivateJob_ResolvesAnInstance_UsingAutofac()
		{
			_builder.Register(c => "called").As<string>();
			var activator = CreateActivator();

			var result = activator.ActivateJob(typeof(string));

			Assert.Equal("called", result);
		}

		[Fact]
		public void InstanceRegisteredWith_InstancePerDependency_IsDisposedOnScopeDisposal()
		{
			var disposable = new Disposable();
			_builder.Register(c => disposable).InstancePerDependency();
			var activator = CreateActivator();

			using (var scope = activator.BeginScope())
			{
				var instance = scope.Resolve(typeof(Disposable));
				Assert.False(disposable.Disposed);
			}

			Assert.True(disposable.Disposed);
		}

		[Fact]
		public void InstanceRegisteredWith_SingleInstance_IsNotDisposedOnScopeDisposal()
		{
			var disposable = new Disposable();
			_builder.Register(c => disposable).SingleInstance();
			var activator = CreateActivator();

			using (var scope = activator.BeginScope())
			{
				var instance = scope.Resolve(typeof(Disposable));
				Assert.False(disposable.Disposed);
			}

			Assert.False(disposable.Disposed);
		}

		[Fact]
		public void InstancePerBackgroundJob_RegistersSameServiceInstance_ForTheSameScopeInstance()
		{
			_builder.Register(c => new object()).As<object>()
				.InstancePerBackgroundJob();
			var activator = CreateActivator();

			using (var scope = activator.BeginScope())
			{
				var instance1 = scope.Resolve(typeof(object));
				var instance2 = scope.Resolve(typeof(object));

				Assert.Same(instance1, instance2);
			}
		}

		[Fact]
		public void InstancePerBackgroundJob_RegistersDifferentServiceInstances_ForDifferentScopeInstances()
		{
			_builder.Register(c => new object()).As<object>().InstancePerBackgroundJob();
			var activator = CreateActivator();

			object instance1;
			using (var scope1 = activator.BeginScope())
			{
				instance1 = scope1.Resolve(typeof(object));
			}

			object instance2;
			using (var scope2 = activator.BeginScope())
			{
				instance2 = scope2.Resolve(typeof(object));
			}

			Assert.NotSame(instance1, instance2);
		}

		[Fact]
		public void InstanceRegisteredWith_InstancePerBackgroundJob_IsDisposedOnScopeDisposal()
		{
			var disposable = new Disposable();
			_builder.Register(c => disposable).InstancePerBackgroundJob();
			var activator = CreateActivator();

			using (var scope = activator.BeginScope())
			{
				var instance = scope.Resolve(typeof(Disposable));
			}

			Assert.True(disposable.Disposed);
		}

		[Fact]
		public void InstancePerJob_RegistersSameServiceInstance_ForTheSameScopeInstance()
		{
			_builder.Register(c => new object()).As<object>().InstancePerLifetimeScope();
			var activator = CreateActivator(false);

			using (var scope = activator.BeginScope())
			{
				var instance1 = scope.Resolve(typeof(object));
				var instance2 = scope.Resolve(typeof(object));

				Assert.Same(instance1, instance2);
			}
		}

		[Fact]
		public void InstancePerJob_RegistersDifferentServiceInstances_ForDifferentScopeInstances()
		{
			_builder.Register(c => new object()).As<object>();
			var activator = CreateActivator(false);

			object instance1;
			using (var scope1 = activator.BeginScope())
			{
				instance1 = scope1.Resolve(typeof(object));
			}

			object instance2;
			using (var scope2 = activator.BeginScope())
			{
				instance2 = scope2.Resolve(typeof(object));
			}

			Assert.NotSame(instance1, instance2);
		}

		[Fact]
		public void InstanceRegisteredWith_InstancePerJob_IsDisposedOnScopeDisposal()
		{
			var disposable = new Disposable();
			_builder.Register(c => disposable);
			var activator = CreateActivator();

			using (var scope = activator.BeginScope())
			{
				var instance = scope.Resolve(typeof(Disposable));
			}

			Assert.True(disposable.Disposed);
		}

		[Fact]
		public void InstancePerJob_RegisteredWithExtraTags_ResolvesForJobScope()
		{
			var alternateLifetimeScopeTag = new object();
			_builder.Register(c => new object()).As<object>()
			   .InstancePerBackgroundJob(alternateLifetimeScopeTag);
			var activator = CreateActivator();

			using (var scope = activator.BeginScope())
			{
				var instance = scope.Resolve(typeof(object));
			}
		}

		[Fact]
		public void InstancePerJob_RegisteredWithExtraTags_ResolvesForAlternateScope()
		{
			var alternateLifetimeScopeTag = new object();
			_builder.Register(c => new object()).As<object>()
			   .InstancePerBackgroundJob(alternateLifetimeScopeTag);
			var container = _builder.Build();

			using (var scope = container.BeginLifetimeScope(alternateLifetimeScopeTag))
			{
				var instance = scope.Resolve(typeof(object));
			}
		}

		//[Fact]
		//public void UseAutofacActivator_CallsUseActivatorCorrectly()
		//{
		//	var configuration = new Mock<IBootstrapperConfiguration>();
		//	var lifetimeScope = new Mock<ILifetimeScope>();

		//	configuration.Object.UseAutofacActivator(lifetimeScope.Object);

		//	configuration.Verify(x => x.UseActivator(It.IsAny<AutofacJobActivator>()));
		//}

		private AutofacJobActivator CreateActivator(bool useTaggedLifetimeScope = true)
		{
			return new AutofacJobActivator(_builder.Build(), useTaggedLifetimeScope);
		}

		class Disposable : IDisposable
		{
			public bool Disposed { get; set; }

			public void Dispose()
			{
				Disposed = true;
			}
		}
	}
}

using System;
using Autofac;
using Xunit;

namespace Hangfire.Autofac.Tests
{
    public class AutofacJobActivatorTests
    {
        private readonly ContainerBuilder _builder = new ContainerBuilder();

        [Fact]
        public void Ctor_ThrowsAnException_WhenLifetimeScopeIsNull()
        {
            var exception = Assert.Throws<ArgumentNullException>(() => new AutofacJobActivator(null));
            Assert.Equal("lifetimeScope", exception.ParamName);
        }

        [Fact]
        public void Class_IsBasedOnJobActivator()
        {
            var activator = CreateActivator();
            Assert.IsAssignableFrom<JobActivator>(activator);
        }

        [Fact]
        public void ActivateJob_ResolvesAnInstance_UsingAutofac()
        {
            _builder.Register(_ => "called").As<string>();
            var activator = CreateActivator();

            var result = activator.ActivateJob(typeof(string));

            Assert.Equal("called", result);
        }

#if !NET452
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ActivateJob_ResolvesServices_RegisteredInTheConfigurationAction(bool taggedScope)
        {
            var activator = CreateActivator((builder, _) =>
            {
                builder.Register(_ => "configuration_action").As<string>();
            }, taggedScope);

            using var scope = BeginScope(activator);

            var result = scope.Resolve(typeof(string));
            Assert.Equal("configuration_action", result);
        }
#endif

        [Fact]
        public void InstanceRegisteredWith_InstancePerDependency_IsDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(_ => disposable).InstancePerDependency();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof(Disposable));
                Assert.False(disposable.Disposed);
            }

            Assert.True(disposable.Disposed);
        }

        [Fact]
        public void InstanceRegisteredWith_SingleInstance_IsNotDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(_ => disposable).SingleInstance();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof (Disposable));
                Assert.False(disposable.Disposed);
            }

            Assert.False(disposable.Disposed);
        }

        [Fact]
        public void InstancePerBackgroundJob_RegistersSameServiceInstance_ForTheSameScopeInstance()
        {
            _builder.Register(_ => new object()).As<object>()
                .InstancePerBackgroundJob();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                var instance1 = scope.Resolve(typeof (object));
                var instance2 = scope.Resolve(typeof (object));

                Assert.Same(instance1, instance2);
            }
        }

        [Fact]
        public void InstancePerBackgroundJob_RegistersDifferentServiceInstances_ForDifferentScopeInstances()
        {
            _builder.Register(_ => new object()).As<object>().InstancePerBackgroundJob();
            var activator = CreateActivator();

            object instance1;
            using (var scope1 = BeginScope(activator))
            {
                instance1 = scope1.Resolve(typeof (object));
            }

            object instance2;
            using (var scope2 = BeginScope(activator))
            {
                instance2 = scope2.Resolve(typeof (object));
            }

            Assert.NotSame(instance1, instance2);
        }

        [Fact]
        public void InstanceRegisteredWith_InstancePerBackgroundJob_IsDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(_ => disposable).InstancePerBackgroundJob();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof (Disposable));
            }

            Assert.True(disposable.Disposed);
        }

        [Fact]
        public void InstancePerJob_RegistersSameServiceInstance_ForTheSameScopeInstance()
        {
            _builder.Register(_ => new object()).As<object>().InstancePerLifetimeScope();
            var activator = CreateActivator(useTaggedLifetimeScope: false);

            using (var scope = BeginScope(activator))
            {
                var instance1 = scope.Resolve(typeof(object));
                var instance2 = scope.Resolve(typeof(object));

                Assert.Same(instance1, instance2);
            }
        }

        [Fact]
        public void InstancePerJob_RegistersDifferentServiceInstances_ForDifferentScopeInstances()
        {
            _builder.Register(_ => new object()).As<object>();
            var activator = CreateActivator(useTaggedLifetimeScope: false);

            object instance1;
            using (var scope1 = BeginScope(activator))
            {
                instance1 = scope1.Resolve(typeof(object));
            }

            object instance2;
            using (var scope2 = BeginScope(activator))
            {
                instance2 = scope2.Resolve(typeof(object));
            }

            Assert.NotSame(instance1, instance2);
        }

        [Fact]
        public void InstanceRegisteredWith_InstancePerJob_IsDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(_ => disposable);
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof(Disposable));
            }

            Assert.True(disposable.Disposed);
        }

        [Fact]
        public void InstancePerJob_RegisteredWithExtraTags_ResolvesForJobScope()
        {
            var alternateLifetimeScopeTag = new object();
            _builder.Register(_ => new object()).As<object>()
               .InstancePerBackgroundJob(alternateLifetimeScopeTag);
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof(object));
            }
        }

        [Fact]
        public void InstancePerJob_RegisteredWithExtraTags_ResolvesForAlternateScope()
        {
            var alternateLifetimeScopeTag = new object();
            _builder.Register(_ => new object()).As<object>()
               .InstancePerBackgroundJob(alternateLifetimeScopeTag);
            var container = _builder.Build();

            using (var scope = container.BeginLifetimeScope(alternateLifetimeScopeTag))
            {
                // ReSharper disable once UnusedVariable
                var  instance = scope.Resolve(typeof(object));
            }
        }

        private AutofacJobActivator CreateActivator(
#if !NET452
            Action<ContainerBuilder, JobActivatorContext> configure = null,
#endif
            bool useTaggedLifetimeScope = true)
        {
            return new AutofacJobActivator(
                _builder.Build(),
#if !NET452
                configure,
#endif
                useTaggedLifetimeScope);
        }

        class Disposable : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private static JobActivatorScope BeginScope(JobActivator activator)
        {
#if NET452
            return activator.BeginScope();
#else
            return activator.BeginScope(null);
#endif
        }
    }
}

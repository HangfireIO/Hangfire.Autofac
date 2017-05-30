using System;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hangfire.Autofac.Tests
{
    [TestClass]
    public class AutofacJobActivatorTests
    {
        private ContainerBuilder _builder;

        [TestInitialize]
        public void SetUp()
        {
            _builder = new ContainerBuilder();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Ctor_ThrowsAnException_WhenLifetimeScopeIsNull()
        {
// ReSharper disable once UnusedVariable
            var activator = new AutofacJobActivator(null);
        }

        [TestMethod]
        public void Class_IsBasedOnJobActivator()
        {
            var activator = CreateActivator();
            Assert.IsInstanceOfType(activator, typeof(JobActivator));
        }

        [TestMethod]
        public void ActivateJob_ResolvesAnInstance_UsingAutofac()
        {
            _builder.Register(c => "called").As<string>();
            var activator = CreateActivator();

            var result = activator.ActivateJob(typeof(string));

            Assert.AreEqual("called", result);
        }

        [TestMethod]
        public void InstanceRegisteredWith_InstancePerDependency_IsDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(c => disposable).InstancePerDependency();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof(Disposable));
                Assert.IsFalse(disposable.Disposed);
            }

            Assert.IsTrue(disposable.Disposed);
        }

        [TestMethod]
        public void InstanceRegisteredWith_SingleInstance_IsNotDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(c => disposable).SingleInstance();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof (Disposable));
                Assert.IsFalse(disposable.Disposed);
            }

            Assert.IsFalse(disposable.Disposed);
        }

        [TestMethod]
        public void InstancePerBackgroundJob_RegistersSameServiceInstance_ForTheSameScopeInstance()
        {
            _builder.Register(c => new object()).As<object>()
                .InstancePerBackgroundJob();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                var instance1 = scope.Resolve(typeof (object));
                var instance2 = scope.Resolve(typeof (object));

                Assert.AreSame(instance1, instance2);
            }
        }

        [TestMethod]
        public void InstancePerBackgroundJob_RegistersDifferentServiceInstances_ForDifferentScopeInstances()
        {
            _builder.Register(c => new object()).As<object>().InstancePerBackgroundJob();
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

            Assert.AreNotSame(instance1, instance2);
        }

        [TestMethod]
        public void InstanceRegisteredWith_InstancePerBackgroundJob_IsDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(c => disposable).InstancePerBackgroundJob();
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof (Disposable));
            }

            Assert.IsTrue(disposable.Disposed);
        }

        [TestMethod]
        public void InstancePerJob_RegistersSameServiceInstance_ForTheSameScopeInstance()
        {
            _builder.Register(c => new object()).As<object>().InstancePerLifetimeScope();
            var activator = CreateActivator(false);

            using (var scope = BeginScope(activator))
            {
                var instance1 = scope.Resolve(typeof(object));
                var instance2 = scope.Resolve(typeof(object));

                Assert.AreSame(instance1, instance2);
            }
        }

        [TestMethod]
        public void InstancePerJob_RegistersDifferentServiceInstances_ForDifferentScopeInstances()
        {
            _builder.Register(c => new object()).As<object>();
            var activator = CreateActivator(false);

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

            Assert.AreNotSame(instance1, instance2);
        }

        [TestMethod]
        public void InstanceRegisteredWith_InstancePerJob_IsDisposedOnScopeDisposal()
        {
            var disposable = new Disposable();
            _builder.Register(c => disposable);
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof(Disposable));
            }

            Assert.IsTrue(disposable.Disposed);
        }

        [TestMethod]
        public void InstancePerJob_RegisteredWithExtraTags_ResolvesForJobScope()
        {
            var alternateLifetimeScopeTag = new object();
            _builder.Register(c => new object()).As<object>()
               .InstancePerBackgroundJob(alternateLifetimeScopeTag);
            var activator = CreateActivator();

            using (var scope = BeginScope(activator))
            {
                // ReSharper disable once UnusedVariable
                var instance = scope.Resolve(typeof(object));
            }
        }

        [TestMethod]
        public void InstancePerJob_RegisteredWithExtraTags_ResolvesForAlternateScope()
        {
            var alternateLifetimeScopeTag = new object();
            _builder.Register(c => new object()).As<object>()
               .InstancePerBackgroundJob(alternateLifetimeScopeTag);
            var container = _builder.Build();

            using (var scope = container.BeginLifetimeScope(alternateLifetimeScopeTag))
            {
                // ReSharper disable once UnusedVariable
                var  instance = scope.Resolve(typeof(object));
            }
        }

#if NET45
        [TestMethod]
        public void UseAutofacActivator_CallsUseActivatorCorrectly()
        {
#pragma warning disable 618
            var configuration = new Moq.Mock<IBootstrapperConfiguration>();
            var lifetimeScope = new Moq.Mock<ILifetimeScope>();

            configuration.Object.UseAutofacActivator(lifetimeScope.Object);

            configuration.Verify(x => x.UseActivator(Moq.It.IsAny<AutofacJobActivator>()));
#pragma warning restore 618
        }
#endif

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

        private static JobActivatorScope BeginScope(JobActivator activator)
        {
#if NET45
            return activator.BeginScope();
#else
            return activator.BeginScope(null);
#endif
        }
    }
}

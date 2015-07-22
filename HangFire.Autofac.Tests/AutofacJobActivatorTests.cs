using System;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

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

            using (var scope = activator.BeginScope())
            {
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

            using (var scope = activator.BeginScope())
            {
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

            using (var scope = activator.BeginScope())
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
            using (var scope1 = activator.BeginScope())
            {
                instance1 = scope1.Resolve(typeof (object));
            }

            object instance2;
            using (var scope2 = activator.BeginScope())
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

            using (var scope = activator.BeginScope())
            {
                var instance = scope.Resolve(typeof (Disposable));
            }

            Assert.IsTrue(disposable.Disposed);
        }

        [TestMethod]
        public void UseAutofacActivator_CallsUseActivatorCorrectly()
        {
            var configuration = new Mock<IBootstrapperConfiguration>();
            var lifetimeScope = new Mock<ILifetimeScope>();

            configuration.Object.UseAutofacActivator(lifetimeScope.Object);

            configuration.Verify(x => x.UseActivator(It.IsAny<AutofacJobActivator>()));
        }

        private AutofacJobActivator CreateActivator()
        {
            return new AutofacJobActivator(_builder.Build());
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

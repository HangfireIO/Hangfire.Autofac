using System;
using Autofac;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HangFire.Autofac.Tests
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
            var activator = new AutofacJobActivator(_builder.Build());
            Assert.IsInstanceOfType(activator, typeof(JobActivator));
        }

        [TestMethod]
        public void ActivateJob_CallsAutofac()
        {
            _builder.Register(c => "called").As<string>();
            var activator = new AutofacJobActivator(_builder.Build());

            var result = activator.ActivateJob(typeof(string));

            Assert.AreEqual("called", result);
        }
    }
}

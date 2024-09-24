using System;
using Autofac;
using Xunit;

namespace Hangfire.Autofac.Tests
{
    public class GlobalConfigurationExtensionsFacts
    {
        [Fact]
        public void UseAutofacActivator_ThrowsAnException_WhenConfigurationIsNull()
        {
            var lifetimeScope = new Moq.Mock<ILifetimeScope>();

            var exception = Assert.Throws<ArgumentNullException>(
                () => ((IGlobalConfiguration)null).UseAutofacActivator(lifetimeScope.Object));

            Assert.Equal("configuration", exception.ParamName);
        }

        [Fact]
        public void UseAutofacActivator_ThrowsAnException_WhenLifetimeScopeIsNull()
        {
            var configuration = new Moq.Mock<IGlobalConfiguration>();

            var exception = Assert.Throws<ArgumentNullException>(
                () => configuration.Object.UseAutofacActivator(null));

            Assert.Equal("lifetimeScope", exception.ParamName);
        }

        [Fact]
        public void UseAutofacActivator_CallsUseActivatorCorrectly()
        {
            var configuration = new Moq.Mock<IGlobalConfiguration>();
            var lifetimeScope = new Moq.Mock<ILifetimeScope>();

            configuration.Object.UseAutofacActivator(lifetimeScope.Object);

            Assert.IsType<AutofacJobActivator>(JobActivator.Current);
        }

#if !NET452
        [Fact]
        public void UseAutofacActivator_WithConfigurationAction_ThrowsAnException_WhenConfigurationIsNull()
        {
            var lifetimeScope = new Moq.Mock<ILifetimeScope>();

            var exception = Assert.Throws<ArgumentNullException>(
                () => ((IGlobalConfiguration)null).UseAutofacActivator(lifetimeScope.Object, (_, _) => {  }));

            Assert.Equal("configuration", exception.ParamName);
        }

        [Fact]
        public void UseAutofacActivator_WithConfigurationAction_ThrowsAnException_WhenLifetimeScopeIsNull()
        {
            var configuration = new Moq.Mock<IGlobalConfiguration>();

            var exception = Assert.Throws<ArgumentNullException>(
                () => configuration.Object.UseAutofacActivator(null, (_, _) => {  }));

            Assert.Equal("lifetimeScope", exception.ParamName);
        }

        [Fact]
        public void UseAutofacActivator_WithConfigurationAction_CallsUseActivatorCorrectly()
        {
            var configuration = new Moq.Mock<IGlobalConfiguration>();
            var lifetimeScope = new Moq.Mock<ILifetimeScope>();

            configuration.Object.UseAutofacActivator(lifetimeScope.Object, (_, _) => {  });

            Assert.IsType<AutofacJobActivator>(JobActivator.Current);
        }
#endif
    }
}
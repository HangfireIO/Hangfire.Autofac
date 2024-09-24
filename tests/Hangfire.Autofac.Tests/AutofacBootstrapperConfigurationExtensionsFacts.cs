#if NET452
using Autofac;
using Xunit;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Hangfire.Autofac.Tests
{
    public class AutofacBootstrapperConfigurationExtensionsFacts
    {
        [Fact]
        public void UseAutofacActivator_CallsUseActivatorCorrectly()
        {
            var configuration = new Moq.Mock<IBootstrapperConfiguration>();
            var lifetimeScope = new Moq.Mock<ILifetimeScope>();

            configuration.Object.UseAutofacActivator(lifetimeScope.Object);

            configuration.Verify(x => x.UseActivator(Moq.It.IsAny<AutofacJobActivator>()));
        }
    }
}

#endif
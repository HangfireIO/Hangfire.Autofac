using Autofac;

namespace Hangfire
{
    public static class AutofacBootstrapperConfigurationExtensions
    {
        /// <summary>
        /// Tells bootstrapper to use the specified Autofac
        /// lifetime scope as a global job activator.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        /// <param name="lifetimeScope">Autofac lifetime scope that will be used to activate jobs</param>
        public static void UseAutofacActivator(
            this IBootstrapperConfiguration configuration,
            ILifetimeScope lifetimeScope)
        {
            configuration.UseActivator(new AutofacJobActivator(lifetimeScope));
        }
    }
}

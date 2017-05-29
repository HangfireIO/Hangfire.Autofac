#if NET45
using System;
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
        /// <param name="useTaggedLifetimeScope">should tagged lifetimeScopes be used</param>
        [Obsolete("Please use `GlobalConfiguration.Configuration.UseAutofacActivator` method instead. Will be removed in version 2.0.0.")]
        public static void UseAutofacActivator(
            this IBootstrapperConfiguration configuration,
            ILifetimeScope lifetimeScope, bool useTaggedLifetimeScope = true)
        {
            configuration.UseActivator(new AutofacJobActivator(lifetimeScope, useTaggedLifetimeScope));
        }
    }
}
#endif
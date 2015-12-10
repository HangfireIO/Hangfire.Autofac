using System;
using Autofac;
using Hangfire.Annotations;

namespace Hangfire
{
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration<AutofacJobActivator> UseAutofacActivator(
            [NotNull] this IGlobalConfiguration configuration, 
            [NotNull] ILifetimeScope lifetimeScope, bool useTaggedLifetimeScope = true)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");

            return configuration.UseActivator(new AutofacJobActivator(lifetimeScope, useTaggedLifetimeScope));
        }
    }
}

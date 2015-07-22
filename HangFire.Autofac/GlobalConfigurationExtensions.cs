using System;
using Autofac;
using Hangfire.Annotations;

namespace Hangfire
{
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration<AutofacJobActivator> UseAutofacActivator(
            [NotNull] this IGlobalConfiguration configuration, 
            [NotNull] ILifetimeScope lifetimeScope)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (lifetimeScope == null) throw new ArgumentNullException(nameof(lifetimeScope));

            return configuration.UseActivator(new AutofacJobActivator(lifetimeScope));
        }
    }
}

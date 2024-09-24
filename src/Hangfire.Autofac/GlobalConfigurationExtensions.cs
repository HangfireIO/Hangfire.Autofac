using System;
using Autofac;
using Hangfire.Annotations;

// ReSharper disable once CheckNamespace
namespace Hangfire
{
    public static class GlobalConfigurationExtensions
    {
        public static IGlobalConfiguration<AutofacJobActivator> UseAutofacActivator(
            [NotNull] this IGlobalConfiguration configuration, 
            [NotNull] ILifetimeScope lifetimeScope, bool useTaggedLifetimeScope = true)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (lifetimeScope == null) throw new ArgumentNullException(nameof(lifetimeScope));

            return configuration.UseActivator(new AutofacJobActivator(lifetimeScope, useTaggedLifetimeScope));
        }

#if !NET45
        public static IGlobalConfiguration<AutofacJobActivator> UseAutofacActivator(
            [NotNull] this IGlobalConfiguration configuration, 
            [NotNull] ILifetimeScope lifetimeScope,
            [CanBeNull] Action<ContainerBuilder, JobActivatorContext> scopeConfigurationAction,
            bool useTaggedLifetimeScope = true)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));
            if (lifetimeScope == null) throw new ArgumentNullException(nameof(lifetimeScope));

            return configuration.UseActivator(new AutofacJobActivator(lifetimeScope, scopeConfigurationAction, useTaggedLifetimeScope));
        }
#endif
    }
}

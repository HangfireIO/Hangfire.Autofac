using System;
using Autofac;
using Hangfire.Annotations;

namespace Hangfire
{
    /// <summary>
    /// Hangfire Job Activator based on Autofac IoC Container.
    /// </summary>
    public class AutofacJobActivator : JobActivator
    {
        /// <summary>
        /// Tag used in setting up per-job lifetime scope registrations.
        /// </summary>
        public static readonly object LifetimeScopeTag = (object) "HangfireJob";

        private readonly ILifetimeScope _lifetimeScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacJobActivator"/>
        /// class with the given Autofac Lifetime Scope.
        /// </summary>
        /// <param name="lifetimeScope">Container that will be used to create instance
        /// of classes during job activation process.</param>
        public AutofacJobActivator([NotNull] ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");
            _lifetimeScope = lifetimeScope;
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return _lifetimeScope.Resolve(jobType);
        }

        public override JobActivatorScope BeginScope()
        {
            return new AutofacScope(_lifetimeScope.BeginLifetimeScope(LifetimeScopeTag));
        }

        class AutofacScope : JobActivatorScope
        {
            private readonly ILifetimeScope _lifetimeScope;

            public AutofacScope(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
            }

            public override object Resolve(Type type)
            {
                return _lifetimeScope.Resolve(type);
            }

            public override void DisposeScope()
            {
                _lifetimeScope.Dispose();
            }
        }
    }
}

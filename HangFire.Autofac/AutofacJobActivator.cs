using System;
using Autofac;

namespace Hangfire
{
    /// <summary>
    /// HangFire Job Activator based on Autofac IoC Container.
    /// </summary>
    public class AutofacJobActivator : JobActivator
    {
        private readonly ILifetimeScope _lifetimeScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacJobActivator"/>
        /// class with the given Autofac Lifetime Scope.
        /// </summary>
        /// <param name="lifetimeScope">Container that will be used to create instance
        /// of classes during job activation process.</param>
        public AutofacJobActivator(ILifetimeScope lifetimeScope)
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");

            _lifetimeScope = lifetimeScope;
        }

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return _lifetimeScope.Resolve(jobType);
        }
    }
}

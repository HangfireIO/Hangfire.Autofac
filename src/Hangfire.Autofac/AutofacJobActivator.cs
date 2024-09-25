﻿using System;
using Autofac;
using Hangfire.Annotations;

// ReSharper disable once CheckNamespace
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
        public static readonly object LifetimeScopeTag = "BackgroundJobScope";

        private readonly ILifetimeScope _lifetimeScope;
#if !NET45
        private readonly Action<ContainerBuilder, JobActivatorContext> _scopeConfigurationAction;
#endif
        private readonly bool _useTaggedLifetimeScope;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacJobActivator"/>
        /// class with the given Autofac Lifetime Scope.
        /// </summary>
        /// <param name="lifetimeScope">Container that will be used to create instance
        /// of classes during job activation process.</param>
        /// <param name="useTaggedLifetimeScope">Should the Container use Tag 
        /// BackgroundJobScope to resolve dependencies or create new Scope on each job</param>
        public AutofacJobActivator([NotNull] ILifetimeScope lifetimeScope, bool useTaggedLifetimeScope = true)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            _useTaggedLifetimeScope = useTaggedLifetimeScope;
        }

#if !NET45
        public AutofacJobActivator(
            [NotNull] ILifetimeScope lifetimeScope,
            [CanBeNull] Action<ContainerBuilder, JobActivatorContext> scopeConfigurationAction,
            bool useTaggedLifetimeScope = true)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            _scopeConfigurationAction = scopeConfigurationAction;
            _useTaggedLifetimeScope = useTaggedLifetimeScope;
        }
#endif

        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return _lifetimeScope.Resolve(jobType);
        }

#if NET45
        public override JobActivatorScope BeginScope()
        {
            return new AutofacScope(_useTaggedLifetimeScope
                ? _lifetimeScope.BeginLifetimeScope(LifetimeScopeTag)
                : _lifetimeScope.BeginLifetimeScope());
        }
#else
        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            if (_scopeConfigurationAction != null)
            {
                Action<ContainerBuilder> configurationAction = builder =>
                {
                    _scopeConfigurationAction(builder, context);
                };

                return new AutofacScope(_useTaggedLifetimeScope
                    ? _lifetimeScope.BeginLifetimeScope(LifetimeScopeTag, configurationAction)
                    : _lifetimeScope.BeginLifetimeScope(configurationAction));
            }

            return new AutofacScope(_useTaggedLifetimeScope
                ? _lifetimeScope.BeginLifetimeScope(LifetimeScopeTag)
                : _lifetimeScope.BeginLifetimeScope());
        }
#endif

        private sealed class AutofacScope : JobActivatorScope
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

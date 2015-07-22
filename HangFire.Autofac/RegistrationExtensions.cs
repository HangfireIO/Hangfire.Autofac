using System;
using Autofac.Builder;
using Hangfire.Annotations;

namespace Hangfire
{
    /// <summary>
    /// Adds registration syntax to the <see cref="Autofac.ContainerBuilder"/> type.
    /// </summary>
    public static class RegistrationExtensions
    {
        /// <summary>
        /// Share one instance of the component within the context of a single
        /// processing background job instance.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="registration"/> is <see langword="null"/>.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerBackgroundJob<TLimit, TActivatorData, TStyle>(
            [NotNull] this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            if (registration == null) throw new ArgumentNullException(nameof(registration));
            return registration.InstancePerMatchingLifetimeScope(AutofacJobActivator.LifetimeScopeTag);
        }
    }
}
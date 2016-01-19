using System;
using System.Linq;
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
            return registration.InstancePerBackgroundJob(new object[] { });
        }

        /// <summary>
        /// Share one instance of the component within the context of a single
        /// processing background job instance.
        /// </summary>
        /// <typeparam name="TLimit">Registration limit type.</typeparam>
        /// <typeparam name="TActivatorData">Activator data type.</typeparam>
        /// <typeparam name="TStyle">Registration style.</typeparam>
        /// <param name="registration">The registration to configure.</param>
        /// <param name="lifetimeScopeTags">Additional tags applied for matching lifetime scopes.</param>
        /// <returns>A registration builder allowing further configuration of the component.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="registration"/> is <see langword="null"/>.
        /// </exception>
        public static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerBackgroundJob<TLimit, TActivatorData, TStyle>(
            [NotNull] this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration,
            params object[] lifetimeScopeTags)
        {
            if (registration == null) throw new ArgumentNullException("registration");

            var tags = new[] { AutofacJobActivator.LifetimeScopeTag }.Concat(lifetimeScopeTags).ToArray();
            return registration.InstancePerMatchingLifetimeScope(tags);
        }
    }
}
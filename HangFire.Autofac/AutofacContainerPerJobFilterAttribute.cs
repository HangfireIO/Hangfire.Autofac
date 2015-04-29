using Hangfire.Common;
using Hangfire.Server;

namespace Hangfire
{
    public class AutofacContainerPerJobFilterAttribute : JobFilterAttribute, IServerFilter
    {
        private readonly AutofacPerLifetimeScopeJobActivator _autofacPerLifetimeScopeJobActivator;

        public AutofacContainerPerJobFilterAttribute(AutofacPerLifetimeScopeJobActivator autofacPerLifetimeScopeJobActivator)
        {
            _autofacPerLifetimeScopeJobActivator = autofacPerLifetimeScopeJobActivator;
        }

        public void OnPerformed(PerformedContext filterContext)
        {
            _autofacPerLifetimeScopeJobActivator.DisposeJobLifetimeScope();
        }

        public void OnPerforming(PerformingContext filterContext)
        {
            _autofacPerLifetimeScopeJobActivator.CreateJobLifetimeScope();
        }
    }
}

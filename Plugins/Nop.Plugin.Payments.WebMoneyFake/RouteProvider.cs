using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.WebMoneyFake
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.WebMoneyFake.Configure",
                 "Plugins/PaymentWebMoneyFake/Configure",
                 new { controller = "PaymentWebMoneyFake", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.WebMoneyFake.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.WebMoneyFake.PaymentInfo",
                 "Plugins/PaymentWebMoneyFake/PaymentInfo",
                 new { controller = "PaymentWebMoneyFake", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.WebMoneyFake.Controllers" }
            );
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }
}

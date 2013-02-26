using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.LiqPayFake
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.LiqPayFake.Configure",
                 "Plugins/PaymentLiqPayFake/Configure",
                 new { controller = "PaymentLiqPayFake", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.LiqPayFake.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.LiqPayFake.PaymentInfo",
                 "Plugins/PaymentLiqPayFake/PaymentInfo",
                 new { controller = "PaymentLiqPayFake", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.LiqPayFake.Controllers" }
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

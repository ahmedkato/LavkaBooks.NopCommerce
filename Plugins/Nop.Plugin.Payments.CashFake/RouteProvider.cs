using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.CashFake
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.CashFake.Configure",
                 "Plugins/PaymentCashFake/Configure",
                 new { controller = "PaymentCashFake", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.CashFake.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.CashFake.PaymentInfo",
                 "Plugins/PaymentCashFake/PaymentInfo",
                 new { controller = "PaymentCashFake", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.CashFake.Controllers" }
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

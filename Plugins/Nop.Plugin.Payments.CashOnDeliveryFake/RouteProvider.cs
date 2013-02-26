using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.CashOnDeliveryFake
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.CashOnDeliveryFake.Configure",
                 "Plugins/PaymentCashOnDeliveryFake/Configure",
                 new { controller = "PaymentCashOnDeliveryFake", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.CashOnDeliveryFake.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.CashOnDeliveryFake.PaymentInfo",
                 "Plugins/PaymentCashOnDeliveryFake/PaymentInfo",
                 new { controller = "PaymentCashOnDeliveryFake", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.CashOnDeliveryFake.Controllers" }
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

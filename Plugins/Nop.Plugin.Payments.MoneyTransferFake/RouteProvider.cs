using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.MoneyTransferFake
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.MoneyTransferFake.Configure",
                 "Plugins/PaymentMoneyTransferFake/Configure",
                 new { controller = "PaymentMoneyTransferFake", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.MoneyTransferFake.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.MoneyTransferFake.PaymentInfo",
                 "Plugins/PaymentMoneyTransferFake/PaymentInfo",
                 new { controller = "PaymentMoneyTransferFake", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.MoneyTransferFake.Controllers" }
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

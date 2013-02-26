using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.PrivatBankFake
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.PrivatBankFake.Configure",
                 "Plugins/PaymentPrivatBankFake/Configure",
                 new { controller = "PaymentPrivatBankFake", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.PrivatBankFake.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.PrivatBankFake.PaymentInfo",
                 "Plugins/PaymentPrivatBankFake/PaymentInfo",
                 new { controller = "PaymentPrivatBankFake", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.PrivatBankFake.Controllers" }
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

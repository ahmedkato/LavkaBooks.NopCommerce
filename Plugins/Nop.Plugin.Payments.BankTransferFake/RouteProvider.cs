using System.Web.Mvc;
using System.Web.Routing;
using Nop.Web.Framework.Mvc.Routes;

namespace Nop.Plugin.Payments.BankTransferFake
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Plugin.Payments.BankTransferFake.Configure",
                 "Plugins/PaymentBankTransferFake/Configure",
                 new { controller = "PaymentBankTransferFake", action = "Configure" },
                 new[] { "Nop.Plugin.Payments.BankTransferFake.Controllers" }
            );

            routes.MapRoute("Plugin.Payments.BankTransferFake.PaymentInfo",
                 "Plugins/PaymentBankTransferFake/PaymentInfo",
                 new { controller = "PaymentBankTransferFake", action = "PaymentInfo" },
                 new[] { "Nop.Plugin.Payments.BankTransferFake.Controllers" }
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

using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Services.Configuration;
using Nop.Services.Payments;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Payments.LiqPayFake.Controllers
{
    public class PaymentLiqPayFakeController : BaseNopPaymentController
    {        
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {            
            return View("Nop.Plugin.Payments.LiqPayFake.Views.PaymentLiqPayFake.Configure", null);
        }
		
        [ChildActionOnly]
        public ActionResult PaymentInfo()
        {
            return View("Nop.Plugin.Payments.LiqPayFake.Views.PaymentLiqPayFake.PaymentInfo", null);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }
    }
}
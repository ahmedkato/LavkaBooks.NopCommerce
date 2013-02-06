using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Nop.Core.Domain.Catalog;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Common;
using Nop.Web.Models.Media;
using Nop.Web.Models.Checkout;

namespace Nop.Web.Models.ShoppingCart
{
    public partial class LavkaShoppingCartModel : ShoppingCartModel
    {
		public LavkaShoppingCartModel()
		{
			ShippingMethods = new CheckoutShippingMethodModel();
			PaymentMethods = new CheckoutPaymentMethodModel();
			ShippingAddress = new CheckoutShippingAddressModel();
		}

        public string SubTotal { get; set; }

        public CheckoutShippingMethodModel ShippingMethods { get; set; }

		public string ShippingMethod { get; set; }

        public CheckoutPaymentMethodModel PaymentMethods { get; set; }

		public string PaymentMethod { get; set; }

        public CheckoutShippingAddressModel ShippingAddress { get; set; }

		public int? ShippingAddressId { get; set; }
    }
}
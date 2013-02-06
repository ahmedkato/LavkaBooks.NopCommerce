using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Media;

namespace Nop.Web.Models.Customer
{
    public partial class CustomerWishlistModel : Nop.Web.Models.ShoppingCart.WishlistModel
    {
        public CustomerNavigationModel NavigationModel { get; set; }
    }
}
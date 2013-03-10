using Nop.Admin.Models.Customers;
using Nop.Admin.Validators.Catalog;
using Nop.Web.Framework;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentValidation.Attributes;

namespace Nop.Admin.Models.Catalog
{
	[Validator(typeof(BookValidator))]
    public class BookModel : BaseNopEntityModel, ILocalizedModel<ProductLocalizedModel>
    {
        public BookModel()
        {
            Locales = new List<ProductLocalizedModel>();
            Spec = new Dictionary<string, string>();
            AvailableCategories = new List<SelectListItem>();
            CategoryIds = new int[0];
        }
        
        [AllowHtml]
        public string Name { get; set; }

        [AllowHtml]
        public string SeName { get; set; }

        [AllowHtml]
        public string FullDescription { get; set; }

        public string PictureUrl { get; set; }

        public Dictionary<string, string> Spec { get; set; }

        public IList<ProductLocalizedModel> Locales { get; set; }

        public int[] CategoryIds { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public decimal Price { get; set; }
    }
}

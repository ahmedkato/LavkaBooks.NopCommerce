using Nop.Web.Framework.Mvc;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Free
{
	public class FreeProductModel : BaseNopEntityModel
	{
		public FreeProductModel()
		{
			FreeSubsribers = new List<CustomerInfoModel>();
		}

		public string Title { get; set; }
		public string Description { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public int Amount { get; set; }
		public ProductOverviewModel Product { get; set; }
		public IList<CustomerInfoModel> FreeSubsribers;
	}
}
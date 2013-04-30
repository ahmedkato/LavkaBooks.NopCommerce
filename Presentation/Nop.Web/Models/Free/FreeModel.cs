using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nop.Web.Models.Free
{
	public class FreeModel : BaseNopModel
	{
		public FreeModel()
		{
			FreeProducts = new List<FreeProductModel>();
		}

		public IList<FreeProductModel> FreeProducts { get; set; }
	}
}
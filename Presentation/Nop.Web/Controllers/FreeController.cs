using Nop.Services.Security;
using Nop.Web.Framework.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;

namespace Nop.Web.Controllers
{
	public class FreeController : BaseNopController
	{
		#region Fields

		private readonly IPermissionService _permissionService;

		#endregion

		public FreeController()
		{
		}

		[NopHttpsRequirement(SslRequirement.Yes)]
		public ActionResult Freelist(Guid? customerGuid)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ViewFreelist))
				return RedirectToRoute("HomePage");

			return View();
		}
	}
}

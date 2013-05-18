using Nop.Web.Framework.Mvc;
using System;
using System.Collections.Generic;

namespace Nop.Web.Models.Blogs
{
	public class HomePageBlogItemsModel : BaseNopModel, ICloneable
	{
		public HomePageBlogItemsModel()
		{
			BlogPosts = new List<BlogPostModel>();
		}

		public int WorkingLanguageId { get; set; }
		public IList<BlogPostModel> BlogPosts { get; set; }

		public object Clone()
		{
			return MemberwiseClone();
		}
	}
}

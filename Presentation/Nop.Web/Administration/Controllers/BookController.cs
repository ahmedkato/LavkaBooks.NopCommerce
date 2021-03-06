﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Nop.Admin.Models.Catalog;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Tax;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Telerik.Web.Mvc;
using Nop.Services.Seo;
using Nop.Services.Customers;
using Nop.Core.Domain.Customers;
using System.Net;
using OfficeOpenXml;

namespace Nop.Admin.Controllers
{
	[AdminAuthorize]
	public class BookController : BaseNopController
	{
		private readonly IProductService _productService;
		private readonly IPermissionService _permissionService;
		private readonly ILanguageService _languageService;
		private readonly ISpecificationAttributeService _specificationAttributeService;
		private readonly IPictureService _pictureService;
		private readonly ILocalizationService _localizationService;
		private readonly ICategoryService _categoryService;
		private readonly IProductTemplateService _productTemplateService;
		private readonly IUrlRecordService _urlRecordService;
		protected enum BookFields
		{
			Name,
			ISBN,
			Author,
			Year,
			Descripton,
			Price,
			Language,
			Publisher,
			Series,
			Pages,
			Format,
			Weight,
			Cover
		}
		private string[] properties;

		public BookController(IProductService productService,
			ILanguageService languageService,
			IPermissionService permissionService,
			ISpecificationAttributeService specificationAttributeService,
			IPictureService pictureService,
			ICategoryService categoryService,
			ILocalizationService localizationService,
			IProductTemplateService productTemplateService,
			IUrlRecordService urlRecordService)
		{
			this._productService = productService;
			this._languageService = languageService;
			this._permissionService = permissionService;
			this._specificationAttributeService = specificationAttributeService;
			this._pictureService = pictureService;
			this._categoryService = categoryService;
			this._localizationService = localizationService;
			this._productTemplateService = productTemplateService;
			this._urlRecordService = urlRecordService;
			properties = new string[]
                {
                    BookFields.Name.ToString(),
					BookFields.ISBN.ToString(),
                    BookFields.Author.ToString(),
                    BookFields.Year.ToString(),
					BookFields.Descripton.ToString(),
					BookFields.Price.ToString(),
                    BookFields.Language.ToString(),
                    BookFields.Publisher.ToString(),
                    BookFields.Series.ToString(),
                    BookFields.Pages.ToString(),
                    BookFields.Format.ToString(),
                    BookFields.Weight.ToString(),
                    BookFields.Cover.ToString()
                };
		}

		public ActionResult CreateBook()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var model = new BookModel();
			AddAvailableSpecificationAttribute(model);
			FillAvailableCategories(model);
			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		public ActionResult CreateBook(BookModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			GetSpecFromContext(model);
			var isbnExist = IsISBNExist(model);

			if (ModelState.IsValid && !isbnExist)
			{
				var product = new Product
					{
						Name = model.Name,
						FullDescription = model.FullDescription,
						CreatedOnUtc = DateTime.UtcNow,
						UpdatedOnUtc = DateTime.UtcNow,
						ProductTemplateId = GetProductTemplate().Id,
						ShowOnHomePage = true,
						AllowCustomerReviews = true,
						ApprovedRatingSum = 0,
						NotApprovedRatingSum = 0,
						ApprovedTotalReviews = 0,
						NotApprovedTotalReviews = 0,
						SubjectToAcl = false,
						Published = true,
						Deleted = false
					};
				_productService.InsertProduct(product);
				model.Id = product.Id;

				SaveSlug(model.SeName, product);
				CreateProductVariant(product, model);
				UpdateSpecifications(model);
				FillAvailableCategories(model);
				UpdateProductCategories(model);
				UpdateProductPictureUrl(model);
				return continueEditing ? RedirectToAction("EditBook", new { id = product.Id }) : RedirectToAction("List", "Product");
			}

			if (isbnExist)
				ErrorNotification(_localizationService.GetResource("Book.ISBNExist"));

			FillAvailableCategories(model);
			return View(model);
		}

		public ActionResult EditBook(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var product = _productService.GetProductById(id);
			if (product == null || product.Deleted)
				//No product found with the specified id
				return RedirectToAction("List");

			var model = new BookModel
			{
				Id = id,
				Name = product.Name,
				FullDescription = product.FullDescription,
				CategoryIds = product.ProductCategories.Select(x => x.CategoryId).ToArray(),
				Price = product.ProductVariants.Select(x => x.Price).First()
			};

			AddLocales(_languageService, model.Locales, (locale, languageId) =>
			{
				locale.Name = product.GetLocalized(x => x.Name, languageId, false, false);
				locale.FullDescription = product.GetLocalized(x => x.FullDescription, languageId, false, false);
			});

			AddAvailableSpecificationAttribute(model);
			FillSpecificationAttribute(model, id);
			FillProductPictureUrl(model, id);
			FillAvailableCategories(model);
			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		public ActionResult EditBook(BookModel model, bool continueEditing)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			var product = _productService.GetProductById(model.Id);
			if (product == null || product.Deleted)
				return RedirectToAction("List");

			GetSpecFromContext(model);

			if (ModelState.IsValid)
			{
				product.Name = model.Name;
				product.FullDescription = model.FullDescription;
				product.UpdatedOnUtc = DateTime.UtcNow;
				_productService.UpdateProduct(product);

				SaveSlug(model.SeName, product);
				UpdateSpecifications(model);
				FillAvailableCategories(model);
				UpdateProductCategories(model);
				UpdateProductPictureUrl(model);
				UpdateProductVariant(product, model);
				return continueEditing ? RedirectToAction("EditBook", new { id = product.Id }) : RedirectToAction("List", "Product");
			}

			return View(model);
		}

		[HttpPost]
		public ActionResult ImportExcel(FormCollection form)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageCatalog))
				return AccessDeniedView();

			try
			{
				var file = Request.Files["importexcelbookfile"];
				if (file != null && file.ContentLength > 0)
				{
					ImportBooksFromXlsx(file.InputStream);
				}
				else
				{
					ErrorNotification(_localizationService.GetResource("Admin.Common.UploadFile"));
					return RedirectToAction("List", "Product");
				}
				SuccessNotification(_localizationService.GetResource("Admin.Catalog.Products.Imported"));
				return RedirectToAction("List", "Product");
			}
			catch (Exception exc)
			{
				ErrorNotification(exc);
				return RedirectToAction("List", "Product");
			}
		}

		[NonAction]
		private void AddAvailableSpecificationAttribute(BookModel model)
		{
			if (model == null)
				throw new ArgumentNullException("model");

			var specificationAttributes = _specificationAttributeService.GetSpecificationAttributes();
			for (int i = 0; i < specificationAttributes.Count; i++)
			{
				model.Spec.Add(specificationAttributes[i].Name, string.Empty);
			}
		}

		[NonAction]
		private void FillSpecificationAttribute(BookModel model, int id)
		{
			var productSpecificationAttributes = _specificationAttributeService.GetProductSpecificationAttributesByProductId(id);
			for (int i = 0; i < productSpecificationAttributes.Count; i++)
			{
				var specName = productSpecificationAttributes[i].SpecificationAttributeOption.SpecificationAttribute.Name;
				model.Spec[specName] = productSpecificationAttributes[i].SpecificationAttributeOption.Name;
			}
		}

		//TODO rewrite to binder
		[NonAction]
		private void GetSpecFromContext(BookModel model)
		{
			model.Spec.Add(BookFields.Author.ToString(),
				HttpContext.Request.Form[BookFields.Author.ToString()]);
			model.Spec.Add(BookFields.Year.ToString(),
				HttpContext.Request.Form[BookFields.Year.ToString()]);
			model.Spec.Add(BookFields.ISBN.ToString(),
				HttpContext.Request.Form[BookFields.ISBN.ToString()]);
			model.Spec.Add(BookFields.Language.ToString(),
				HttpContext.Request.Form[BookFields.Language.ToString()]);
			model.Spec.Add(BookFields.Publisher.ToString(),
				HttpContext.Request.Form[BookFields.Publisher.ToString()]);
			model.Spec.Add(BookFields.Series.ToString(),
				HttpContext.Request.Form[BookFields.Series.ToString()]);
			model.Spec.Add(BookFields.Pages.ToString(),
				HttpContext.Request.Form[BookFields.Pages.ToString()]);
			model.Spec.Add(BookFields.Format.ToString(),
				HttpContext.Request.Form[BookFields.Format.ToString()]);
			model.Spec.Add(BookFields.Weight.ToString(),
				HttpContext.Request.Form[BookFields.Weight.ToString()]);
			model.Spec.Add(BookFields.Cover.ToString(),
				HttpContext.Request.Form[BookFields.Cover.ToString()]);
		}

		[NonAction]
		private void UpdateSpecifications(BookModel model)
		{
			DeleteAllProductSpecAttributes(model.Id);

			var availableSpecAttributes = _specificationAttributeService.GetSpecificationAttributes();

			foreach (var spec in model.Spec)
			{
				AddProductSpecification(availableSpecAttributes, spec.Key, spec.Value, model.Id);
			}
		}

		[NonAction]
		private void DeleteAllProductSpecAttributes(int id)
		{
			var productSpecAttributes = _specificationAttributeService.GetProductSpecificationAttributesByProductId(id);

			foreach (var attr in productSpecAttributes)
			{
				_specificationAttributeService.DeleteProductSpecificationAttribute(attr);
			}
		}

		[NonAction]
		private void AddProductSpecification(IList<SpecificationAttribute> attributes, string attribute, string option, int productId)
		{
			if (string.IsNullOrEmpty(option))
				return;

			var specification = attributes.Single(x => x.Name.Equals(attribute, StringComparison.CurrentCultureIgnoreCase));

			if (!specification.SpecificationAttributeOptions.Any(x => x.Name.Equals(option, StringComparison.CurrentCultureIgnoreCase)))
			{
				_specificationAttributeService.InsertSpecificationAttributeOption(
					new SpecificationAttributeOption
				{
					Name = option,
					SpecificationAttribute = specification
				});
				specification = _specificationAttributeService.GetSpecificationAttributeById(specification.Id);
			}

			var specAttributeOption = specification.SpecificationAttributeOptions.Single(x => x.Name.Equals(option));

			_specificationAttributeService.InsertProductSpecificationAttribute(
					new ProductSpecificationAttribute
					{
						ProductId = productId,
						SpecificationAttributeOptionId = specAttributeOption.Id,
						AllowFiltering = IsAllowFiltering(attribute),
						ShowOnProductPage = true
					});
		}

		[NonAction]
		private bool IsAllowFiltering(string attribute)
		{
			var allowFiltering = false;
			if (attribute.Equals(BookFields.Author.ToString())
				|| attribute.Equals(BookFields.Publisher.ToString())
				|| attribute.Equals(BookFields.Series.ToString()))
			{
				allowFiltering = true;
			}
			return allowFiltering;
		}

		[NonAction]
		private void FillProductPictureUrl(BookModel model, int id)
		{
			var productPictures = _productService.GetProductPicturesByProductId(id);
			model.PictureUrl = productPictures
				.Select(x => _pictureService.GetPictureUrl(x.PictureId))
				.FirstOrDefault();
		}

		[NonAction]
		private void UpdateProductPictureUrl(BookModel model)
		{
			var productPictures = _productService.GetProductPicturesByProductId(model.Id);
			var pictureUrl = productPictures
				.Select(x => _pictureService.GetPictureUrl(x.PictureId))
				.FirstOrDefault();

			if (!string.IsNullOrEmpty(pictureUrl)
				&& pictureUrl.Equals(model.PictureUrl))
				return;

			foreach (var image in productPictures)
			{
				_productService.DeleteProductPicture(image);
			}

			if (string.IsNullOrEmpty(model.PictureUrl))
				return;

			var bytes = LoadImage(model.PictureUrl);
			if (bytes.Length == 0)
				return;

			var picture = _pictureService.InsertPicture(bytes, "image/jpeg", _pictureService.GetPictureSeName(model.Name), true);

			_productService.InsertProductPicture(new ProductPicture
			{
				ProductId = model.Id,
				PictureId = picture.Id
			});
		}

		[NonAction]
		private byte[] LoadImage(string url)
		{
			var buf = new byte[0];
			try
			{
				HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
				WebResponse myResp = myReq.GetResponse();

				int byteCount = Convert.ToInt32(myResp.ContentLength);
				using (BinaryReader reader = new BinaryReader(myResp.GetResponseStream()))
				{
					buf = reader.ReadBytes(byteCount);
				}
			}
			catch (Exception)
			{
				return buf;
			}
			return buf;
		}

		[NonAction]
		private void FillAvailableCategories(BookModel model)
		{
			foreach (var c in _categoryService.GetAllCategories(showHidden: true))
				model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService), Value = c.Id.ToString() });
		}

		[NonAction]
		private void UpdateProductCategories(BookModel model)
		{
			var productCategories = _categoryService.GetProductCategoriesByProductId(model.Id, true);
			var productCategoriesId = productCategories.Select(x => x.CategoryId).ToArray();
			var categoriesForAdd = model.CategoryIds.Except<int>(productCategoriesId);
			var categoriesForRemove = productCategoriesId.Except<int>(model.CategoryIds);

			foreach (var categoryId in categoriesForRemove)
			{
				_categoryService.DeleteProductCategory(productCategories.Single(x => x.CategoryId == categoryId));
			}

			foreach (var categoryId in categoriesForAdd)
			{
				_categoryService.InsertProductCategory(new ProductCategory
				{
					CategoryId = categoryId,
					ProductId = model.Id
				});
			}
		}

		[NonAction]
		private void UpdateProductVariant(Product product, BookModel model)
		{
			var variant = product.ProductVariants.FirstOrDefault();

			if (variant == null)
				return;

			variant.Price = model.Price;
			variant.UpdatedOnUtc = DateTime.UtcNow;
			_productService.UpdateProductVariant(variant);
		}

		[NonAction]
		private void CreateProductVariant(Product product, BookModel model)
		{
			_productService.InsertProductVariant(new ProductVariant
			{
				ProductId = product.Id,
				IsGiftCard = false,
				GiftCardTypeId = 0,
				RequireOtherProducts = false,
				AutomaticallyAddRequiredProductVariants = false,
				IsDownload = false,
				DownloadId = 0,
				UnlimitedDownloads = true,
				MaxNumberOfDownloads = 10,
				DownloadActivationTypeId = 1,
				HasSampleDownload = false,
				SampleDownloadId = 0,
				HasUserAgreement = false,
				IsRecurring = false,
				RecurringCycleLength = 100,
				RecurringCyclePeriodId = 0,
				RecurringTotalCycles = 10,
				IsShipEnabled = true,
				IsFreeShipping = false,
				AdditionalShippingCharge = 0,
				IsTaxExempt = false,
				TaxCategoryId = 0,
				ManageInventoryMethodId = 0,
				StockQuantity = 1,
				DisplayStockAvailability = false,
				MinStockQuantity = 0,
				LowStockActivityId = 0,
				NotifyAdminForQuantityBelow = 1,
				BackorderModeId = 0,
				AllowBackInStockSubscriptions = false,
				OrderMinimumQuantity = 1,
				OrderMaximumQuantity = 100,
				DisableBuyButton = false,
				DisableWishlistButton = false,
				AvailableForPreOrder = false,
				CallForPrice = false,
				Price = model.Price,
				OldPrice = 0,
				ProductCost = 0,
				CustomerEntersPrice = false,
				MinimumCustomerEnteredPrice = 0,
				MaximumCustomerEnteredPrice = 1000,
				HasTierPrices = false,
				HasDiscountsApplied = false,
				Weight = 0,
				Length = 0,
				Width = 0,
				Height = 0,
				PictureId = 0,
				Published = true,
				Deleted = false,
				DisplayOrder = 0,
				CreatedOnUtc = DateTime.UtcNow,
				UpdatedOnUtc = DateTime.UtcNow
			});
		}

		[NonAction]
		private void SaveSlug(string seName, Product product)
		{
			var seNameValue = product.ValidateSeName(seName, product.Name, true);
			_urlRecordService.SaveSlug(product, seNameValue, 0);
		}

		[NonAction]
		private ProductTemplate GetProductTemplate()
		{
			return _productTemplateService.GetAllProductTemplates()
					.First(x => x.Name == "Lavka Variant");
		}

		[NonAction]
		public void ImportBooksFromXlsx(Stream stream)
		{
			// ok, we can run the real code of the sample now
			using (var xlPackage = new ExcelPackage(stream))
			{
				// get the first worksheet in the workbook
				var worksheet = xlPackage.Workbook.Worksheets.FirstOrDefault();
				if (worksheet == null)
					throw new NopException("No worksheet found");

				int iRow = 2;
				while (true)
				{
					bool allColumnsAreEmpty = true;
					for (var i = 1; i <= properties.Length; i++)
						if (worksheet.Cells[iRow, i].Value != null && !String.IsNullOrEmpty(worksheet.Cells[iRow, i].Value.ToString()))
						{
							allColumnsAreEmpty = false;
							break;
						}
					if (allColumnsAreEmpty)
						break;

					var model = new BookModel
					{
						Name = worksheet.Cells[iRow, GetColumnIndex(properties, BookFields.Name.ToString())].Value as string,
						FullDescription = worksheet.Cells[iRow, GetColumnIndex(properties, BookFields.Descripton.ToString())].Value as string,
						Price = Convert.ToDecimal(worksheet.Cells[iRow, GetColumnIndex(properties, BookFields.Price.ToString())].Value)
					};
					model.Spec.Add(BookFields.Author.ToString(), GetSpecCellValue(BookFields.Author, worksheet, iRow));
					model.Spec.Add(BookFields.ISBN.ToString(), GetSpecCellValue(BookFields.ISBN, worksheet, iRow));
					model.Spec.Add(BookFields.Language.ToString(), GetSpecCellValue(BookFields.Language, worksheet, iRow));
					model.Spec.Add(BookFields.Publisher.ToString(), GetSpecCellValue(BookFields.Publisher, worksheet, iRow));
					model.Spec.Add(BookFields.Series.ToString(), GetSpecCellValue(BookFields.Series, worksheet, iRow));
					model.Spec.Add(BookFields.Pages.ToString(), GetSpecCellValue(BookFields.Pages, worksheet, iRow));
					model.Spec.Add(BookFields.Format.ToString(), GetSpecCellValue(BookFields.Format, worksheet, iRow));
					model.Spec.Add(BookFields.Weight.ToString(), GetSpecCellValue(BookFields.Weight, worksheet, iRow));
					model.Spec.Add(BookFields.Cover.ToString(), GetSpecCellValue(BookFields.Cover, worksheet, iRow));
					model.Spec.Add(BookFields.Year.ToString(), GetSpecCellValue(BookFields.Year, worksheet, iRow));

					var isISBNExist = IsISBNExist(model);

					if (ModelState.IsValid && !isISBNExist)
					{
						var product = new Product
							{
								Name = model.Name,
								FullDescription = model.FullDescription,
								CreatedOnUtc = DateTime.UtcNow,
								UpdatedOnUtc = DateTime.UtcNow,
								ProductTemplateId = GetProductTemplate().Id,
								ShowOnHomePage = true,
								AllowCustomerReviews = true,
								ApprovedRatingSum = 0,
								NotApprovedRatingSum = 0,
								ApprovedTotalReviews = 0,
								NotApprovedTotalReviews = 0,
								SubjectToAcl = false,
								Published = true,
								Deleted = false
							};
						_productService.InsertProduct(product);
						model.Id = product.Id;

						SaveSlug(model.SeName, product);
						CreateProductVariant(product, model);
						AssingToImportCategory(product);
						UpdateSpecifications(model);
					}
					else
					{
						if (isISBNExist)
						{
							ErrorNotification(
									string.Format("{0}: {1}",
										model.Spec[BookFields.ISBN.ToString()],
										_localizationService.GetResource("Book.ISBNExist")));
						}
						if (!ModelState.IsValid)
						{
							ErrorNotification(
								string.Format("Errors ({0}): {1}",
									model.Name,
									string.Join(";# ", ModelState.Values
										.SelectMany(x => x.Errors)
										.Select(x => x.ErrorMessage))));
						}
					}

					//next product
					iRow++;
				}
			}
		}

		[NonAction]
		protected string GetSpecCellValue(BookFields field, ExcelWorksheet worksheet, int index)
		{
			return Convert.ToString(worksheet.Cells[index, GetColumnIndex(properties, field.ToString())].Value);
		}

		[NonAction]
		protected virtual int GetColumnIndex(string[] properties, string columnName)
		{
			if (properties == null)
				throw new ArgumentNullException("properties");

			if (columnName == null)
				throw new ArgumentNullException("columnName");

			for (int i = 0; i < properties.Length; i++)
				if (properties[i].Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
					return i + 1; //excel indexes start from 1
			return 0;
		}

		[NonAction]
		protected void AssingToImportCategory(Product product)
		{
			var category = _categoryService.GetAllCategories(showHidden: true)
				.FirstOrDefault(x => x.Name == "IMPORTED");

			if (category == null)
				return;

			_categoryService.InsertProductCategory(new ProductCategory
			{
				CategoryId = category.Id,
				ProductId = product.Id
			});
		}

		[NonAction]
		protected bool IsISBNExist(BookModel model)
		{
			var isbn = model.Spec[BookFields.ISBN.ToString()];
			return _specificationAttributeService.SearchSpecificationAttributeOptionByISBN(isbn).Any();
		}
	}
}

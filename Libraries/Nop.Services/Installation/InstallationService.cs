using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Forums;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Messages;
using Nop.Core.Domain.News;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Polls;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tasks;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Topics;
using Nop.Core.Infrastructure;
using Nop.Core.IO;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Media;
using Nop.Services.Localization;
using Nop.Services.Seo;
using Nop.Core.Domain.Seo;

namespace Nop.Services.Installation
{
    public partial class InstallationService : IInstallationService
    {
        #region Fields

        private readonly IRepository<MeasureDimension> _measureDimensionRepository;
        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IRepository<TaxCategory> _taxCategoryRepository;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<Currency> _currencyRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<CustomerRole> _customerRoleRepository;
        private readonly IRepository<SpecificationAttribute> _specificationAttributeRepository;
        private readonly IRepository<ProductAttribute> _productAttributeRepository;
        private readonly IRepository<Category> _categoryRepository;
        private readonly IRepository<Manufacturer> _manufacturerRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly IRepository<RelatedProduct> _relatedProductRepository;
        private readonly IRepository<EmailAccount> _emailAccountRepository;
        private readonly IRepository<MessageTemplate> _messageTemplateRepository;
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<Country> _countryRepository;
        private readonly IRepository<StateProvince> _stateProvinceRepository;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<Poll> _pollRepository;
        private readonly IRepository<ShippingMethod> _shippingMethodRepository;
        private readonly IRepository<ActivityLogType> _activityLogTypeRepository;
        private readonly IRepository<ProductTag> _productTagRepository;
        private readonly IRepository<ProductTemplate> _productTemplateRepository;
        private readonly IRepository<CategoryTemplate> _categoryTemplateRepository;
        private readonly IRepository<ManufacturerTemplate> _manufacturerTemplateRepository;
        private readonly IRepository<ScheduleTask> _scheduleTaskRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public InstallationService(IRepository<MeasureDimension> measureDimensionRepository,
            IRepository<MeasureWeight> measureWeightRepository,
            IRepository<TaxCategory> taxCategoryRepository,
            IRepository<Language> languageRepository,
            IRepository<Currency> currencyRepository,
            IRepository<Customer> customerRepository,
            IRepository<CustomerRole> customerRoleRepository,
            IRepository<SpecificationAttribute> specificationAttributeRepository,
            IRepository<ProductAttribute> productAttributeRepository,
            IRepository<Category> categoryRepository,
            IRepository<Manufacturer> manufacturerRepository,
            IRepository<Product> productRepository,
            IRepository<UrlRecord> urlRecordRepository,
            IRepository<RelatedProduct> relatedProductRepository,
            IRepository<EmailAccount> emailAccountRepository,
            IRepository<MessageTemplate> messageTemplateRepository,
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<Country> countryRepository,
            IRepository<StateProvince> stateProvinceRepository,
            IRepository<Discount> discountRepository,
            IRepository<BlogPost> blogPostRepository,
            IRepository<Topic> topicRepository,
            IRepository<NewsItem> newsItemRepository,
            IRepository<Poll> pollRepository,
            IRepository<ShippingMethod> shippingMethodRepository,
            IRepository<ActivityLogType> activityLogTypeRepository,
            IRepository<ProductTag> productTagRepository,
            IRepository<ProductTemplate> productTemplateRepository,
            IRepository<CategoryTemplate> categoryTemplateRepository,
            IRepository<ManufacturerTemplate> manufacturerTemplateRepository,
            IRepository<ScheduleTask> scheduleTaskRepository,
            IGenericAttributeService genericAttributeService,
            IWebHelper webHelper)
        {
            this._measureDimensionRepository = measureDimensionRepository;
            this._measureWeightRepository = measureWeightRepository;
            this._taxCategoryRepository = taxCategoryRepository;
            this._languageRepository = languageRepository;
            this._currencyRepository = currencyRepository;
            this._customerRepository = customerRepository;
            this._customerRoleRepository = customerRoleRepository;
            this._specificationAttributeRepository = specificationAttributeRepository;
            this._productAttributeRepository = productAttributeRepository;
            this._categoryRepository = categoryRepository;
            this._manufacturerRepository = manufacturerRepository;
            this._productRepository = productRepository;
            this._urlRecordRepository = urlRecordRepository;
            this._relatedProductRepository = relatedProductRepository;
            this._emailAccountRepository = emailAccountRepository;
            this._messageTemplateRepository = messageTemplateRepository;
            this._forumGroupRepository = forumGroupRepository;
            this._forumRepository = forumRepository;
            this._countryRepository = countryRepository;
            this._stateProvinceRepository = stateProvinceRepository;
            this._discountRepository = discountRepository;
            this._blogPostRepository = blogPostRepository;
            this._topicRepository = topicRepository;
            this._newsItemRepository = newsItemRepository;
            this._pollRepository = pollRepository;
            this._shippingMethodRepository = shippingMethodRepository;
            this._activityLogTypeRepository = activityLogTypeRepository;
            this._productTagRepository = productTagRepository;
            this._productTemplateRepository = productTemplateRepository;
            this._categoryTemplateRepository = categoryTemplateRepository;
            this._manufacturerTemplateRepository = manufacturerTemplateRepository;
            this._scheduleTaskRepository = scheduleTaskRepository;
            this._genericAttributeService = genericAttributeService;
            this._webHelper = webHelper;
        }

        #endregion

        #region Classes

        private class LocaleStringResourceParent : LocaleStringResource
        {
            public LocaleStringResourceParent(XmlNode localStringResource, string nameSpace = "")
            {
                Namespace = nameSpace;
                var resNameAttribute = localStringResource.Attributes["Name"];
                var resValueNode = localStringResource.SelectSingleNode("Value");

                if (resNameAttribute == null)
                {
                    throw new NopException("All language resources must have an attribute Name=\"Value\".");
                }
                var resName = resNameAttribute.Value.Trim();
                if (string.IsNullOrEmpty(resName))
                {
                    throw new NopException("All languages resource attributes 'Name' must have a value.'");
                }
                ResourceName = resName;

                if (resValueNode == null || string.IsNullOrEmpty(resValueNode.InnerText.Trim()))
                {
                    IsPersistable = false;
                }
                else
                {
                    IsPersistable = true;
                    ResourceValue = resValueNode.InnerText.Trim();
                }

                foreach (XmlNode childResource in localStringResource.SelectNodes("Children/LocaleResource"))
                {
                    ChildLocaleStringResources.Add(new LocaleStringResourceParent(childResource, NameWithNamespace));
                }
            }
            public string Namespace { get; set; }
            public IList<LocaleStringResourceParent> ChildLocaleStringResources = new List<LocaleStringResourceParent>();

            public bool IsPersistable { get; set; }

            public string NameWithNamespace
            {
                get
                {
                    var newNamespace = Namespace;
                    if (!string.IsNullOrEmpty(newNamespace))
                    {
                        newNamespace += ".";
                    }
                    return newNamespace + ResourceName;
                }
            }
        }

        private class ComparisonComparer<T> : IComparer<T>, IComparer
        {
            private readonly Comparison<T> _comparison;

            public ComparisonComparer(Comparison<T> comparison)
            {
                _comparison = comparison;
            }

            public int Compare(T x, T y)
            {
                return _comparison(x, y);
            }

            public int Compare(object o1, object o2)
            {
                return _comparison((T)o1, (T)o2);
            }
        }

        #endregion

        #region Utilities

        private void RecursivelyWriteResource(LocaleStringResourceParent resource, XmlWriter writer)
        {
            //The value isn't actually used, but the name is used to create a namespace.
            if (resource.IsPersistable)
            {
                writer.WriteStartElement("LocaleResource", "");

                writer.WriteStartAttribute("Name", "");
                writer.WriteString(resource.NameWithNamespace);
                writer.WriteEndAttribute();

                writer.WriteStartElement("Value", "");
                writer.WriteString(resource.ResourceValue);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }

            foreach (var child in resource.ChildLocaleStringResources)
            {
                RecursivelyWriteResource(child, writer);
            }

        }

        private void RecursivelySortChildrenResource(LocaleStringResourceParent resource)
        {
            ArrayList.Adapter((IList)resource.ChildLocaleStringResources).Sort(new InstallationService.ComparisonComparer<LocaleStringResourceParent>((x1, x2) => x1.ResourceName.CompareTo(x2.ResourceName)));

            foreach (var child in resource.ChildLocaleStringResources)
            {
                RecursivelySortChildrenResource(child);
            }

        }

        protected virtual void InstallMeasures()
        {
            var measureDimensions = new List<MeasureDimension>()
            {
                new MeasureDimension()
                {
                    Name = "cantimeter(s)",
                    SystemKeyword = "cantimeters",
                    Ratio = 1,
                    DisplayOrder = 1,
                },
                new MeasureDimension()
                {
                    Name = "millimetre(s)",
                    SystemKeyword = "millimetres",
                    Ratio = 10M,
                    DisplayOrder = 2,
                }
            };

            measureDimensions.ForEach(x => _measureDimensionRepository.Insert(x));

            var measureWeights = new List<MeasureWeight>()
            {
                new MeasureWeight()
                {
                    Name = "kg(s)",
                    SystemKeyword = "kg",
                    Ratio = 1,
                    DisplayOrder = 1,
                },
                new MeasureWeight()
                {
                    Name = "gram(s)",
                    SystemKeyword = "grams",
                    Ratio = 1000M,
                    DisplayOrder = 2,
                }
            };

            measureWeights.ForEach(x => _measureWeightRepository.Insert(x));
        }

        protected virtual void InstallTaxCategories()
        {
        }

        protected virtual void InstallLanguages()
        {
            var language = new Language
            {
                Name = "Russian",
                LanguageCulture = "ru-RU",
                UniqueSeoCode = "ru",
                FlagImageFileName = "ru.png",
                Published = true,
                DisplayOrder = 1
            };
            _languageRepository.Insert(language);
        }

        protected virtual void InstallLocaleResources()
        {
            //'English' language
            var language = _languageRepository.Table.Where(l => l.Name == "Russian").Single();

            //save resoureces
            foreach (var filePath in System.IO.Directory.EnumerateFiles(_webHelper.MapPath("~/App_Data/Localization/"), "*.nopres.xml", SearchOption.TopDirectoryOnly))
            {
                #region Parse resource files (with <Children> elements)
                //read and parse original file with resources (with <Children> elements)

                var originalXmlDocument = new XmlDocument();
                originalXmlDocument.Load(filePath);

                var resources = new List<LocaleStringResourceParent>();

                foreach (XmlNode resNode in originalXmlDocument.SelectNodes(@"//Language/LocaleResource"))
                    resources.Add(new LocaleStringResourceParent(resNode));

                resources.Sort((x1, x2) => x1.ResourceName.CompareTo(x2.ResourceName));

                foreach (var resource in resources)
                    RecursivelySortChildrenResource(resource);

                var sb = new StringBuilder();
                var writer = XmlWriter.Create(sb);
                writer.WriteStartDocument();
                writer.WriteStartElement("Language", "");

                writer.WriteStartAttribute("Name", "");
                writer.WriteString(originalXmlDocument.SelectSingleNode(@"//Language").Attributes["Name"].InnerText.Trim());
                writer.WriteEndAttribute();

                foreach (var resource in resources)
                    RecursivelyWriteResource(resource, writer);

                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();

                var parsedXml = sb.ToString();
                #endregion

                //now we have a parsed XML file (the same structure as exported language packs)
                //let's save resources
                var localizationService = EngineContext.Current.Resolve<ILocalizationService>();
                localizationService.ImportResourcesFromXml(language, parsedXml);
            }

        }

        protected virtual void InstallCurrencies()
        {
            var currencies = new List<Currency>()
            {
                new Currency
                {
                    Name = "Ukraine Hryvnia",
                    CurrencyCode = "UAH",
                    Rate = 1,
                    DisplayLocale = "uk-UA",
                    CustomFormatting = "0.00 грн",
                    Published = true,
                    DisplayOrder = 1,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                }
            };
            currencies.ForEach(c => _currencyRepository.Insert(c));
        }

        protected virtual void InstallCountriesAndStates()
        {
            var cUsa = new Country
            {
                Name = "Ukraine",
                AllowsBilling = true,
                AllowsShipping = true,
                TwoLetterIsoCode = "UA",
                ThreeLetterIsoCode = "UKR",
                NumericIsoCode = 804,
                SubjectToVat = false,
                DisplayOrder = 1,
                Published = true,
            };
            _countryRepository.Insert(cUsa);
        }

        protected virtual void InstallShippingMethods()
        {
            var shippingMethods = new List<ShippingMethod>
                                {
                                    new ShippingMethod
                                        {
                                            Name = "In-Store Pickup",
                                            Description ="Pick up your items at the store",
                                            DisplayOrder = 0
                                        },
                                    new ShippingMethod
                                        {
                                            Name = "By Ground",
                                            Description ="Compared to other shipping methods, like by flight or over seas, ground shipping is carried out closer to the earth",
                                            DisplayOrder = 1
                                        },
                                    new ShippingMethod
                                        {
                                            Name = "By Air",
                                            Description ="The one day air shipping",
                                            DisplayOrder = 3
                                        },
                                };
            shippingMethods.ForEach(sm => _shippingMethodRepository.Insert(sm));

        }

        protected virtual void InstallCustomersAndUsers(string defaultUserEmail, string defaultUserPassword)
        {
            var crAdministrators = new CustomerRole
            {
                Name = "Administrators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Administrators,
            };
            var crForumModerators = new CustomerRole
            {
                Name = "Forum Moderators",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.ForumModerators,
            };
            var crRegistered = new CustomerRole
            {
                Name = "Registered",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Registered,
            };
            var crGuests = new CustomerRole
            {
                Name = "Guests",
                Active = true,
                IsSystemRole = true,
                SystemName = SystemCustomerRoleNames.Guests,
            };
            var customerRoles = new List<CustomerRole>
                                {
                                    crAdministrators,
                                    crForumModerators,
                                    crRegistered,
                                    crGuests
                                };
            customerRoles.ForEach(cr => _customerRoleRepository.Insert(cr));

            //admin user
            var adminUser = new Customer()
            {
                CustomerGuid = Guid.NewGuid(),
                Email = defaultUserEmail,
                Username = defaultUserEmail,
                Password = defaultUserPassword,
                PasswordFormat = PasswordFormat.Clear,
                PasswordSalt = "",
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            var defaultAdminUserAddress = new Address()
            {
                FirstName = "John",
                LastName = "Smith",
                PhoneNumber = "12345678",
                Email = "admin@yourStore.com",
                FaxNumber = "",
                Company = "Nop Solutions",
                Address1 = "21 West 52nd Street",
                Address2 = "",
                City = "New York",
                StateProvince = _stateProvinceRepository.Table.Where(sp => sp.Name == "New York").FirstOrDefault(),
                Country = _countryRepository.Table.Where(c => c.ThreeLetterIsoCode == "USA").FirstOrDefault(),
                ZipPostalCode = "10021",
                CreatedOnUtc = DateTime.UtcNow,
            };
            adminUser.Addresses.Add(defaultAdminUserAddress);
            adminUser.BillingAddress = defaultAdminUserAddress;
            adminUser.ShippingAddress = defaultAdminUserAddress;
            adminUser.CustomerRoles.Add(crAdministrators);
            adminUser.CustomerRoles.Add(crForumModerators);
            adminUser.CustomerRoles.Add(crRegistered);
            _customerRepository.Insert(adminUser);
            //set default customer name
            _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.FirstName, "John");
            _genericAttributeService.SaveAttribute(adminUser, SystemCustomerAttributeNames.LastName, "Smith");


            //search engine (crawler) built-in user
            var searchEngineUser = new Customer()
            {
                Email = "builtin@search_engine_record.com",
                CustomerGuid = Guid.NewGuid(),
                PasswordFormat = PasswordFormat.Clear,
                AdminComment = "Built-in system guest record used for requests from search engines.",
                Active = true,
                IsSystemAccount = true,
                SystemName = SystemCustomerNames.SearchEngine,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };
            searchEngineUser.CustomerRoles.Add(crGuests);
            _customerRepository.Insert(searchEngineUser);
        }

        protected virtual void HashDefaultCustomerPassword(string defaultUserEmail, string defaultUserPassword)
        {
            var customerRegistrationService = EngineContext.Current.Resolve<ICustomerRegistrationService>();
            customerRegistrationService.ChangePassword(new ChangePasswordRequest(defaultUserEmail, false,
                 PasswordFormat.Hashed, defaultUserPassword));
        }

        protected virtual void InstallEmailAccounts()
        {
            var emailAccounts = new List<EmailAccount>
                               {
                                   new EmailAccount
                                       {
                                           Email = "test@mail.com",
                                           DisplayName = "General contact",
                                           Host = "smtp.mail.com",
                                           Port = 25,
                                           Username = "123",
                                           Password = "123",
                                           EnableSsl = false,
                                           UseDefaultCredentials = false
                                       },
                                   new EmailAccount
                                       {
                                           Email = "test@mail.com",
                                           DisplayName = "Sales representative",
                                           Host = "smtp.mail.com",
                                           Port = 25,
                                           Username = "123",
                                           Password = "123",
                                           EnableSsl = false,
                                           UseDefaultCredentials = false
                                       },
                                   new EmailAccount
                                       {
                                           Email = "test@mail.com",
                                           DisplayName = "Customer support",
                                           Host = "smtp.mail.com",
                                           Port = 25,
                                           Username = "123",
                                           Password = "123",
                                           EnableSsl = false,
                                           UseDefaultCredentials = false
                                       }, 
                               };
            emailAccounts.ForEach(ea => _emailAccountRepository.Insert(ea));

        }

        protected virtual void InstallMessageTemplates()
        {
            var eaGeneral = _emailAccountRepository.Table.Where(ea => ea.DisplayName.Equals("General contact")).FirstOrDefault();
            //var eaSale = _emailAccountRepository.Table.Where(ea => ea.DisplayName.Equals("Sales representative")).FirstOrDefault();
            //var eaCustomer = _emailAccountRepository.Table.Where(ea => ea.DisplayName.Equals("Customer support")).FirstOrDefault();
            var messageTemplates = new List<MessageTemplate>
                               {
                                   new MessageTemplate
                                       {
                                           Name = "Blog.BlogComment",
                                           Subject = "%Store.Name%. New blog comment.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new blog comment has been created for blog post \"%BlogComment.BlogPostTitle%\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.BackInStock",
                                           Subject = "%Store.Name%. Back in stock notification",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Customer.FullName%, <br />Product \"%BackInStockSubscription.ProductName%\" is in stock.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.EmailValidationMessage",
                                           Subject = "%Store.Name%. Email validation",
                                           Body = "<a href=\"%Store.URL%\">%Store.Name%</a>  <br />  <br />  To activate your account <a href=\"%Customer.AccountActivationURL%\">click here</a>.     <br />  <br />  %Store.Name%",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.NewPM",
                                           Subject = "%Store.Name%. You have received a new private message",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />You have received a new private message.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.PasswordRecovery",
                                           Subject = "%Store.Name%. Password recovery",
                                           Body = "<a href=\"%Store.URL%\">%Store.Name%</a>  <br />  <br />  To change your password <a href=\"%Customer.PasswordRecoveryURL%\">click here</a>.     <br />  <br />  %Store.Name%",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.WelcomeMessage",
                                           Subject = "Welcome to %Store.Name%",
                                           Body = "We welcome you to <a href=\"%Store.URL%\"> %Store.Name%</a>.<br /><br />You can now take part in the various services we have to offer you. Some of these services include:<br /><br />Permanent Cart - Any products added to your online cart remain there until you remove them, or check them out.<br />Address Book - We can now deliver your products to another address other than yours! This is perfect to send birthday gifts direct to the birthday-person themselves.<br />Order History - View your history of purchases that you have made with us.<br />Products Reviews - Share your opinions on products with our other customers.<br /><br />For help with any of our online services, please email the store-owner: <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.<br /><br />Note: This email address was given to us by one of our customers. If you did not signup to be a member, please send an email to <a href=\"mailto:%Store.Email%\">%Store.Email%</a>.",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Forums.NewForumPost",
                                           Subject = "%Store.Name%. New Post Notification.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new post has been created in the topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.<br /><br />Click <a href=\"%Forums.TopicURL%\">here</a> for more info.<br /><br />Post author: %Forums.PostAuthor%<br />Post body: %Forums.PostBody%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Forums.NewForumTopic",
                                           Subject = "%Store.Name%. New Topic Notification.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new topic <a href=\"%Forums.TopicURL%\">\"%Forums.TopicName%\"</a> has been created at <a href=\"%Forums.ForumURL%\">\"%Forums.ForumName%\"</a> forum.<br /><br />Click <a href=\"%Forums.TopicURL%\">here</a> for more info.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "GiftCard.Notification",
                                           Subject = "%GiftCard.SenderName% has sent you a gift card for %Store.Name%",
                                           Body = "<p>You have received a gift card for %Store.Name%</p><p>Dear %GiftCard.RecipientName%, <br /><br />%GiftCard.SenderName% (%GiftCard.SenderEmail%) has sent you a %GiftCard.Amount% gift cart for <a href=\"%Store.URL%\"> %Store.Name%</a></p><p>You gift card code is %GiftCard.CouponCode%</p><p>%GiftCard.Message%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewCustomer.Notification",
                                           Subject = "%Store.Name%. New customer registration",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new customer registered with your store. Below are the customer's details:<br />Full name: %Customer.FullName%<br />Email: %Customer.Email%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewReturnRequest.StoreOwnerNotification",
                                           Subject = "%Store.Name%. New return request.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% has just submitted a new return request. Details are below:<br />Request ID: %ReturnRequest.ID%<br />Product: %ReturnRequest.Product.Quantity% x Product: %ReturnRequest.Product.Name%<br />Reason for return: %ReturnRequest.Reason%<br />Requested action: %ReturnRequest.RequestedAction%<br />Customer comments:<br />%ReturnRequest.CustomerComment%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "News.NewsComment",
                                           Subject = "%Store.Name%. New news comment.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new news comment has been created for news \"%NewsComment.NewsTitle%\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewsLetterSubscription.ActivationMessage",
                                           Subject = "%Store.Name%. Subscription activation message.",
                                           Body = "<p><a href=\"%NewsLetterSubscription.ActivationUrl%\">Click here to confirm your subscription to our list.</a></p><p>If you received this email by mistake, simply delete it.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewsLetterSubscription.DeactivationMessage",
                                           Subject = "%Store.Name%. Subscription deactivation message.",
                                           Body = "<p><a href=\"%NewsLetterSubscription.DeactivationUrl%\">Click here to unsubscribe from news letters.</a></p><p>If you received this email by mistake, simply delete it.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "NewVATSubmitted.StoreOwnerNotification",
                                           Subject = "%Store.Name%. New VAT number is submitted.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% (%Customer.Email%) has just submitted a new VAT number. Details are below:<br />VAT number: %Customer.VatNumber%<br />VAT number status: %Customer.VatNumberStatus%<br />Received name: %VatValidationResult.Name%<br />Received address: %VatValidationResult.Address%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderCancelled.CustomerNotification",
                                           Subject = "%Store.Name%. Your order cancelled",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Your order has been cancelled. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderCompleted.CustomerNotification",
                                           Subject = "%Store.Name%. Your order completed",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Your order has been completed. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ShipmentDelivered.CustomerNotification",
                                           Subject = "Your order from %Store.Name% has been delivered.",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /> <br /> Hello %Order.CustomerFullName%, <br /> Good news! You order has been delivered. <br /> Order Number: %Order.OrderNumber%<br /> Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br /> Date Ordered: %Order.CreatedOn%<br /> <br /> <br /> <br /> Billing Address<br /> %Order.BillingFirstName% %Order.BillingLastName%<br /> %Order.BillingAddress1%<br /> %Order.BillingCity% %Order.BillingZipPostalCode%<br /> %Order.BillingStateProvince% %Order.BillingCountry%<br /> <br /> <br /> <br /> Shipping Address<br /> %Order.ShippingFirstName% %Order.ShippingLastName%<br /> %Order.ShippingAddress1%<br /> %Order.ShippingCity% %Order.ShippingZipPostalCode%<br /> %Order.ShippingStateProvince% %Order.ShippingCountry%<br /> <br /> Shipping Method: %Order.ShippingMethod% <br /> <br /> Delivered Products: <br /> <br /> %Shipment.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.CustomerNotification",
                                           Subject = "Order receipt from %Store.Name%.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%, <br />Thanks for buying from <a href=\"%Store.URL%\">%Store.Name%</a>. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Order Details: <a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "OrderPlaced.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Purchase Receipt for Order #%Order.OrderNumber%",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Order.CustomerFullName% (%Order.CustomerEmail%) has just placed an order from your store. Below is the summary of the order. <br /><br />Order Number: %Order.OrderNumber%<br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod%<br /><br />%Order.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ShipmentSent.CustomerNotification",
                                           Subject = "Your order from %Store.Name% has been shipped.",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /><br />Hello %Order.CustomerFullName%!, <br />Good news! You order has been shipped. <br />Order Number: %Order.OrderNumber%<br />Order Details: <a href=\"%Order.OrderURLForCustomer%\" target=\"_blank\">%Order.OrderURLForCustomer%</a><br />Date Ordered: %Order.CreatedOn%<br /><br /><br /><br />Billing Address<br />%Order.BillingFirstName% %Order.BillingLastName%<br />%Order.BillingAddress1%<br />%Order.BillingCity% %Order.BillingZipPostalCode%<br />%Order.BillingStateProvince% %Order.BillingCountry%<br /><br /><br /><br />Shipping Address<br />%Order.ShippingFirstName% %Order.ShippingLastName%<br />%Order.ShippingAddress1%<br />%Order.ShippingCity% %Order.ShippingZipPostalCode%<br />%Order.ShippingStateProvince% %Order.ShippingCountry%<br /><br />Shipping Method: %Order.ShippingMethod% <br /> <br /> Shipped Products: <br /> <br /> %Shipment.Product(s)%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Product.ProductReview",
                                           Subject = "%Store.Name%. New product review.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />A new product review has been written for product \"%ProductReview.ProductName%\".</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "QuantityBelow.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Quantity below notification. %ProductVariant.FullProductName%",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%ProductVariant.FullProductName% (ID: %ProductVariant.ID%) low quantity. <br /><br />Quantity: %ProductVariant.StockQuantity%<br /></p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "ReturnRequestStatusChanged.CustomerNotification",
                                           Subject = "%Store.Name%. Return request status was changed.",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Customer.FullName%,<br />Your return request #%ReturnRequest.ID% status has been changed.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Service.EmailAFriend",
                                           Subject = "%Store.Name%. Referred Item",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /><br />%EmailAFriend.Email% was shopping on %Store.Name% and wanted to share the following item with you. <br /><br /><b><a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">%Product.Name%</a></b> <br />%Product.ShortDescription% <br /><br />For more info click <a target=\"_blank\" href=\"%Product.ProductURLForCustomer%\">here</a> <br /><br /><br />%EmailAFriend.PersonalMessage%<br /><br />%Store.Name%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Wishlist.EmailAFriend",
                                           Subject = "%Store.Name%. Wishlist",
                                           Body = "<p><a href=\"%Store.URL%\"> %Store.Name%</a> <br /><br />%Wishlist.Email% was shopping on %Store.Name% and wanted to share a wishlist with you. <br /><br /><br />For more info click <a target=\"_blank\" href=\"%Wishlist.URLForCustomer%\">here</a> <br /><br /><br />%Wishlist.PersonalMessage%<br /><br />%Store.Name%</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "Customer.NewOrderNote",
                                           Subject = "%Store.Name%. New order note has been added",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />Hello %Customer.FullName%, <br />New order note has been added to your account:<br />\"%Order.NewNoteText%\".<br /><a target=\"_blank\" href=\"%Order.OrderURLForCustomer%\">%Order.OrderURLForCustomer%</a></p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                                   new MessageTemplate
                                       {
                                           Name = "RecurringPaymentCancelled.StoreOwnerNotification",
                                           Subject = "%Store.Name%. Recurring payment cancelled",
                                           Body = "<p><a href=\"%Store.URL%\">%Store.Name%</a> <br /><br />%Customer.FullName% (%Customer.Email%) has just cancelled a recurring payment ID=%RecurringPayment.ID%.</p>",
                                           IsActive = true,
                                           EmailAccountId = eaGeneral.Id,
                                       },
                               };
            messageTemplates.ForEach(mt => _messageTemplateRepository.Insert(mt));

        }

        protected virtual void InstallTopics()
        {
            var topics = new List<Topic>
                               {
                                   new Topic
                                       {
                                           SystemName = "AboutUs",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "",
                                           Body = @"<div class=""about-us""><p style=""text-align: center;"">ДОБРО ПОЖАЛОВАТЬ В КНИЖНУЮ ЛАВКУ!</p>
<p>Книжная Лавка - уютное место, где можно найти и купить замечательные книги.</p>
<p></p><p>Мы - коллектив книголюбов и книгопочитателей, который занимается книготорговой деятельностью уже более двадцати лет.</p>
<p></p><p>Мы рады предложить вам самые разные книги, собранные для вас в одном месте. У нас прекрасный выбор книг, который вас обязательно порадует. В Книжной Лавке вы найдете нужные и полезные книги российских и украинских издательств, а также издательств других стран, сопутствующие товары, подарки, изделия ручной работы, развивающие и настольные игры, паззлы, и многое другое.</p>
<p>Хотя в нашей лавке вы найдете все возможные категории книг, но у нас есть и специализации. Вот в чем мы особенно хороши.</p>
<ul><li><p class=""bold"">Огромная коллекция скидочных книг.</p>
<p>Что такое скидочные книги? Это книги, которые значительно понизились в цене по ряду причин. Иногда, книга становится дешевле, когда залежится на наших полках, а новые книги поджимают и требуют места. Иногда, где-то закрывается книжный магазин, и его книги находят приют у нас - по самым привлекательным ценам. Иногда, издательство избавляется от остатков книг на своих складах, и у нас появляется возможность предложить его книги по просто волшебным ценам.</p>
</li><li><p class=""bold"">Широчайший ассортимент компьютерной, технической и бизнес-литературы.</p>
<p>Мы давно и плодотворно сотрудничаем с передовыми российскими и украинскими издательствами компьютерной, технической и бизнес-литературы. На складе Книжной Лавки всегда в наличии есть как новинки, так и давно искомые вами раритеты книг данной категории. Мы рады предложить вам максимально доступные цены.</p>
</li><li><p class=""bold"">Новинки популярной литературы.</p>
<p>Мы пристально следим за новинками литературы, и стараемся первыми представить вам все хиты книжного мира.</p>
</li><li><p class=""bold"">Интеллектуальная проза.</p>
<p>Книги, которые порой непросто найти, либо потому что спрос в магазинах на них не слишком высок, либо потому, что изданы они небольшими издательствами или маленькими тиражами. Порой, цены на такие книги высоки, но они стоят того.</p>
</li><li><p class=""bold"">Детская литература.</p>
<p>Мы напрямую работаем с прекрасными украинскими издательствами книжек для детей, на русском и украинском языке. В Книжной Лавке вы можете найти чудесные детские книги, развивающие материалы, картон, паззлы, раскраски, энциклопедии, книги по поделкам и увлечениям. И все это - по самым добрым ценам!</p>
</li></ul><p style=""text-align: center;"">ПОЧЕМУ КНИЖНАЯ ЛАВКА?</p>
<ul><li><p class=""bold"">У нас уютно.</p>
<p>Мы дружелюбны и открыты! Мы хотим, чтобы вы ушли от нас не только с покупкой, но и с улыбкой и хорошим настроением.</p>
</li><li><p class=""bold"">Индивидуальный подход.</p>
<p>Мы стараемся помочь вам найти именно то, что вам нужно. У нас можно оставлять заявки на заказ - если книги или другого товара вы не нашли в нашей онлайн-лавке, мы все равно попробуем найти его для вас. Мы гибки в доставке и оплате. Мы предоставляем много возможностей для оплаты и доставки товаров, более того, вы всегда можете предложить свой, более удобный вариант. Мы предоставляем срочную курьерскую доставку и доставку в послерабочее время.</p>
</li><li><p class=""bold"">Цены.</p>
<p>Мы прилагаем все усилия, чтобы наши цены были максимально приемлемы и позволяли вам читать еще больше!</p>
</li></ul><p class=""for-partner"">&#9660;&nbsp;&nbsp;Партнерам</p>
<ul><li><p>Для оптовых покупателей действует приятная система скидок.</p>
</li><li><p>Библиотеки, НГО, неприбыльные организации, благотворительные фонды и любые социальные организации, в особенности, работающие с детьми, приравниваются к оптовым покупателям. Мы всегда открыты работе с социальными проектами.</p>
</li><li><p>Мы рады дружить с интересными сайтами книжной направленности, сайтами издательств и проектов, направленных на популяризацию чтения - путем обмена ссылками и взаимной рекламы.</p>
</li></ul></div><div class=""about-us-footer"">Загрузка...</div>"
                                       },
                                   new Topic
                                       {
                                           SystemName = "CheckoutAsGuestOrRegister",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "",
                                           Body = "<p><strong>Register and save time!</strong><br />Register with us for future convenience:</p><ul><li>Fast and easy check out</li><li>Easy access to your order history and status</li></ul>"
                                       },
                                   new Topic
                                       {
                                           SystemName = "ConditionsOfUse",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "Conditions of use",
                                           Body = "<p>Put your conditions of use information here. You can edit this in the admin site.</p>"
                                       },
                                   new Topic
                                       {
                                           SystemName = "ContactUs",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "",
                                           Body = @"<table><tbody><tr><td><div class=""clock""></div></td><td colspan=""2"">
<p class=""bold"">Режим работы:</p><ul><li>- Прием заказов онлайн - круглосуточно</li><li>- Обработка, подтверждение и формирование заказов - 9.00 - 16.00</li><li>- Доставка по г. Харьков 16.00 - 20.00 (индивидуальная доставка: 8.00 - 20.00)</li><li>- Самовывоз по г. Харьков - 9.00 - 16.00</li></ul></td></tr>
<tr><td><div class=""email""></div></td><td colspan=""2""><p class=""bold"">E-mail:</p><ul><li>lavkabooks@gmail.com</li><li>info@lavkabooks.com</li></ul></td></tr>
<tr><td><div class=""skype""></div></td><td colspan=""2""><p class=""bold"">Skype:</p>Lavka Books</td></tr>
<tr><td><div class=""phone""></div></td><td colspan=""2""><p class=""bold"">Телефоны:</p><ul><li>+38 (063) *******</li><li>+38 (050) *******</li><li>+38 (097) *******</li></ul></td></tr>
<tr><td></td><td colspan=""2""><span class=""bold"">Офис-склад:</span> г. Харьков, пер. Кравцова, 19 (в здании Райского Уголка)</td></tr>
<tr><td></td><td colspan=""2"">Здесь можно забрать свой заказ (самовывоз).</td></tr><tr><td></td><td colspan=""2"">Здесь же осуществляется работа с оптовыми покупателями.</td></tr>
<tr><td></td><td colspan=""2""><div class=""store""></div></td></tr>
<tr><td></td><td colspan=""2""><p class=""bold"">Наши живые Книжные Лавки</p><br />В наших живых Книжных Лавках Вы можете полистать, выбрать и купить книги, а так же забрать свой онлайн-заказ. Обратите внимание: если Вам удобнее в одной из живых Книжных Лавок, то срок передачи товара со склада может составлять до 3-х дней.
<p style=""font-style: italic;"">Важно: цены в живых Книжных Лавках и интернет магазине могут отличаться.</p></td></tr>
<tr><td></td><td><p class=""bold"">""Книжная Лавка"" на Конном</p><br /><p>пл. Восстания, 5/6</p><br /><p>район Конного рынка,</p><p>напротив трамвайной остановки</p><br /><p>от метро ""Пл. Восстания"" - 3 мин.</p><p>от метро ""Спортивная"" - 10 мин.</p><br /><p>Тел. +38 (063) 858 78 70</p><br /><p>Режимы работы:</p><p>Понедельник - Пятница: 8.30 - 19.00</p><p>Суббота-Воскресенье: 8.30 - 16.00</p></td><td><div class=""store1""></div></td></tr>
<tr><td></td><td><p class=""bold"">""Книжная Лавка"" на Алексеевке</p><br /><p>пр. Людвига Свободы, 31</p><br /><p>выход из метро в сторону супермаркета ""Класс"" цокольный этаж</p><br /><p>от метро ""Алексеевская"" - 20 секунд</p><br /><p>Тел. +38 (093) 400 06 68</p><br /><p>Режимы работы:</p><p>Понедельник - Воскресенье: 9.00 - 19.00</p></td><td><div class=""store2""></div></td></tr>
<tr><td></td><td colspan=""2""><p class=""bold"" id=""contact-us-form"">Напишите нам!</p><br /> Мы с радостью ответим на любой Ваш вопрос. Для этого воспользуйтесь формой ниже:</td></tr></tbody></table>"
                                       },
                                   new Topic
                                       {
                                           SystemName = "ForumWelcomeMessage",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "Forums",
                                           Body = "<p>Put your welcome message here. You can edit this in the admin site.</p>"
                                       },   
                                   new Topic
                                       {
                                           SystemName = "PrivacyInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "Privacy policy",
                                           Body = "<p>Put your privacy policy information here. You can edit this in the admin site.</p>"
                                       },
                                   new Topic
                                       {
                                           SystemName = "ShippingInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "Shipping & Returns",
                                           Body = "<p>Put your shipping &amp; returns information here. You can edit this in the admin site.</p>"
                                       },
                                   new Topic
                                       {
                                           SystemName = "SippingPaymentInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "",
                                           Body = @"<div class=""shipping-info"" id=""shipping"">
<h4>Доставка</h4><br />Доставка по Украине<table><tbody>
<tr><td class=""self""></td><td>Самовывоз (г. Харьков)</td></tr>
<tr><td class=""courier""></td><td>Курьерская доставка (г. Харьков)</td></tr>
<tr><td class=""ukr-poshta""></td><td>Доставка Укрпочтой (до отделенеия связи и курьерская доставка до Ваших дверей)</td></tr>
<tr><td class=""nova-poshta""></td><td>Доставка Новой Почтой (до склада и курьерская доставка до Ваших дверей)</td></tr>
<tr><td class=""other""></td><td>Другие службы доставки</td></tr>
<tr><td class=""russia-sng""></td><td>Доставка в Россию, СНГ и дальнее зарубежье</td></tr>
<tr><td class=""international""></td><td>Доставка международными почтовыми компаниями</td></tr></tbody></table>
</div><div class=""paymant-info"" id=""payment""><h4>Оплата</h4><br />Оплата по Украине<table><tbody>
<tr><td class=""cash""></td><td>Оплата наличными в пункте самовывоза</td></tr>
<tr><td class=""cash""></td><td>Оплата наличными курьеру</td></tr>
<tr><td class=""privat-bank""></td><td>Перевод денег на карту ПриватБанка</td></tr>
<tr><td class=""privat24""></td><td>Перевод денег в системе Privat24</td></tr>
<tr><td class=""web-money""></td><td>Оплата электронными деньгами Webmoney</td></tr>
<tr><td class=""visa-mastercard""></td><td>Оплата кредитными картами Visa/Mastercard в системе LiqPay</td></tr>
<tr><td class=""transfer""></td><td>Банковский перевод</td></tr>
<tr><td class=""transfer""></td><td>Оплата через системы денежных переводов/электронных переводов</td></tr>
<tr><td class=""terminal""></td><td>Оплата через терминалы моментальной оплаты</td></tr>
</tbody></table><br />Оплата из любой точки мира<table><tbody>
<tr><td class=""visa-mastercard""></td><td>Оплата кредитными картами Visa/Mastercard в системе LiqPay</td></tr>
<tr><td class=""international-transfer""></td><td>Международные денежные переводы</td></tr>
<tr><td class=""privat-world""></td><td>PrivatMoney World</td></tr>
<tr><td class=""transfer""></td><td>Банковский перевод</td></tr>
</tbody></table></div>"
                                       },
                                   new Topic
                                       {
                                           SystemName = "RegistrationInfo",
                                           IncludeInSitemap = false,
                                           IsPasswordProtected = false,
                                           Title = "",
                                           Body = @"<p>Регистрация не является обязательной для совершения покупок в Книжной Лавке, но значительно облегчит процедуру заказа и предоставит значительные преимущества!</p>
<div id=""where-improvement""><h4>Хотите узнать какие?</h4><ul>
<li><p>Возможность принимать участие в акциях и получать приятные сюрпризы от Книжной Лавки: специальные скидки, подарки ко Дню Рождения и другие приятные бонусы</p></li>
<li><p>Возможность учавствовать в еженедельных бесплатных Раздачах Книг</p></li>
<li><p>Возможность быстрее и удобнее пользоваться лавкой: отслеживать статус заказа, просматривать историю покупок, добавлять контактные данные и данные об адресе доставки, и не вносить их заново каждый раз</p></li>
<li><p>Возможность оставлять отзывы о книгах, учавствовать в наших книжных мероприятиях и узнавать о литературных благотворительных проектах</p></li>
<li><p>Возможность узнавать о событиях мира книг и находиться в курсе интересных вам книжных новинок</p></li></ul></div>"
                                       },
                               };
            topics.ForEach(t => _topicRepository.Insert(t));

        }

        protected virtual void InstallSettings()
        {
            EngineContext.Current.Resolve<IConfigurationProvider<PdfSettings>>()
                .SaveSettings(new PdfSettings()
                {
                    Enabled = true,
                    LetterPageSizeEnabled = false,
                    RenderOrderNotes = true,
                    FontFileName = "FreeSerif.ttf",
                });

            EngineContext.Current.Resolve<IConfigurationProvider<CommonSettings>>()
                .SaveSettings(new CommonSettings()
                {
                    UseSystemEmailForContactUsForm = true,
                    UseStoredProceduresIfSupported = true,
                    SitemapEnabled = true,
                    SitemapIncludeCategories = true,
                    SitemapIncludeManufacturers = true,
                    SitemapIncludeProducts = false,
                    SitemapIncludeTopics = true,
                    DisplayJavaScriptDisabledWarning = false,
                    UseFullTextSearch = false,
                    FullTextMode = FulltextSearchMode.ExactMatch,
                    Log404Errors = true,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<SeoSettings>>()
                .SaveSettings(new SeoSettings()
                {
                    PageTitleSeparator = ". ",
                    PageTitleSeoAdjustment = PageTitleSeoAdjustment.PagenameAfterStorename,
                    DefaultTitle = "Lavka books",
                    DefaultMetaKeywords = "",
                    DefaultMetaDescription = "",
                    ConvertNonWesternChars = false,
                    AllowUnicodeCharsInUrls = true,
                    EnableJsBundling = true,
                    ReservedUrlRecordSlugs = new List<string>() { "admin", "install", "recentlyviewedproducts", "newproducts", "compareproducts", "clearcomparelist", "setproductreviewhelpfulness", "login", "register", "logout", "cart", "wishlist", "emailwishlist", "checkout", "onepagecheckout", "contactus", "passwordrecovery", "subscribenewsletter", "blog", "boards", "inboxupdate", "sentupdate", "news", "sitemap", "sitemapseo", "search", "config", "eucookielawaccept" },
                });

            EngineContext.Current.Resolve<IConfigurationProvider<AdminAreaSettings>>()
                .SaveSettings(new AdminAreaSettings()
                {
                    GridPageSize = 15,
                    DisplayProductPictures = true,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<CatalogSettings>>()
                .SaveSettings(new CatalogSettings()
                {
                    ShowProductSku = false,
                    ShowManufacturerPartNumber = false,
                    AllowProductSorting = true,
                    AllowProductViewModeChanging = false,
                    DefaultViewMode = "grid",
                    ShowProductsFromSubcategories = false,
                    ShowCategoryProductNumber = false,
                    ShowCategoryProductNumberIncludingSubcategories = false,
                    CategoryBreadcrumbEnabled = true,
                    ShowShareButton = true,
                    PageShareCode = "<!-- AddThis Button BEGIN --><div class=\"addthis_toolbox addthis_default_style \"><a class=\"addthis_button_preferred_1\"></a><a class=\"addthis_button_preferred_2\"></a><a class=\"addthis_button_preferred_3\"></a><a class=\"addthis_button_preferred_4\"></a><a class=\"addthis_button_compact\"></a><a class=\"addthis_counter addthis_bubble_style\"></a></div><script type=\"text/javascript\" src=\"http://s7.addthis.com/js/250/addthis_widget.js#pubid=nopsolutions\"></script><!-- AddThis Button END -->",
                    ProductReviewsMustBeApproved = false,
                    DefaultProductRatingValue = 5,
                    AllowAnonymousUsersToReviewProduct = false,
                    NotifyStoreOwnerAboutNewProductReviews = false,
                    EmailAFriendEnabled = true,
                    AllowAnonymousUsersToEmailAFriend = false,
                    RecentlyViewedProductsNumber = 4,
                    RecentlyViewedProductsEnabled = true,
                    RecentlyAddedProductsNumber = 6,
                    RecentlyAddedProductsEnabled = true,
                    CompareProductsEnabled = true,
                    ProductSearchAutoCompleteEnabled = true,
                    ProductSearchAutoCompleteNumberOfProducts = 10,
                    ProductSearchTermMinimumLength = 3,
                    ShowProductImagesInSearchAutoComplete = false,
                    ShowBestsellersOnHomepage = true,
                    NumberOfBestsellersOnHomepage = 6,
                    SearchPageProductsPerPage = 6,
                    ProductsAlsoPurchasedEnabled = true,
                    ProductsAlsoPurchasedNumber = 3,
                    EnableDynamicPriceUpdate = false,
                    NumberOfProductTags = 15,
                    ProductsByTagPageSize = 4,
                    IncludeShortDescriptionInCompareProducts = false,
                    IncludeFullDescriptionInCompareProducts = false,
                    UseSmallProductBoxOnHomePage = true,
                    IncludeFeaturedProductsInNormalLists = false,
                    DisplayTierPricesWithDiscounts = true,
                    IgnoreDiscounts = false,
                    IgnoreFeaturedProducts = false,
                    DefaultCategoryPageSizeOptions = "4, 2, 8, 12",
                    DefaultManufacturerPageSizeOptions = "4, 2, 8, 12",
                    ProductsByTagAllowCustomersToSelectPageSize = true,
                    ProductsByTagPageSizeOptions = "12, 24",
                    MaximumBackInStockSubscriptions = 200,
                    FileUploadMaximumSizeBytes = 1024 * 200, //200KB
                    ManufacturersBlockItemsToDisplay = 5,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<LocalizationSettings>>()
                .SaveSettings(new LocalizationSettings()
                {
                    DefaultAdminLanguageId = _languageRepository.Table.Where(l => l.Name == "Russian").Single().Id,
                    UseImagesForLanguageSelection = false,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<CustomerSettings>>()
                .SaveSettings(new CustomerSettings()
                {
                    UsernamesEnabled = false,
                    CheckUsernameAvailabilityEnabled = false,
                    AllowUsersToChangeUsernames = false,
                    DefaultPasswordFormat = PasswordFormat.Hashed,
                    HashedPasswordFormat = "SHA1",
                    PasswordMinLength = 6,
                    UserRegistrationType = UserRegistrationType.Standard,
                    AllowCustomersToUploadAvatars = false,
                    AvatarMaximumSizeBytes = 20000,
                    DefaultAvatarEnabled = true,
                    ShowCustomersLocation = false,
                    ShowCustomersJoinDate = false,
                    AllowViewingProfiles = false,
                    NotifyNewCustomerRegistration = false,
                    HideDownloadableProductsTab = false,
                    HideBackInStockSubscriptionsTab = false,
                    DownloadableProductsValidateUser = false,
                    CustomerNameFormat = CustomerNameFormat.ShowEmails,
                    GenderEnabled = false,
                    DateOfBirthEnabled = false,
                    CompanyEnabled = false,
                    StreetAddressEnabled = false,
                    StreetAddress2Enabled = false,
                    ZipPostalCodeEnabled = false,
                    CityEnabled = false,
                    CountryEnabled = false,
                    StateProvinceEnabled = false,
                    PhoneEnabled = false,
                    FaxEnabled = false,
                    NewsletterEnabled = true,
                    HideNewsletterBlock = true,
                    OnlineCustomerMinutes = 20,
                    StoreLastVisitedPage = true,
                    SuffixDeletedCustomers = false,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<AddressSettings>>()
                .SaveSettings(new AddressSettings()
                {
                    CompanyEnabled = false,
                    StreetAddressEnabled = true,
                    StreetAddressRequired = true,
                    StreetAddress2Enabled = true,
                    ZipPostalCodeEnabled = true,
                    ZipPostalCodeRequired = true,
                    CityEnabled = true,
                    CityRequired = true,
                    CountryEnabled = true,
                    StateProvinceEnabled = true,
                    PhoneEnabled = true,
                    PhoneRequired = true,
                    FaxEnabled = true,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<MediaSettings>>()
                .SaveSettings(new MediaSettings()
                {
                    AvatarPictureSize = 85,
                    ProductThumbPictureSize = 125,
                    ProductDetailsPictureSize = 300,
                    ProductThumbPictureSizeOnProductDetailsPage = 70,
                    ProductVariantPictureSize = 125,
                    CategoryThumbPictureSize = 125,
                    ManufacturerThumbPictureSize = 125,
                    CartThumbPictureSize = 80,
                    MiniCartThumbPictureSize = 47,
                    AutoCompleteSearchThumbPictureSize = 20,
                    MaximumImageSize = 1280,
                    DefaultPictureZoomEnabled = false,
                    DefaultImageQuality = 100,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<StoreInformationSettings>>()
                .SaveSettings(new StoreInformationSettings()
                {
                    StoreName = "Lavka books",
                    StoreUrl = "http://lavkabooks.com.ua/",
                    StoreClosed = false,
                    StoreClosedAllowForAdmins = false,
                    DefaultStoreThemeForDesktops = "LavkaBooks",
                    AllowCustomerToSelectTheme = false,
                    MobileDevicesSupported = false,
                    DefaultStoreThemeForMobileDevices = "Mobile",
                    EmulateMobileDevice = false,
                    DisplayMiniProfilerInPublicStore = false,
                    DisplayEuCookieLawWarning = false,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<RewardPointsSettings>>()
                .SaveSettings(new RewardPointsSettings()
                {
                    Enabled = false,
                    ExchangeRate = 1,
                    PointsForRegistration = 0,
                    PointsForPurchases_Amount = 10,
                    PointsForPurchases_Points = 1,
                    PointsForPurchases_Awarded = OrderStatus.Complete,
                    PointsForPurchases_Canceled = OrderStatus.Cancelled,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<CurrencySettings>>()
                .SaveSettings(new CurrencySettings()
                {
                    PrimaryStoreCurrencyId = _currencyRepository.Table.Where(c => c.CurrencyCode == "UAH").Single().Id,
                    PrimaryExchangeRateCurrencyId = _currencyRepository.Table.Where(c => c.CurrencyCode == "UAH").Single().Id,
                    ActiveExchangeRateProviderSystemName = "CurrencyExchange.MoneyConverter",
                    AutoUpdateEnabled = false,
                    LastUpdateTime = 0
                });

            EngineContext.Current.Resolve<IConfigurationProvider<MeasureSettings>>()
                .SaveSettings(new MeasureSettings()
                {
                    BaseDimensionId = _measureDimensionRepository.Table.Where(m => m.SystemKeyword == "cantimeters").Single().Id,
                    BaseWeightId = _measureWeightRepository.Table.Where(m => m.SystemKeyword == "grams").Single().Id,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<MessageTemplatesSettings>>()
                .SaveSettings(new MessageTemplatesSettings()
                {
                    CaseInvariantReplacement = false,
                    Color1 = "#b9babe",
                    Color2 = "#ebecee",
                    Color3 = "#dde2e6",
                });

            EngineContext.Current.Resolve<IConfigurationProvider<ShoppingCartSettings>>()
                .SaveSettings(new ShoppingCartSettings()
                {
                    DisplayCartAfterAddingProduct = false,
                    DisplayWishlistAfterAddingProduct = false,
                    MaximumShoppingCartItems = 1000,
                    MaximumWishlistItems = 1000,
                    AllowOutOfStockItemsToBeAddedToWishlist = false,
                    MoveItemsFromWishlistToCart = false,
                    ShowProductImagesOnShoppingCart = true,
                    ShowProductImagesOnWishList = false,
                    ShowDiscountBox = true,
                    ShowGiftCardBox = true,
                    CrossSellsNumber = 2,
                    EmailWishlistEnabled = false,
                    AllowAnonymousUsersToEmailWishlist = false,
                    MiniShoppingCartEnabled = true,
                    ShowProductImagesInMiniShoppingCart = true,
                    MiniShoppingCartProductNumber = 5,
                    RoundPricesDuringCalculation = true,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<OrderSettings>>()
                .SaveSettings(new OrderSettings()
                {
                    IsReOrderAllowed = true,
                    MinOrderSubtotalAmount = 0,
                    MinOrderTotalAmount = 0,
                    AnonymousCheckoutAllowed = true,
                    TermsOfServiceEnabled = false,
                    OnePageCheckoutEnabled = true,
                    ReturnRequestsEnabled = true,
                    ReturnRequestActions = new List<string>() { "Repair", "Replacement", "Store Credit" },
                    ReturnRequestReasons = new List<string>() { "Received Wrong Product", "Wrong Product Ordered", "There Was A Problem With The Product" },
                    NumberOfDaysReturnRequestAvailable = 365,
                    MinimumOrderPlacementInterval = 30,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<SecuritySettings>>()
                .SaveSettings(new SecuritySettings()
                {
                    ForceSslForAllPages = false,
                    EncryptionKey = "273ece6f97dd844d",
                    AdminAreaAllowedIpAddresses = null
                });

            EngineContext.Current.Resolve<IConfigurationProvider<ShippingSettings>>()
                .SaveSettings(new ShippingSettings()
                {
                    ActiveShippingRateComputationMethodSystemNames = new List<string>() { "Shipping.FixedRate" },
                    FreeShippingOverXEnabled = false,
                    FreeShippingOverXValue = decimal.Zero,
                    FreeShippingOverXIncludingTax = false,
                    EstimateShippingEnabled = true,
                    DisplayShipmentEventsToCustomers = false,
                    ReturnValidOptionsIfThereAreAny = true,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<PaymentSettings>>()
                .SaveSettings(new PaymentSettings()
                {
                    ActivePaymentMethodSystemNames = new List<string>() 
                    { 
                        "Payments.CashOnDelivery",
                        "Payments.CheckMoneyOrder",
                        "Payments.Manual",
                        "Payments.PayInStore",
                        "Payments.PurchaseOrder",
                    },
                    AllowRePostingPayments = true,
                    BypassPaymentMethodSelectionIfOnlyOne = true,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<TaxSettings>>()
                .SaveSettings(new TaxSettings()
                {
                    TaxBasedOn = TaxBasedOn.BillingAddress,
                    TaxDisplayType = TaxDisplayType.ExcludingTax,
                    ActiveTaxProviderSystemName = "Tax.FixedRate",
                    DefaultTaxAddressId = 0,
                    DisplayTaxSuffix = false,
                    DisplayTaxRates = false,
                    PricesIncludeTax = false,
                    AllowCustomersToSelectTaxDisplayType = false,
                    HideZeroTax = false,
                    HideTaxInOrderSummary = false,
                    ShippingIsTaxable = false,
                    ShippingPriceIncludesTax = false,
                    ShippingTaxClassId = 0,
                    PaymentMethodAdditionalFeeIsTaxable = false,
                    PaymentMethodAdditionalFeeIncludesTax = false,
                    PaymentMethodAdditionalFeeTaxClassId = 0,
                    EuVatEnabled = false,
                    EuVatShopCountryId = 0,
                    EuVatAllowVatExemption = true,
                    EuVatUseWebService = false,
                    EuVatEmailAdminWhenNewVatSubmitted = false
                });

            EngineContext.Current.Resolve<IConfigurationProvider<FileSystemSettings>>()
                .SaveSettings(new FileSystemSettings()
                {
                });

            EngineContext.Current.Resolve<IConfigurationProvider<DateTimeSettings>>()
                .SaveSettings(new DateTimeSettings()
                {
                    DefaultStoreTimeZoneId = "",
                    AllowCustomersToSetTimeZone = false
                });

            EngineContext.Current.Resolve<IConfigurationProvider<BlogSettings>>()
                .SaveSettings(new BlogSettings()
                {
                    Enabled = true,
                    PostsPageSize = 10,
                    AllowNotRegisteredUsersToLeaveComments = true,
                    NotifyAboutNewBlogComments = false,
                    NumberOfTags = 15,
                    ShowHeaderRssUrl = false,
                    MainPagePostCount = 3
                });
            EngineContext.Current.Resolve<IConfigurationProvider<NewsSettings>>()
                .SaveSettings(new NewsSettings()
                {
                    Enabled = true,
                    AllowNotRegisteredUsersToLeaveComments = true,
                    NotifyAboutNewNewsComments = false,
                    ShowNewsOnMainPage = true,
                    MainPageNewsCount = 3,
                    NewsArchivePageSize = 10,
                    ShowHeaderRssUrl = false,
                });
            EngineContext.Current.Resolve<IConfigurationProvider<ForumSettings>>()
                .SaveSettings(new ForumSettings()
                {
                    ForumsEnabled = false,
                    RelativeDateTimeFormattingEnabled = true,
                    AllowCustomersToDeletePosts = false,
                    AllowCustomersToEditPosts = false,
                    AllowCustomersToManageSubscriptions = false,
                    AllowGuestsToCreatePosts = false,
                    AllowGuestsToCreateTopics = false,
                    TopicSubjectMaxLength = 450,
                    PostMaxLength = 4000,
                    StrippedTopicMaxLength = 45,
                    TopicsPageSize = 10,
                    PostsPageSize = 10,
                    SearchResultsPageSize = 10,
                    LatestCustomerPostsPageSize = 10,
                    ShowCustomersPostCount = true,
                    ForumEditor = EditorType.BBCodeEditor,
                    SignaturesEnabled = true,
                    AllowPrivateMessages = false,
                    ShowAlertForPM = false,
                    PrivateMessagesPageSize = 10,
                    ForumSubscriptionsPageSize = 10,
                    NotifyAboutPrivateMessages = false,
                    PMSubjectMaxLength = 450,
                    PMTextMaxLength = 4000,
                    HomePageActiveDiscussionsTopicCount = 5,
                    ActiveDiscussionsPageTopicCount = 50,
                    ActiveDiscussionsFeedEnabled = false,
                    ActiveDiscussionsFeedCount = 25,
                    ForumFeedsEnabled = false,
                    ForumFeedCount = 10,
                    ForumSearchTermMinimumLength = 3,
                });

            EngineContext.Current.Resolve<IConfigurationProvider<EmailAccountSettings>>()
                .SaveSettings(new EmailAccountSettings()
                {
                    DefaultEmailAccountId = _emailAccountRepository.Table.FirstOrDefault().Id
                });
        }

        protected virtual void InstallSpecificationAttributes()
        {
            var specificationAttributes = new List<SpecificationAttribute>
                {
                    new SpecificationAttribute
                        {
                            Name = "Author",
                            DisplayOrder = 1,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Year",
                            DisplayOrder = 2,
                        },
                    new SpecificationAttribute
                        {
                            Name = "ISBN",
                            DisplayOrder = 3,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Language",
                            DisplayOrder = 4,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Publisher",
                            DisplayOrder = 5,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Series",
                            DisplayOrder = 6,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Pages",
                            DisplayOrder = 7,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Format",
                            DisplayOrder = 8,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Weight",
                            DisplayOrder = 9,
                        },
                    new SpecificationAttribute
                        {
                            Name = "Cover",
                            DisplayOrder = 10,
                        },
                };
            specificationAttributes.ForEach(sa => _specificationAttributeRepository.Insert(sa));
        }

        protected virtual void InstallProductAttributes()
        {

        }

        protected virtual void InstallCategories()
        {
            //pictures
            var pictureService = EngineContext.Current.Resolve<IPictureService>();
            var sampleImagesPath = _webHelper.MapPath("~/content/samples/");

            var categoryTemplateInGridAndLines =
                _categoryTemplateRepository.Table.Where(pt => pt.Name == "Products in Grid or Lines").FirstOrDefault();

            //categories
            var allCategories = new List<Category>();

            var cBooks = new Category
            {
                Name = "Художествнные",
                CategoryTemplateId = categoryTemplateInGridAndLines.Id,
                MetaKeywords = "Books, Dictionary, Textbooks",
                MetaDescription = "Books category description",
                PageSize = 4,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "12, 24",
                PictureId = pictureService.InsertPicture(File.ReadAllBytes(sampleImagesPath + "category_book.jpeg"), "image/jpeg", pictureService.GetPictureSeName("Book"), true).Id,
                PriceRanges = "",
                Published = true,
                DisplayOrder = 2,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
            };
            allCategories.Add(cBooks);
            _categoryRepository.Insert(cBooks);

            //search engine names
            foreach (var category in allCategories)
            {
                _urlRecordRepository.Insert(new UrlRecord()
                {
                    EntityId = category.Id,
                    EntityName = "Category",
                    LanguageId = 0,
                    Slug = category.ValidateSeName("", category.Name, true)
                });
            }
        }

        protected virtual void InstallManufacturers()
        {
        }

        protected virtual void InstallProducts()
        {
        }

        protected virtual void InstallForums()
        {
        }

        protected virtual void InstallDiscounts()
        {
        }

        protected virtual void InstallBlogPosts()
        {
        }

        protected virtual void InstallNews()
        {
        }

        protected virtual void InstallPolls()
        {
        }

        protected virtual void InstallActivityLogTypes()
        {
            var activityLogTypes = new List<ActivityLogType>()
                                      {
                                          //admin area activities
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCategory",
                                                  Enabled = true,
                                                  Name = "Add a new category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCustomer",
                                                  Enabled = true,
                                                  Name = "Add a new customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewCustomerRole",
                                                  Enabled = true,
                                                  Name = "Add a new customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewDiscount",
                                                  Enabled = true,
                                                  Name = "Add a new discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewGiftCard",
                                                  Enabled = true,
                                                  Name = "Add a new gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewManufacturer",
                                                  Enabled = true,
                                                  Name = "Add a new manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProduct",
                                                  Enabled = true,
                                                  Name = "Add a new product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProductAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewProductVariant",
                                                  Enabled = true,
                                                  Name = "Add a new product variant"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewSetting",
                                                  Enabled = true,
                                                  Name = "Add a new setting"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Add a new specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "AddNewWidget",
                                                  Enabled = true,
                                                  Name = "Add a new widget"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCategory",
                                                  Enabled = true,
                                                  Name = "Delete category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCustomer",
                                                  Enabled = true,
                                                  Name = "Delete a customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteCustomerRole",
                                                  Enabled = true,
                                                  Name = "Delete a customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteDiscount",
                                                  Enabled = true,
                                                  Name = "Delete a discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteGiftCard",
                                                  Enabled = true,
                                                  Name = "Delete a gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteManufacturer",
                                                  Enabled = true,
                                                  Name = "Delete a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProduct",
                                                  Enabled = true,
                                                  Name = "Delete a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProductAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteProductVariant",
                                                  Enabled = true,
                                                  Name = "Delete a product variant"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteReturnRequest",
                                                  Enabled = true,
                                                  Name = "Delete a return request"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteSetting",
                                                  Enabled = true,
                                                  Name = "Delete a setting"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Delete a specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "DeleteWidget",
                                                  Enabled = true,
                                                  Name = "Delete a widget"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCategory",
                                                  Enabled = true,
                                                  Name = "Edit category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCheckoutAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a checkout attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCustomer",
                                                  Enabled = true,
                                                  Name = "Edit a customer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditCustomerRole",
                                                  Enabled = true,
                                                  Name = "Edit a customer role"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditDiscount",
                                                  Enabled = true,
                                                  Name = "Edit a discount"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditGiftCard",
                                                  Enabled = true,
                                                  Name = "Edit a gift card"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditManufacturer",
                                                  Enabled = true,
                                                  Name = "Edit a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProduct",
                                                  Enabled = true,
                                                  Name = "Edit a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProductAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a product attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditProductVariant",
                                                  Enabled = true,
                                                  Name = "Edit a product variant"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditPromotionProviders",
                                                  Enabled = true,
                                                  Name = "Edit promotion providers"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditReturnRequest",
                                                  Enabled = true,
                                                  Name = "Edit a return request"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditSettings",
                                                  Enabled = true,
                                                  Name = "Edit setting(s)"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditSpecAttribute",
                                                  Enabled = true,
                                                  Name = "Edit a specification attribute"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "EditWidget",
                                                  Enabled = true,
                                                  Name = "Edit a widget"
                                              },
                                              //public store activities
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewCategory",
                                                  Enabled = false,
                                                  Name = "Public store. View a category"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewManufacturer",
                                                  Enabled = false,
                                                  Name = "Public store. View a manufacturer"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ViewProduct",
                                                  Enabled = false,
                                                  Name = "Public store. View a product"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.PlaceOrder",
                                                  Enabled = false,
                                                  Name = "Public store. Place an order"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.SendPM",
                                                  Enabled = false,
                                                  Name = "Public store. Send PM"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.ContactUs",
                                                  Enabled = false,
                                                  Name = "Public store. Use contact us form"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToCompareList",
                                                  Enabled = false,
                                                  Name = "Public store. Add to compare list"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToShoppingCart",
                                                  Enabled = false,
                                                  Name = "Public store. Add to shopping cart"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddToWishlist",
                                                  Enabled = false,
                                                  Name = "Public store. Add to wishlist"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.Login",
                                                  Enabled = false,
                                                  Name = "Public store. Login"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.Logout",
                                                  Enabled = false,
                                                  Name = "Public store. Logout"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddProductReview",
                                                  Enabled = false,
                                                  Name = "Public store. Add product review"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddNewsComment",
                                                  Enabled = false,
                                                  Name = "Public store. Add news comment"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddBlogComment",
                                                  Enabled = false,
                                                  Name = "Public store. Add blog comment"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Add forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.EditForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Edit forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.DeleteForumTopic",
                                                  Enabled = false,
                                                  Name = "Public store. Delete forum topic"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.AddForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Add forum post"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.EditForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Edit forum post"
                                              },
                                          new ActivityLogType
                                              {
                                                  SystemKeyword = "PublicStore.DeleteForumPost",
                                                  Enabled = false,
                                                  Name = "Public store. Delete forum post"
                                              },
                                      };
            activityLogTypes.ForEach(alt => _activityLogTypeRepository.Insert(alt));
        }

        protected virtual void InstallProductTemplates()
        {
            var productTemplates = new List<ProductTemplate>
                               {
                                   new ProductTemplate
                                       {
                                           Name = "Lavka Variant",
                                           ViewPath = "ProductTemplate.LavkaVariant",
                                           DisplayOrder = 1
                                       },
                               };
            productTemplates.ForEach(pt => _productTemplateRepository.Insert(pt));

        }

        protected virtual void InstallCategoryTemplates()
        {
            var categoryTemplates = new List<CategoryTemplate>
                               {
                                   new CategoryTemplate
                                       {
                                           Name = "Products in Grid or Lines",
                                           ViewPath = "CategoryTemplate.ProductsInGridOrLines",
                                           DisplayOrder = 1
                                       },
                               };
            categoryTemplates.ForEach(ct => _categoryTemplateRepository.Insert(ct));

        }

        protected virtual void InstallManufacturerTemplates()
        {
            var manufacturerTemplates = new List<ManufacturerTemplate>
                               {
                                   new ManufacturerTemplate
                                       {
                                           Name = "Products in Grid or Lines",
                                           ViewPath = "ManufacturerTemplate.ProductsInGridOrLines",
                                           DisplayOrder = 1
                                       },
                               };
            manufacturerTemplates.ForEach(mt => _manufacturerTemplateRepository.Insert(mt));

        }

        protected virtual void InstallScheduleTasks()
        {
            var tasks = new List<ScheduleTask>()
            {
                new ScheduleTask()
                {
                    Name = "Send emails",
                    Seconds = 60,
                    Type = "Nop.Services.Messages.QueuedMessagesSendTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Keep alive",
                    Seconds = 300,
                    Type = "Nop.Services.Common.KeepAliveTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Delete guests",
                    Seconds = 600,
                    Type = "Nop.Services.Customers.DeleteGuestsTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Clear cache",
                    Seconds = 600,
                    Type = "Nop.Services.Caching.ClearCacheTask, Nop.Services",
                    Enabled = false,
                    StopOnError = false,
                },
                new ScheduleTask()
                {
                    Name = "Update currency exchange rates",
                    Seconds = 900,
                    Type = "Nop.Services.Directory.UpdateExchangeRateTask, Nop.Services",
                    Enabled = true,
                    StopOnError = false,
                },
            };

            tasks.ForEach(x => _scheduleTaskRepository.Insert(x));
        }

        #endregion

        #region Methods

        public virtual void InstallData(string defaultUserEmail,
            string defaultUserPassword, bool installSampleData = true)
        {
            InstallMeasures();
            InstallTaxCategories();
            InstallLanguages();
            InstallCurrencies();
            InstallCountriesAndStates();
            InstallShippingMethods();
            InstallCustomersAndUsers(defaultUserEmail, defaultUserPassword);
            InstallEmailAccounts();
            InstallMessageTemplates();
            InstallTopics();
            InstallSettings();
            InstallLocaleResources();
            InstallActivityLogTypes();
            HashDefaultCustomerPassword(defaultUserEmail, defaultUserPassword);
            InstallProductTemplates();
            InstallCategoryTemplates();
            InstallManufacturerTemplates();
            InstallScheduleTasks();

            if (installSampleData)
            {
                InstallSpecificationAttributes();
                InstallProductAttributes();
                InstallCategories();
                InstallManufacturers();
                InstallProducts();
                InstallForums();
                InstallDiscounts();
                InstallBlogPosts();
                InstallNews();
                InstallPolls();
            }
        }

        #endregion
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Self;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Core.Rss;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Helpers;
using Nop.Services.Html;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Self;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Self;

namespace Nop.Web.Controllers
{
    [AutoValidateAntiforgeryToken]
    public partial class ProductController : BasePublicController
    {
        #region Fields

        private readonly CaptchaSettings _captchaSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly IAclService _aclService;
        private readonly ICompareProductsService _compareProductsService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IHtmlFormatter _htmlFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductModelFactory _productModelFactory;
        private readonly IProductService _productService;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IReviewTypeService _reviewTypeService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly IAppointmentService _appointmentService;
        private readonly IAppointmentModelFactory _appointmentModelFactory;
        private readonly IDateTimeHelper _dateTimeHelper;

        #endregion

        #region Ctor

        public ProductController(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings,
            IAclService aclService,
            ICompareProductsService compareProductsService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            IHtmlFormatter htmlFormatter,
            ILocalizationService localizationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IProductAttributeParser productAttributeParser,
            IProductModelFactory productModelFactory,
            IProductService productService,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IReviewTypeService reviewTypeService,
            IShoppingCartModelFactory shoppingCartModelFactory,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            ShoppingCartSettings shoppingCartSettings,
            IAppointmentService appointmentService,
            IAppointmentModelFactory appointmentModelFactory,
            IDateTimeHelper dateTimeHelper,
            ShippingSettings shippingSettings)
        {
            _captchaSettings = captchaSettings;
            _catalogSettings = catalogSettings;
            _aclService = aclService;
            _compareProductsService = compareProductsService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _htmlFormatter = htmlFormatter;
            _localizationService = localizationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _productAttributeParser = productAttributeParser;
            _productModelFactory = productModelFactory;
            _productService = productService;
            _reviewTypeService = reviewTypeService;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _shippingSettings = shippingSettings;
            _appointmentService = appointmentService;
            _appointmentModelFactory = appointmentModelFactory;
            _dateTimeHelper = dateTimeHelper;
        }

        #endregion

        #region Utilities

        protected virtual async Task ValidateProductReviewAvailabilityAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.OnlyRegisteredUsersCanWriteReviews"));

            if (!_catalogSettings.ProductReviewPossibleOnlyAfterPurchasing)
                return;

            var hasCompletedOrders = product.ProductType == ProductType.SimpleProduct
                ? await HasCompletedOrdersAsync(product)
                : await (await _productService.GetAssociatedProductsAsync(product.Id)).AnyAwaitAsync(HasCompletedOrdersAsync);

            if (!hasCompletedOrders)
                ModelState.AddModelError(string.Empty, await _localizationService.GetResourceAsync("Reviews.ProductReviewPossibleOnlyAfterPurchasing"));
        }

        protected virtual async ValueTask<bool> HasCompletedOrdersAsync(Product product)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            return (await _orderService.SearchOrdersAsync(customerId: customer.Id,
                productId: product.Id,
                osIds: new List<int> { (int)OrderStatus.Complete },
                pageSize: 1)).Any();
        }

        #endregion

        #region Appointment Methods

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> AppointmentSlotsByCustomer(DateTime start, DateTime end, int resourceId)
        {
            var currentCustomer = _workContext.GetCurrentCustomerAsync();
            var events = await _appointmentService.GetAvailableAppointmentsByCustomerAsync(start, end, resourceId, currentCustomer.Id);

            var model = new List<AppointmentInfoModel>();
            foreach (var appointment in events)
            {
                var item = await _appointmentModelFactory.PrepareAppointmentInfoModel(appointment);
                model.Add(item);
            }

            return Json(model);
        }

        public virtual async Task<IActionResult> AppointmentUpdate(int id)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            //if (currentCustomer.IsGuest())
            //{
            //    string statusText = _localizationService.GetResource("Product.AppointmentUpdate.LoginRequired");
            //    return Json(new { status = false, message = statusText, data = 0 });
            //}

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment != null && (!appointment.CustomerId.HasValue || appointment.CustomerId == currentCustomer.Id))
            {
                //prepare model
                var model = _appointmentModelFactory.PrepareAppointmentUpdateModel(appointment);
                return Json(new { status = true, data = model });
            }
            else
            {
                string statusText = await _localizationService.GetResourceAsync("Product.AppointmentUpdate.SlotNotExist");
                return Json(new { status = false, message = statusText });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> AppointmentRequest(int id, string notes)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            //if (_workContext.CurrentCustomer.IsGuest())
            //{
            //    string statusText = _localizationService.GetResource("Product.AppointmentUpdate.LoginRequired");
            //    return Json(new { status = false, message = statusText, data = 0 });
            //}

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            // TODO: Check business logic by Product
            // Check if CurrentCustomer is a member of Vendor 
            if (appointment != null && appointment.Status == AppointmentStatusType.Free)
            {
                appointment.CustomerId = currentCustomer.Id;
                appointment.Status = AppointmentStatusType.Waiting;
                appointment.Notes = notes;
                await _appointmentService.UpdateAppointmentAsync(appointment);

                var model = _appointmentModelFactory.PrepareAppointmentUpdateModel(appointment);

                return Json(new { status = true });
            }
            else
            {
                string statusText = await _localizationService.GetResourceAsync("Product.AppointmentRequest.Failed");
                return Json(new { status = false, message = statusText });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> AppointmentCancel(int id)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            //if (_workContext.CurrentCustomer.IsGuest())
            //{
            //    string statusText = _localizationService.GetResource("Product.AppointmentUpdate.LoginRequired");
            //    return Json(new { status = false, message = statusText, data = 0 });
            //}

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment != null && appointment.CustomerId == currentCustomer.Id && appointment.Status == AppointmentStatusType.Waiting)
            {
                appointment.Status = AppointmentStatusType.Free;
                appointment.CustomerId = null;
                appointment.Notes = "";
                await _appointmentService.UpdateAppointmentAsync(appointment);

                var model = _appointmentModelFactory.PrepareAppointmentUpdateModel(appointment);

                return Json(new { status = true });
            }
            else
            {
                string statusText = await _localizationService.GetResourceAsync("Product.AppointmentCancel.Failed");
                return Json(new { status = false, message = statusText });
            }
        }

        #endregion Appointment Methods

        #region Grouped Products Appointments

        public virtual async Task<IActionResult> GetResourcesByParent(int parentProductId)
        {
            var model = await _appointmentModelFactory.PrepareVendorResourcesModelAsync(parentProductId);
            return Json(model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> GetAppointmentsByParent(int parentProductId, DateTime start, DateTime end)
        {
            var events = await _appointmentService.GetAppointmentsByParentAsync(parentProductId, start, end);

            var model = new List<VendorAppointmentInfoModel>();
            foreach (var appointment in events)
            {
                var item = await _appointmentModelFactory.PrepareVendorAppointmentInfoModel(appointment);
                model.Add(item);
                item.backColor = "#E69138";
                item.bubbleHtml = "Not available";
                item.moveDisabled = true;
                item.resizeDisabled = true;
                item.clickDisabled = true;
                // TODO: remove customer name for non-admin user ?
                var customer = await _customerService.GetCustomerByIdAsync(appointment.CustomerId.Value);
                if (customer != null)
                {
                    item.text = customer.Username ?? customer.Email;
                };
            }

            return Json(model);
        }

        public virtual async Task<IActionResult> RequestVendorAppointment(int parentProductId, int resourceId, DateTime start, DateTime end)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            //if (_workContext.CurrentCustomer.IsGuest())
            //{
            //    string statusText = _localizationService.GetResource("GroupedProduct.RequestVendorAppointment.LoginRequired");
            //    return Json(new { status = false, message = statusText, data = 0 });
            //}

            var vendorResources = await _appointmentModelFactory.PrepareVendorResourcesModelAsync(parentProductId);
            var vendorResource = vendorResources.FirstOrDefault(o => o.id == resourceId.ToString());

            VendorAppointmentInfoModel model = new VendorAppointmentInfoModel();
            model.parentProductId = parentProductId.ToString();
            model.resource = resourceId.ToString();
            model.resourceName = vendorResource != null ? vendorResource.name : resourceId.ToString();
            model.timeRange = $"{start.ToShortTimeString()} - {end.ToShortTimeString()}, {start.ToShortDateString()} {start.ToString("dddd")}";
            model.start = start.ToString("yyyy-MM-ddTHH:mm:ss");
            model.end = end.ToString("yyyy-MM-ddTHH:mm:ss"); ;

            return Json(new { status = true, data = model });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public virtual async Task<IActionResult> SaveVendorAppointment(int parentProductId, int resourceId, DateTime start, DateTime end)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            //if (_workContext.CurrentCustomer.IsGuest())
            //{
            //    string statusText = _localizationService.GetResource("GroupedProduct.RequestVendorAppointment.LoginRequired");
            //    return Json(new { status = false, message = statusText });
            //}

            // TODO: Get Product by parentProductId
            // Check business logic by Product
            // Check if CurrentCustomer is a member of Vendor 

            try
            {
                // Check if the requested time slot is taken already 
                if (!_appointmentService.IsTaken(resourceId, start, end))
                {
                    var startTimeUtc = _dateTimeHelper.ConvertToUtcTime(start);
                    var endTimeUtc = _dateTimeHelper.ConvertToUtcTime(end);

                    Appointment appointment = new Appointment
                    {
                        StartTimeUtc = startTimeUtc,
                        EndTimeUtc = endTimeUtc,
                        ResourceId = resourceId,
                        StatusId = (int)AppointmentStatusType.Confirmed,
                        CustomerId = currentCustomer.Id,
                        ParentProductId = parentProductId
                    };
                    await _appointmentService.InsertAppointmentAsync(appointment);
                    return Json(new { status = true });
                }
                else
                {
                    // Time slot is taken, show error message
                    string statusText = await _localizationService.GetResourceAsync("GroupedProduct.VendorAppointment.TimeTaken");
                    return Json(new { status = false, message = statusText });
                }
            }
            catch (Exception ex)
            {
                string statusText = $"{await _localizationService.GetResourceAsync("GroupedProduct.VendorAppointment.Failed")}: {ex.Message}";
                return Json(new { status = false, message = statusText });
            }
        }

        #endregion Grouped Products Appointments

        #region Product details page

        public virtual async Task<IActionResult> ProductDetails(int productId, int updatecartitemid = 0)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted)
                return InvokeHttp404();

            var notAvailable =
                //published?
                (!product.Published && !_catalogSettings.AllowViewUnpublishedProductPage) ||
                //ACL (access control list) 
                !await _aclService.AuthorizeAsync(product) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(product) ||
                //availability dates
                !_productService.ProductIsAvailable(product);
            //Check whether the current user has a "Manage products" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts);
            if (notAvailable && !hasAdminAccess)
                return InvokeHttp404();

            //visible individually?
            if (!product.VisibleIndividually)
            {
                //is this one an associated products?
                var parentGroupedProduct = await _productService.GetProductByIdAsync(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return RedirectToRoute("Homepage");

                return RedirectToRoutePermanent("Product", new { SeName = await _urlRecordService.GetSeNameAsync(parentGroupedProduct) });
            }

            //update existing shopping cart or wishlist  item?
            ShoppingCartItem updatecartitem = null;
            if (_shoppingCartSettings.AllowCartItemEditing && updatecartitemid > 0)
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), storeId: store.Id);
                updatecartitem = cart.FirstOrDefault(x => x.Id == updatecartitemid);
                //not found?
                if (updatecartitem == null)
                {
                    return RedirectToRoute("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) });
                }
                //is it this product?
                if (product.Id != updatecartitem.ProductId)
                {
                    return RedirectToRoute("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) });
                }
            }

            //save as recently viewed
            await _recentlyViewedProductsService.AddProductToRecentlyViewedListAsync(product.Id);

            //display "edit" (manage) link
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) &&
                await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            {
                //a vendor should have access only to his products
                var currentVendor = await _workContext.GetCurrentVendorAsync();
                if (currentVendor == null || currentVendor.Id == product.VendorId)
                {
                    DisplayEditLink(Url.Action("Edit", "Product", new { id = product.Id, area = AreaNames.Admin }));
                }
            }

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.ViewProduct",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.ViewProduct"), product.Name), product);

            //model
            var model = await _productModelFactory.PrepareProductDetailsModelAsync(product, updatecartitem, false);
            //template
            var productTemplateViewPath = await _productModelFactory.PrepareProductTemplateViewPathAsync(product);

            return View(productTemplateViewPath, model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EstimateShipping([FromQuery] ProductDetailsModel.ProductEstimateShippingModel model, IFormCollection form)
        {
            if (model == null)
                model = new ProductDetailsModel.ProductEstimateShippingModel();

            var errors = new List<string>();
            
            if (!_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.ZipPostalCode))
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.ZipPostalCode.Required"));

            if (_shippingSettings.EstimateShippingCityNameEnabled && string.IsNullOrEmpty(model.City))
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.City.Required"));

            if (model.CountryId == null || model.CountryId == 0)
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShipping.Country.Required"));

            if (errors.Count > 0)
                return Json(new
                {
                    Success = false,
                    Errors = errors
                });

            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted)
            {
                errors.Add(await _localizationService.GetResourceAsync("Shipping.EstimateShippingPopUp.Product.IsNotFound"));
                return Json(new
                {
                    Success = false,
                    Errors = errors
                });
            }
            
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            var wrappedProduct = new ShoppingCartItem()
            {
                StoreId = store.Id,
                ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                CustomerId = customer.Id,
                ProductId = product.Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            var addToCartWarnings = new List<string>();
            //customer entered price
            wrappedProduct.CustomerEnteredPrice = await _productAttributeParser.ParseCustomerEnteredPriceAsync(product, form);

            //entered quantity
            wrappedProduct.Quantity = _productAttributeParser.ParseEnteredQuantity(product, form);

            //product and gift card attributes
            wrappedProduct.AttributesXml = await _productAttributeParser.ParseProductAttributesAsync(product, form, addToCartWarnings);

            //rental attributes
            _productAttributeParser.ParseRentalDates(product, form, out var rentalStartDate, out var rentalEndDate);
            wrappedProduct.RentalStartDateUtc = rentalStartDate;
            wrappedProduct.RentalEndDateUtc = rentalEndDate;

            var result = await _shoppingCartModelFactory.PrepareEstimateShippingResultModelAsync(new[] { wrappedProduct }, model, false);

            return Json(result);
        }

        //ignore SEO friendly URLs checks
        [CheckLanguageSeoCode(true)]
        public virtual async Task<IActionResult> GetProductCombinations(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return NotFound();

            var model = await _productModelFactory.PrepareProductCombinationModelsAsync(product);
            return Ok(model);
        }

        #endregion

        #region Recently viewed products

        public virtual async Task<IActionResult> RecentlyViewedProducts()
        {
            if (!_catalogSettings.RecentlyViewedProductsEnabled)
                return Content("");

            var products = await _recentlyViewedProductsService.GetRecentlyViewedProductsAsync(_catalogSettings.RecentlyViewedProductsNumber);

            var model = new List<ProductOverviewModel>();
            model.AddRange(await _productModelFactory.PrepareProductOverviewModelsAsync(products));

            return View(model);
        }

        #endregion

        #region New (recently added) products page

        public virtual async Task<IActionResult> NewProducts()
        {
            if (!_catalogSettings.NewProductsEnabled)
                return Content("");

            var store = await _storeContext.GetCurrentStoreAsync();
            var storeId = store.Id;
            var products = await _productService.GetProductsMarkedAsNewAsync(storeId);
            var model = (await _productModelFactory.PrepareProductOverviewModelsAsync(products)).ToList();

            return View(model);
        }

        [CheckLanguageSeoCode(true)]
        public virtual async Task<IActionResult> NewProductsRss()
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(store, x => x.Name)}: New products",
                "Information about products",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            if (!_catalogSettings.NewProductsEnabled)
                return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));

            var items = new List<RssItem>();

            var storeId = store.Id;
            var products = await _productService.GetProductsMarkedAsNewAsync(storeId);

            foreach (var product in products)
            {
                var productUrl = Url.RouteUrl("Product", new { SeName = await _urlRecordService.GetSeNameAsync(product) }, _webHelper.GetCurrentRequestProtocol());
                var productName = await _localizationService.GetLocalizedAsync(product, x => x.Name);
                var productDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription);
                var item = new RssItem(productName, productDescription, new Uri(productUrl), $"urn:store:{store.Id}:newProducts:product:{product.Id}", product.CreatedOnUtc);
                items.Add(item);
                //uncomment below if you want to add RSS enclosure for pictures
                //var picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();
                //if (picture != null)
                //{
                //    var imageUrl = _pictureService.GetPictureUrl(picture, _mediaSettings.ProductDetailsPictureSize);
                //    item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("type", "image/jpeg"), new XAttribute("url", imageUrl), new XAttribute("length", picture.PictureBinary.Length)));
                //}

            }
            feed.Items = items;
            return new RssActionResult(feed, _webHelper.GetThisPageUrl(false));
        }

        #endregion

        #region Product reviews

        public virtual async Task<IActionResult> ProductReviews(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews)
                return RedirectToRoute("Homepage");

            var model = new ProductReviewsModel();
            model = await _productModelFactory.PrepareProductReviewsModelAsync(model, product);

            await ValidateProductReviewAvailabilityAsync(product);

            //default value
            model.AddProductReview.Rating = _catalogSettings.DefaultProductRatingValue;
            
            //default value for all additional review types
            if (model.ReviewTypeList.Count > 0)
                foreach (var additionalProductReview in model.AddAdditionalProductReviewList)
                {
                    additionalProductReview.Rating = additionalProductReview.IsRequired ? _catalogSettings.DefaultProductRatingValue : 0;
                }

            return View(model);
        }

        [HttpPost, ActionName("ProductReviews")]
        [FormValueRequired("add-review")]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ProductReviewsAdd(int productId, ProductReviewsModel model, bool captchaValid)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            if (product == null || product.Deleted || !product.Published || !product.AllowCustomerReviews ||
                !await _productService.CanAddReviewAsync(product.Id, _catalogSettings.ShowProductReviewsPerStore ? currentStore.Id : 0))
                return RedirectToRoute("Homepage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnProductReviewPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            await ValidateProductReviewAvailabilityAsync(product);

            if (ModelState.IsValid)
            {
                //save review
                var rating = model.AddProductReview.Rating;
                if (rating < 1 || rating > 5)
                    rating = _catalogSettings.DefaultProductRatingValue;
                var isApproved = !_catalogSettings.ProductReviewsMustBeApproved;
                var customer = await _workContext.GetCurrentCustomerAsync();

                var productReview = new ProductReview
                {
                    ProductId = product.Id,
                    CustomerId = customer.Id,
                    Title = model.AddProductReview.Title,
                    ReviewText = model.AddProductReview.ReviewText,
                    Rating = rating,
                    HelpfulYesTotal = 0,
                    HelpfulNoTotal = 0,
                    IsApproved = isApproved,
                    CreatedOnUtc = DateTime.UtcNow,
                    StoreId = currentStore.Id,
                };

                await _productService.InsertProductReviewAsync(productReview);

                //add product review and review type mapping                
                foreach (var additionalReview in model.AddAdditionalProductReviewList)
                {
                    var additionalProductReview = new ProductReviewReviewTypeMapping
                    {
                        ProductReviewId = productReview.Id,
                        ReviewTypeId = additionalReview.ReviewTypeId,
                        Rating = additionalReview.Rating
                    };

                    await _reviewTypeService.InsertProductReviewReviewTypeMappingsAsync(additionalProductReview);
                }

                //update product totals
                await _productService.UpdateProductReviewTotalsAsync(product);

                //notify store owner
                if (_catalogSettings.NotifyStoreOwnerAboutNewProductReviews)
                    await _workflowMessageService.SendProductReviewNotificationMessageAsync(productReview, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                await _customerActivityService.InsertActivityAsync("PublicStore.AddProductReview",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddProductReview"), product.Name), product);

                //raise event
                if (productReview.IsApproved)
                    await _eventPublisher.PublishAsync(new ProductReviewApprovedEvent(productReview));

                model = await _productModelFactory.PrepareProductReviewsModelAsync(model, product);
                model.AddProductReview.Title = null;
                model.AddProductReview.ReviewText = null;

                model.AddProductReview.SuccessfullyAdded = true;
                if (!isApproved)
                    model.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SeeAfterApproving");
                else
                    model.AddProductReview.Result = await _localizationService.GetResourceAsync("Reviews.SuccessfullyAdded");

                return View(model);
            }

            //if we got this far, something failed, redisplay form
            model = await _productModelFactory.PrepareProductReviewsModelAsync(model, product);
            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SetProductReviewHelpfulness(int productReviewId, bool washelpful)
        {
            var productReview = await _productService.GetProductReviewByIdAsync(productReviewId);
            if (productReview == null)
                throw new ArgumentException("No product review found with the specified id");

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToReviewProduct)
            {
                return Json(new
                {
                    Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.OnlyRegistered"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            //customers aren't allowed to vote for their own reviews
            if (productReview.CustomerId == customer.Id)
            {
                return Json(new
                {
                    Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.YourOwnReview"),
                    TotalYes = productReview.HelpfulYesTotal,
                    TotalNo = productReview.HelpfulNoTotal
                });
            }

            await _productService.SetProductReviewHelpfulnessAsync(productReview, washelpful);

            //new totals
            await _productService.UpdateProductReviewHelpfulnessTotalsAsync(productReview);

            return Json(new
            {
                Result = await _localizationService.GetResourceAsync("Reviews.Helpfulness.SuccessfullyVoted"),
                TotalYes = productReview.HelpfulYesTotal,
                TotalNo = productReview.HelpfulNoTotal
            });
        }

        public virtual async Task<IActionResult> CustomerProductReviews(int? pageNumber)
        {
            if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()))
                return Challenge();

            if (!_catalogSettings.ShowProductReviewsTabOnAccountPage)
            {
                return RedirectToRoute("CustomerInfo");
            }

            var model = await _productModelFactory.PrepareCustomerProductReviewsModelAsync(pageNumber);

            return View(model);
        }

        #endregion

        #region Email a friend

        public virtual async Task<IActionResult> ProductEmailAFriend(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("Homepage");

            var model = new ProductEmailAFriendModel();
            model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, false);
            return View(model);
        }

        [HttpPost, ActionName("ProductEmailAFriend")]
        [FormValueRequired("send-email")]
        [ValidateCaptcha]
        public virtual async Task<IActionResult> ProductEmailAFriendSend(ProductEmailAFriendModel model, bool captchaValid)
        {
            var product = await _productService.GetProductByIdAsync(model.ProductId);
            if (product == null || product.Deleted || !product.Published || !_catalogSettings.EmailAFriendEnabled)
                return RedirectToRoute("Homepage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailProductToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Common.WrongCaptchaMessage"));
            }

            //check whether the current customer is guest and ia allowed to email a friend
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (await _customerService.IsGuestAsync(customer) && !_catalogSettings.AllowAnonymousUsersToEmailAFriend)
            {
                ModelState.AddModelError("", await _localizationService.GetResourceAsync("Products.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                await _workflowMessageService.SendProductEmailAFriendMessageAsync(customer,
                        (await _workContext.GetWorkingLanguageAsync()).Id, product,
                        model.YourEmailAddress, model.FriendEmail,
                        _htmlFormatter.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, true);
                model.SuccessfullySent = true;
                model.Result = await _localizationService.GetResourceAsync("Products.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            model = await _productModelFactory.PrepareProductEmailAFriendModelAsync(model, product, true);
            return View(model);
        }

        #endregion

        #region Comparing products

        [HttpPost]
        public virtual async Task<IActionResult> AddProductToCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || product.Deleted || !product.Published)
                return Json(new
                {
                    success = false,
                    message = "No product found with the specified ID"
                });

            if (!_catalogSettings.CompareProductsEnabled)
                return Json(new
                {
                    success = false,
                    message = "Product comparison is disabled"
                });

            await _compareProductsService.AddProductToCompareListAsync(productId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddToCompareList",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddToCompareList"), product.Name), product);

            return Json(new
            {
                success = true,
                message = string.Format(await _localizationService.GetResourceAsync("Products.ProductHasBeenAddedToCompareList.Link"), Url.RouteUrl("CompareProducts"))
                //use the code below (commented) if you want a customer to be automatically redirected to the compare products page
                //redirect = Url.RouteUrl("CompareProducts"),
            });
        }

        public virtual async Task<IActionResult> RemoveProductFromCompareList(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return RedirectToRoute("Homepage");

            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("Homepage");

            await _compareProductsService.RemoveProductFromCompareListAsync(productId);

            return RedirectToRoute("CompareProducts");
        }

        public virtual async Task<IActionResult> CompareProducts()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("Homepage");

            var model = new CompareProductsModel
            {
                IncludeShortDescriptionInCompareProducts = _catalogSettings.IncludeShortDescriptionInCompareProducts,
                IncludeFullDescriptionInCompareProducts = _catalogSettings.IncludeFullDescriptionInCompareProducts,
            };

            var products = await (await _compareProductsService.GetComparedProductsAsync())
            //ACL and store mapping
            .WhereAwait(async p => await _aclService.AuthorizeAsync(p) && await _storeMappingService.AuthorizeAsync(p))
            //availability dates
            .Where(p => _productService.ProductIsAvailable(p)).ToListAsync();

            //prepare model
            (await _productModelFactory.PrepareProductOverviewModelsAsync(products, prepareSpecificationAttributes: true))
                .ToList()
                .ForEach(model.Products.Add);

            return View(model);
        }

        public virtual IActionResult ClearCompareList()
        {
            if (!_catalogSettings.CompareProductsEnabled)
                return RedirectToRoute("Homepage");

            _compareProductsService.ClearCompareProducts();

            return RedirectToRoute("CompareProducts");
        }

        #endregion
    }
}
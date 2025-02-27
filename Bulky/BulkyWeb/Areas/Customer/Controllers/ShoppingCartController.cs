using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShoppingCartVM  ShoppingCartVM { get; set; }

        public ShoppingCartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(includeProperties: "Product").Where(s => s.ApplicationUserId == userId).ToList(),
                OrderHeader = new()
            };

            foreach(ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            return View(ShoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCartRepository.Get(c => c.Id == cartId);

            cart.Count += 1;

            _unitOfWork.ShoppingCartRepository.Update(cart);
            _unitOfWork.Save();

            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCartRepository.Get(c => c.Id == cartId);

            if(cart.Count <= 1)
            {
                _unitOfWork.ShoppingCartRepository.Remove(cart);
            }

            else
            {
                cart.Count -= 1;
                _unitOfWork.ShoppingCartRepository.Update(cart);

            }

            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            ShoppingCart cart = _unitOfWork.ShoppingCartRepository.Get(c => c.Id == cartId);
            _unitOfWork.ShoppingCartRepository.Remove(cart);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(includeProperties: "Product").Where(s => s.ApplicationUserId == userId).ToList(),
                OrderHeader = new()
            };

            ShoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            ShoppingCartVM.OrderHeader.Name = ShoppingCartVM.OrderHeader.ApplicationUser.Name;
            ShoppingCartVM.OrderHeader.PhoneNumber = ShoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            ShoppingCartVM.OrderHeader.StreetAddress = ShoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            ShoppingCartVM.OrderHeader.City = ShoppingCartVM.OrderHeader.ApplicationUser.City;
            ShoppingCartVM.OrderHeader.State = ShoppingCartVM.OrderHeader.ApplicationUser.State;
            ShoppingCartVM.OrderHeader.PostalCode = ShoppingCartVM.OrderHeader.ApplicationUser.PostalCode;


            foreach (ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            return View(ShoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPOST()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(includeProperties: "Product").Where(s => s.ApplicationUserId == userId).ToList();
            ShoppingCartVM.OrderHeader.OrderDate = DateTime.Now;
            ShoppingCartVM.OrderHeader.ApplicationUserId = userId;
            ApplicationUser ApplicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            foreach (ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }

            if(ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
                ShoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                ShoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
            }

            _unitOfWork.OrderHeaderRepository.Add(ShoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = ShoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
            }

            //Stripe - regular
            if (ApplicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7285/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"Customer/ShoppingCart/OrderConfirmation?id={ShoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "Customer/ShoppingCart/Index",
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach(ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(cart.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = cart.Product.Title
                            }
                        },
                        Quantity = cart.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }


                var service = new Stripe.Checkout.SessionService();
                Session session = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(ShoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction("OrderConfirmation", new {id = ShoppingCartVM.OrderHeader.Id});
        }

        public IActionResult OrderConfirmation(int id)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.Get(o => o.Id == id, includeProperties: "ApplicationUser");

            if(orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if(session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderRepository.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartRepository.GetAll().Where(s => s.ApplicationUserId == orderHeader.ApplicationUserId).ToList();

            _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(id);
        }

        private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if (shoppingCart.Count <= 50)
                return shoppingCart.Product.Price;
            else if (shoppingCart.Count <= 100)
                return shoppingCart.Product.Price50;
            return shoppingCart.Product.Price100;
        }
    }
}

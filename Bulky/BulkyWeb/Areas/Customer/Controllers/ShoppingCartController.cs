using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class ShoppingCartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
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
            };

            foreach(ShoppingCart cart in ShoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                ShoppingCartVM.OrderTotal += cart.Price * cart.Count;
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
            return View();
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

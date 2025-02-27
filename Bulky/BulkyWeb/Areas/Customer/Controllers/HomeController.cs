using System.Diagnostics;
using System.Security.Claims;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.ProductRepository.GetAll(includeProperties:"Category");
            return View(productList);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart shoppingCart = new()
            {
                Id = 0,
                ProductId = id,
                Product = _unitOfWork.ProductRepository.Get(p => p.Id == id, includeProperties: "Category"),
                Count = 1
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            //Iz nekog razloga uvek dobije 1?????
            shoppingCart.Id = 0;
            var claimsIdentity = (ClaimsIdentity) User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(ModelState.IsValid)
            {
                shoppingCart.ApplicationUserId = userId;

                ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(s => s.ProductId == shoppingCart.ProductId && s.ApplicationUserId == userId);

                if(cartFromDb != null)
                {
                    cartFromDb.Count += shoppingCart.Count;
                    _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
                    TempData["success"] = "Cart succesfully updated";
                }

                else
                {
                    _unitOfWork.ShoppingCartRepository.Add(shoppingCart);
                    TempData["success"] = "Cart succesfully created";
                }

                _unitOfWork.Save();
                return RedirectToAction("Index");
            }

            return View(shoppingCart);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.ProductRepository.GetAll().ToList();
            
            return View(products);
        }

        public IActionResult Upsert(int? id)
        {
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = new Product()
            };

            if(id == null || id == 0)
            {
                return View(productVM);
            }
            else
            {
                productVM.Product = _unitOfWork.ProductRepository.Get(p => p.Id == id);
                return View(productVM);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if(file != null)
                {
                    string filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if(!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        string oldImg = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));

                        if(System.IO.File.Exists(oldImg))
                        {
                            System.IO.File.Delete(oldImg); 
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(productPath, filename), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    productVM.Product.ImageUrl = @"\images\product\" + filename;
                }

                if(productVM.Product.Id == 0)
                    _unitOfWork.ProductRepository.Add(productVM.Product);
                else
                    _unitOfWork.ProductRepository.Update(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Successfully created product";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                });
                return View(productVM);
            }
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id <= 0)
            {
                return NotFound();
            }

            Product? product = _unitOfWork.ProductRepository.Get(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.CategoryRepository.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                }),
                Product = product
            };

            return View(productVM);
        }

        [HttpPost]
        public IActionResult Delete(ProductVM productVM)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductRepository.Remove(productVM.Product);
                _unitOfWork.Save();
                TempData["success"] = "Successfully deleted product";
                return RedirectToAction("Index");
            }

            return View(productVM);
        }
    }
}

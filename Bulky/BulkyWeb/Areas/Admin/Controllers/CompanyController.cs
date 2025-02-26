using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Bulky.Models.ViewModel;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            List<Company> companies = _unitOfWork.CompanyRepository.GetAll().ToList();

            return View(companies);
        }

        public IActionResult Upsert(int? id)
        {
            Company? company = new Company();

            if (id == null || id == 0)
            {
                return View(company);
            }
            else
            {
                company = _unitOfWork.CompanyRepository.Get(c => c.Id == id);
                return View(company);
            }
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if (company.Id == 0)
                    _unitOfWork.CompanyRepository.Add(company);
                else
                    _unitOfWork.CompanyRepository.Update(company);

                _unitOfWork.Save();

                TempData["success"] = "Successfully created company";
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> companies = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new { data = companies });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            Company? company = _unitOfWork.CompanyRepository.Get(c => c.Id == id);
            if (company == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }


            _unitOfWork.CompanyRepository.Remove(company);
            _unitOfWork.Save();

            return Json(new { success = true, message = "Delete successful" });
        }

        #endregion
    }
}

using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebEnterprise.Models.Entities;
using WebEnterprise.Repositories.Abstraction;
using WebEnterprise.ViewModels.Faculty;

namespace WebEnterprise.Areas.Admin.Controllers
{
    [Area("Admin")]
    //[Authorize(Roles = "Admin")]
    public class FacultyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public INotyfService _notyfService { get; }

        public FacultyController(IUnitOfWork unitOfWork, INotyfService notyfService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notyfService = notyfService;
        }

        public IActionResult Index()
        {
            List<Faculty> facultyList = _unitOfWork.FacultyRepository.GetAll2().ToList();
            return View(facultyList);

        }

        // Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateFacultyModel faculty)
        {

            if (ModelState.IsValid)
            {
                _notyfService.Success("You successfully create a new Faculty");
                var facultyVM = _mapper.Map<Faculty>(faculty);
                await _unitOfWork.FacultyRepository.Add(facultyVM);
                return RedirectToAction("Index");
            }
            else
            {
                return View();
            }
        }

        // Edit
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Faculty? facultyFromDb = _unitOfWork.FacultyRepository.Get(u => u.Id == id);

            if (facultyFromDb == null)
            {
                return NotFound();
            }
            EditFacultyModel facultyVM = _mapper.Map<EditFacultyModel>(facultyFromDb);
            return View(facultyVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit([Bind("Id,Name,Description")] EditFacultyModel facultyVM)
        {
            if (ModelState.IsValid)
            {
                var faculty = await _unitOfWork.FacultyRepository.GetById(facultyVM.Id);
                if(faculty == null)
                {
                    return NotFound();
                }
                 _mapper.Map(facultyVM, faculty);
                await _unitOfWork.FacultyRepository.Update(faculty);
                _notyfService.Success("You successfully update a Faculty");
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        //Delete
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Faculty? facultyFromDb = _unitOfWork.FacultyRepository.Get(u => u.Id == id);

            if (facultyFromDb == null)
            {
                return NotFound();
            }
            return View(facultyFromDb);
        }

        [HttpDelete]
        public async Task<IActionResult> DeletePOST(int? id)
        {
            Faculty? obj = _unitOfWork.FacultyRepository.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _notyfService.Success("You successfully delete a Faculty");
            await _unitOfWork.FacultyRepository.Delete(obj);
            return RedirectToAction("Index");
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<Faculty> facultyList = _unitOfWork.FacultyRepository.GetAll2().ToList();
            return Json(new { data = facultyList });
        }


        #endregion
    }
}

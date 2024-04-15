
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebEnterprise.Repositories.Abstraction;
using WebEnterprise.ViewModels.Megazine;
using X.PagedList;

namespace WebEnterprise.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public HomeController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        // GET: Home
        public async Task<IActionResult> Index(string query, int? page)
        {
            ViewBag.StoredQuery = query;
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            var megazines = await _unitOfWork.MegazineRepository.GetMegazinesWithRelevant(query);
            var megazineModels = _mapper.Map<List<GetMegazineModel>>(megazines);
            return View(megazineModels.ToPagedList(pageNumber, pageSize));
        }

    }
}

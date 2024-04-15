using AspNetCoreHero.ToastNotification.Abstractions;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using NuGet.Packaging.Signing;
using System.Net.Sockets;
using WebEnterprise.Infrastructure.Persistance;
using WebEnterprise.Models.Entities;
using WebEnterprise.Repositories.Abstraction;
using WebEnterprise.ViewModels.Contribution;
using WebEnterprise.ViewModels.Megazine;

namespace WebEnterprise.Areas.Coordinator.Controllers
{
    [Area("Coordinator")]
    public class MegazineController : Controller
    {
        private readonly UniversityDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public INotyfService _notyfService { get; }

        private static string apiKey = "AIzaSyDxbWg5cX5zoEDrQCssfBGh5CZrRkAr8ro";
        private static string Bucket = "webenterprise-8a158.appspot.com";
        private static string AuthEmail = "betngaongo@gmail.com";
        private static string AuthPassword = "betngaongo";

        public MegazineController(UniversityDbContext context, IUnitOfWork unitOfWork, IMapper mapper, INotyfService notyfService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _notyfService = notyfService;
        }

        // GET: Coordinator/Megazine

        public async Task<IActionResult> Index()
        {
            // Lấy FacultyId của người dùng từ Session
            int? currentUserFacultyId = _httpContextAccessor.HttpContext.Session.GetInt32("FacultyId");

            if (currentUserFacultyId.HasValue)
            {
                // Lọc danh sách magazines dựa trên FacultyId của người dùng hiện tại
                var megazines = _context.Megazines
                                        .Where(e => !e.IsDeleted && e.FacultyId == currentUserFacultyId)
                                        .Include(m => m.Faculty);

                return View(await megazines.ToListAsync());
            }
            else
            {
                return Redirect("/authentication/login");
            }
        }


        [HttpGet]
        public async Task<IActionResult> Contributions(int id)
        {

            int? facultyId = _httpContextAccessor.HttpContext.Session.GetInt32("FacultyId");
            ViewBag.FacultyId = facultyId;

            var contributions = await _context.Contributions
                                          .Where(con => con.MegazineId == id)
                .Select(c => new GetContributionModel
                {
                    Id = c.Id,
                    Title = c.Title,
                    CreatedDate = c.CreatedDate,
                    FullName = c.User.FullName,
                    ProfilePicture = c.User.ProfilePicture,
                    ReplyCount = c.Comments.Count(),
                    Megazine = c.Megazine.Name,
                    FilePath = c.FilePath,
                    Status = c.Status
                })
                .ToListAsync();
            ContributionForCoo contributionForCoo = new ContributionForCoo();
            contributionForCoo.Contributions = contributions;
            contributionForCoo.MegazineId = id;
            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            var storage = new FirebaseStorage(Bucket, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
            });
            try
            {
                foreach (var con in contributions)
                {
                    var conReference = storage.Child("assets").Child(con.ProfilePicture);
                    var downloadConTask = conReference.GetDownloadUrlAsync();
                    var conPath = await downloadConTask;
                    con.ProfilePicture = conPath;
                }
            }catch(Exception err)
            {
                Console.WriteLine(err);
            }

            if (contributions == null)
            {
                return NotFound();
            }

            return View(contributionForCoo);
        }

        [HttpGet]
        public async Task<IActionResult> ContributionDetail(int id)
        {
            var contribution = await _unitOfWork.ContributionRepository.GetContributionWithRelevant(id);
            contribution.FacultyUserName = await _unitOfWork.UserRepository.FindUserName(contribution.FacultyUserName);
            if (contribution == null)
            {
                return NotFound();
            }

            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            var storage = new FirebaseStorage(Bucket, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
            });
            var reference = storage.Child("assets").Child(contribution.FilePath);
            var listImagePath = new List<string>();

            try
            {
                // Get a download URL for the file
                var downloadUrlTask = reference.GetDownloadUrlAsync();
                var downloadUrl = await downloadUrlTask;

                if (downloadUrl != null)
                {
                    ViewBag.DocumentUrl = downloadUrl;
                }

                foreach (var image in contribution.imagePaths)
                {
                    var imageReference = storage.Child("assets").Child(image);
                    var downloadImageTask = imageReference.GetDownloadUrlAsync();
                    var imagePath = await downloadImageTask;
                    listImagePath.Add(imagePath);
                }
                ViewBag.ImagePaths = listImagePath;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving download URL: " + ex.Message);
            }

            return View(contribution);
        }


        [HttpPost]
        public async Task<IActionResult> ToggleContributionStatus(int id)
        {
            var contribution = await _context.Contributions.FindAsync(id);
            if (contribution == null)
            {
                return NotFound();
            }

            contribution.Status = contribution.Status == "Pending" ? "Accept" : "Pending";
            _context.Contributions.Update(contribution);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Contributions), new { id = contribution.MegazineId }); 
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditContribution model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var contribution = await _unitOfWork.ContributionRepository.GetById(model.Id);
                    if (contribution == null)
                    {
                        _notyfService.Information("Failed to find contribution with ID: " + model.Id);
                    }
                    contribution.Title = model.Title;
                    await _unitOfWork.ContributionRepository.Update(contribution);
                    _notyfService.Success("Editing Successful");
                }
                catch (Exception err)
                {
                    Console.WriteLine(err);
                }
            }
            else
            {
                _notyfService.Warning("Failed");
            }
            return RedirectToAction("Contributions", new { id = model.MegazineId });
        }
    }
}
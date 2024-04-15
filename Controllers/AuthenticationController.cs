using AspNetCoreHero.ToastNotification.Abstractions;
using Firebase.Auth;
using Firebase.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using WebEnterprise.Infrastructure.Persistance;
using WebEnterprise.Models.Entities;
using WebEnterprise.Repositories.Abstraction;
using WebEnterprise.Repositories.Implement;
using WebEnterprise.ViewModels;
using WebEnterprise.ViewModels.User;
using User = WebEnterprise.Models.Entities.User;

namespace WebEnterprise.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly IUserAuthenticationService _service;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostEnvironment;
        public INotyfService _notyfService { get; }
        private static string apiKey = "AIzaSyDxbWg5cX5zoEDrQCssfBGh5CZrRkAr8ro";
        private static string Bucket = "webenterprise-8a158.appspot.com";
        private static string AuthEmail = "betngaongo@gmail.com";
        private static string AuthPassword = "betngaongo";

        public AuthenticationController(IUserAuthenticationService service, UserManager<User> userManager,
            IHttpContextAccessor httpContextAccessor, INotyfService notyfService, IWebHostEnvironment webHostEnvironment)
        {
            _service = service;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _notyfService = notyfService;
            _hostEnvironment = webHostEnvironment;
        }

        public IActionResult Register()
        {
            ViewData["Message"] = "Registration Page";
            return View();
        }


        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel user)
        {
            if (ModelState.IsValid)
            {
                var checkUser = await _userManager.FindByEmailAsync(user.Email);
                var result = await _service.LoginAsync(user);
                if (result.StatusCode == 1)
                {
                    _httpContextAccessor.HttpContext.Session.SetString("Email", user.Email);
                    _httpContextAccessor.HttpContext.Session.SetString("UserId", checkUser.Id);
                    _httpContextAccessor.HttpContext.Session.SetString("UserName", checkUser.FullName);
                    _httpContextAccessor.HttpContext.Session.SetInt32("FacultyId", checkUser.FacultyId);
                    _notyfService.Success("Login Successfully");

                    if (await _userManager.IsInRoleAsync(checkUser, "Coordinator"))
                    {
                        return RedirectToAction("Index", "Megazine", new { area = "Coordinator" });
                    }
                    else if (await _userManager.IsInRoleAsync(checkUser, "Admin"))
                    {
                        return RedirectToAction("Index", "Account", new { area = "Admin" });
                    }
                    else if (await _userManager.IsInRoleAsync(checkUser, "Manager"))
                    {
                        return RedirectToAction("Index", "Dashboard", new { area = "Manager" });
                    }
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    TempData["msg"] = result.Message;
                    return View(user);
                }
            }
            else
            {
                return View();
            }

        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _service.TaskLogoutAsync();
            return View("Login");
        }

        [Authorize]
        public async Task<IActionResult> ViewProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            var storage = new FirebaseStorage(Bucket, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
            });
            var reference = storage.Child("assets").Child(user.ProfilePicture);

            try
            {
                var downloadUrlTask = reference.GetDownloadUrlAsync();
                var downloadUrl = await downloadUrlTask;

                if (downloadUrl != null)
                {
                    ViewBag.ProfilePicture = downloadUrl;
                }              
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving download URL: " + ex.Message);
            }
            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            var model = new EditProfileModel
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                ProfilePictureUrl = user.ProfilePicture
            };
            var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
            var storage = new FirebaseStorage(Bucket, new FirebaseStorageOptions
            {
                AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
            });
            var reference = storage.Child("assets").Child(user.ProfilePicture);

            try
            {
                var downloadUrlTask = reference.GetDownloadUrlAsync();
                var downloadUrl = await downloadUrlTask;

                if (downloadUrl != null)
                {
                    ViewBag.ProfilePicture = downloadUrl;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving download URL: " + ex.Message);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(EditProfileModel model)
        {
            if (ModelState.IsValid)
            {
                var foundUser = await _userManager.GetUserAsync(User);
                if (foundUser == null)
                {
                    return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
                }

                foundUser.Email = model.Email;
                foundUser.PhoneNumber = model.PhoneNumber;
                foundUser.FullName = model.FullName;
                foundUser.UserName = model.FullName.Replace(" ", "");
                var fileUpload = model.ProfilePicture;
                FileStream fileStream;
                if (fileUpload.Length > 0)
                {
                    string folderName = "documentFile";
                    string path = Path.Combine(_hostEnvironment.WebRootPath, $"files/{folderName}");
                    string uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileUpload.FileName)}";

                    var auth = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
                    var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                    var cancellation = new CancellationTokenSource();

                    using (var stream = fileUpload.OpenReadStream())
                    {
                        var task = new FirebaseStorage(
                            Bucket,
                            new FirebaseStorageOptions
                            {
                                AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                                ThrowOnCancel = true
                            })
                            .Child("assets")
                            .Child($"{uniqueFileName}")
                            .PutAsync(stream, cancellation.Token);
                        try
                        {
                            _notyfService.Success("success");
                            foundUser.ProfilePicture = uniqueFileName;
                            var result = await _userManager.UpdateAsync(foundUser);
                            if (result.Succeeded)
                            {
                                return RedirectToAction("ViewProfile");
                            }


                        }
                        catch (Exception ex)
                        {
                            _notyfService.Information("failed:" + ex.Message);
                            Debug.WriteLine(ex);
                        }

                    }
                }

            }
            return View("EditProfile", model);

        }
    }
}

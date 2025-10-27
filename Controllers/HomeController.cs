using Microsoft.AspNetCore.Mvc;
using AccTion.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace AccTion.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly PostgresContext _context;
        private readonly IWebHostEnvironment _environment;

        public HomeController(IHttpClientFactory clientFactory, PostgresContext context, IWebHostEnvironment environment)
        {
            _clientFactory = clientFactory;
            _context = context;
            _environment = environment;
        }

        public IActionResult Index()
        {
            // Load all posts from all users (newest first) - like Instagram feed
            var posts = _context.Posts
                .Include(p => p.UserTable)
                .OrderByDescending(p => p.CreatedAt)
                .ToList();
            
            return View(posts);
        }

        // ✅ NEW: Create Post Action (for authenticated users)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(string caption, IFormFile? imageFile, IFormFile? videoFile)
        {
            try
            {
                var userId = User.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return RedirectToAction("Login");
                }

                var post = new Post
                {
                    UserTableId = int.Parse(userId),
                    Caption = caption,
                    CreatedAt = DateTime.UtcNow,
                    LikeCount = 0,
                    CommentCount = 0
                };

                // Handle Image Upload
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "posts");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    post.Image = $"/uploads/posts/{fileName}";
                }

                // Handle Video Upload
                if (videoFile != null && videoFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "posts");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileName = $"{Guid.NewGuid()}_{videoFile.FileName}";
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await videoFile.CopyToAsync(stream);
                    }

                    post.Video = $"/uploads/posts/{fileName}";
                }

                _context.Posts.Add(post);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Post created successfully!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating post: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // ... rest of your existing code (Privacy, Datas, Register, Login, etc.)
        
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Datas()
        {
            var userTypeId = User.FindFirst("userTypeId")?.Value;
            if (userTypeId != "3")
            {
                return RedirectToAction("Index");
            }
            // ... rest of existing Datas code
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("http://localhost:5289/api/User/all");
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                    var root = doc.RootElement;
                    var usersData = root.GetProperty("data");
                    var users = new List<UserViewModel>();
                    foreach (var userElement in usersData.EnumerateArray())
                    {
                        int? userTypeValue = null;
                        if (userElement.TryGetProperty("userTypeId", out var userTypeElement) &&
                            userTypeElement.ValueKind != JsonValueKind.Null)
                        {
                            userTypeValue = userTypeElement.GetInt32();
                        }
                        int? subTypeValue = null;
                        if (userElement.TryGetProperty("subscriptionTypeId", out var subTypeElement) &&
                            subTypeElement.ValueKind != JsonValueKind.Null)
                        {
                            subTypeValue = subTypeElement.GetInt32();
                        }
                        DateTime createdDate = DateTime.MinValue;
                        if (userElement.TryGetProperty("createdAt", out var dateElement) &&
                            dateElement.ValueKind != JsonValueKind.Null)
                        {
                            createdDate = dateElement.GetDateTime();
                        }
                        string? photoPath = null;
                        if (userElement.TryGetProperty("photoPath", out var photoElement) &&
                            photoElement.ValueKind != JsonValueKind.Null)
                        {
                            photoPath = photoElement.GetString();
                        }
                        users.Add(new UserViewModel
                        {
                            UserID = userElement.GetProperty("id").GetInt32(),
                            Username = userElement.GetProperty("name").GetString() ?? "N/A",
                            Email = userElement.GetProperty("email").GetString() ?? "N/A",
                            UserType = userTypeValue ?? 0,
                            SubscriptionTypeId = subTypeValue,
                            RegistrationDate = createdDate,
                            PhoneNumber = userElement.GetProperty("pNo").GetString() ?? "",
                            PhotoPath = photoPath
                        });
                    }
                    ViewBag.Users = users;
                    ViewBag.Message = $"Total users: {users.Count}";
                }
                else
                {
                    ViewBag.Error = "Failed to fetch users from API";
                    ViewBag.Users = new List<UserViewModel>();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error fetching users: {ex.Message}";
                ViewBag.Users = new List<UserViewModel>();
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Registration", model);

            var client = _clientFactory.CreateClient();
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5289/api/Auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Registration successful! Please log in.";
                return RedirectToAction("Login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ViewBag.ErrorMessage = errorContent.Trim('"');
                return View("Registration", model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Dashboard");
                }
            }
            return View();
        }

        [HttpGet]
        public IActionResult Registration()
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (!string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Dashboard");
                }
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CheckAdminExists()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("http://localhost:5289/api/Auth/check-admin");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<JsonElement>(content);
                    var adminExists = result.GetProperty("adminExists").GetBoolean();
                    return Json(new { adminExists });
                }
                return Json(new { adminExists = false });
            }
            catch
            {
                return Json(new { adminExists = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _clientFactory.CreateClient();
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync("http://localhost:5289/api/Auth/login", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(jsonResponse, options);
                    if (tokenResponse?.Token != null)
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var jwtToken = handler.ReadJwtToken(tokenResponse.Token);
                        var claims = new List<Claim>();
                        foreach (var claim in jwtToken.Claims)
                        {
                            claims.Add(claim);
                        }
                        var identity = new ClaimsIdentity(claims, "Cookies");
                        var principal = new ClaimsPrincipal(identity);
                        await HttpContext.SignInAsync("Cookies", principal);
                        HttpContext.Session.SetString("JwtToken", tokenResponse.Token);
                        Response.Cookies.Append("JwtToken", tokenResponse.Token,
                            new Microsoft.AspNetCore.Http.CookieOptions
                            {
                                HttpOnly = true,
                                Secure = true,
                                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict
                            });
                        return RedirectToAction("Dashboard");
                    }
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Login failed: " + ex.Message);
            }
            return View(model);
        }

        [Authorize]
        public IActionResult Dashboard()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("JwtToken");
            Response.Cookies.Delete("JwtToken");
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index");
        }

        [Authorize]
        public async Task<IActionResult> Profile(int? id)
        {
            try
            {
                var currentUserId = User.FindFirst("userId")?.Value;
                var targetUserId = id?.ToString() ?? currentUserId;
                if (string.IsNullOrEmpty(targetUserId))
                    return RedirectToAction("Login");

                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync($"http://localhost:5289/api/User/{targetUserId}");
                if (!response.IsSuccessStatusCode)
                {
                    ViewBag.Error = "Failed to fetch user profile";
                    return View();
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                var userData = doc.RootElement.GetProperty("data");

                var photoPath = userData.TryGetProperty("photoPath", out var photoElement) &&
                    photoElement.ValueKind != JsonValueKind.Null
                    ? photoElement.GetString() ?? ""
                    : "";

                var userProfile = new UserViewModel
                {
                    UserID = userData.GetProperty("id").GetInt32(),
                    Username = userData.GetProperty("name").GetString() ?? "N/A",
                    Email = userData.GetProperty("email").GetString() ?? "N/A",
                    PhoneNumber = userData.GetProperty("pNo").GetString() ?? "",
                    UserType = userData.GetProperty("userTypeId").GetInt32(),
                    SubscriptionTypeId = userData.TryGetProperty("subscriptionTypeId", out var subType) &&
                        subType.ValueKind != JsonValueKind.Null ? subType.GetInt32() : null,
                    RegistrationDate = userData.GetProperty("createdAt").GetDateTime(),
                    PhotoPath = photoPath
                };
                var userPosts = _context.Posts
                    .Include(p => p.UserTable)
                    .Where(p => p.UserTableId == userProfile.UserID)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToList();

                ViewBag.IsOwnProfile = currentUserId == targetUserId;
                ViewBag.UserPosts = userPosts;
                ViewBag.PostCount = userPosts.Count;

                return View(userProfile);
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"Error loading profile: {ex.Message}";
                return View();
            }
        }
    }

    public class TokenResponse
    {
        public string? Token { get; set; }
    }
}
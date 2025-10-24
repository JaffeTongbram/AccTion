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

namespace AccTion.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public HomeController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // ✅ UPDATED - Fetch all users from API
        // ✅ UPDATED - Fetch all users from API with proper null handling
// ✅ UPDATED - Fetch all users from API with PhotoPath
[Authorize]
public async Task<IActionResult> Datas()
{
    // Verify user is admin (UserTypeId == 3)
    var userTypeId = User.FindFirst("userTypeId")?.Value;
    if (userTypeId != "3")
    {
        return RedirectToAction("Index");
    }

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
                // ✅ Handle null values safely
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

                // ✅ ADD THIS - Get PhotoPath
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
                    PhotoPath = photoPath  // ✅ ADD THIS
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
                // Read the error message from API
                var errorContent = await response.Content.ReadAsStringAsync();
        
                // The API returns plain text error messages, so use them directly
                ViewBag.ErrorMessage = errorContent.Trim('"'); // Remove quotes if present
        
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
        // **NEW: Add this endpoint for real-time admin check**
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
public async Task<IActionResult> Profile()
{
    try
    {
        var userId = User.FindFirst("userId")?.Value;
        if (string.IsNullOrEmpty(userId))
            return RedirectToAction("Login");

        var client = _clientFactory.CreateClient();
        var response = await client.GetAsync($"http://localhost:5289/api/User/{userId}");

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Failed to fetch user profile";
            return View();
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using JsonDocument doc = JsonDocument.Parse(jsonResponse);
        var userData = doc.RootElement.GetProperty("data");

        // ✅ Correct property name
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
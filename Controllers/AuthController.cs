using Microsoft.AspNetCore.Mvc;
using AccTion.Models;
using AccTion.Helpers;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AccTion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly PasswordHelper _passwordHelper;
        private readonly JwtTokenHelper _jwtHelper;

        public AuthController(PostgresContext context, IConfiguration config)
        {
            _context = context;
            _passwordHelper = new PasswordHelper();
            _jwtHelper = new JwtTokenHelper(config);
        }

        [HttpPost("register")]
public async Task<IActionResult> Register([FromBody] RegisterDto dto)
{
    if (dto == null)
        return BadRequest("Invalid request");
        
    if (string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Email) || 
        string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.PNo))
        return BadRequest("Name, Email, Password and PNo are required");
        
    if (dto.UserTypeId <= 0)
        return BadRequest("User Type is required");
    
    // **NEW: Check if user is trying to register as Admin**
    if (dto.UserTypeId == 3)
    {
        // Check if admin already exists in database
        var adminExists = await _context.UserTables.AnyAsync(u => u.UserTypeId == 3);
        
        if (adminExists)
        {
            return BadRequest("Cannot register as admin. Admin already exists.");
        }
        
        // Admin cannot have subscription type
        if (dto.SubscriptionTypeId.HasValue && dto.SubscriptionTypeId > 0)
            return BadRequest("Admin users cannot have subscription types");
    }
    
    // If Consumer (UserTypeId = 2), SubscriptionTypeId is required
    if (dto.UserTypeId == 2 && (!dto.SubscriptionTypeId.HasValue || dto.SubscriptionTypeId <= 0))
        return BadRequest("Subscription Type is required for Consumers");
    
    if (await _context.UserTables.AnyAsync(u => u.Email == dto.Email))
        return BadRequest("Email already exists");
    
    var user = new UserTable
    {
        Name = dto.Name!,
        Email = dto.Email!,
        Password = _passwordHelper.HashPassword(dto.Password!),
        PNo = dto.PNo,
        UserTypeId = dto.UserTypeId,
        SubscriptionTypeId = dto.SubscriptionTypeId
    };
    
    _context.UserTables.Add(user);
    await _context.SaveChangesAsync();
    
    return Ok(new { Message = "Registration successful" });
}

// **NEW: Add this endpoint to check if admin exists (for real-time validation)**
[HttpGet("check-admin")]
public async Task<IActionResult> CheckAdminExists()
{
    var adminExists = await _context.UserTables.AnyAsync(u => u.UserTypeId == 3);
    return Ok(new { adminExists });
}

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.UserTables.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !_passwordHelper.VerifyPassword(user.Password, request.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            var token = _jwtHelper.GenerateToken(user.Email, user.Id, user.UserTypeId);
            return Ok(new { token });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class RegisterDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PNo { get; set; } =string.Empty;
    public int UserTypeId { get; set; }
    public int? SubscriptionTypeId { get; set; }
}
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccTion.Models;
using AccTion.Helpers;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace AccTion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTablesController : ControllerBase
    {
        private readonly PostgresContext _context;
        private readonly PasswordHelper _passwordHelper;
        private readonly JwtTokenHelper _jwtHelper;

        public UserTablesController(PostgresContext context, IConfiguration config)
        {
            _context = context;
            _passwordHelper = new PasswordHelper();
            _jwtHelper = new JwtTokenHelper(config);
        }

        // ========================
        // 🔐 REGISTER
        // ========================
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserTable user)
        {
            if (_context.UserTables.Any(u => u.Email == user.Email))
                return BadRequest(new { message = "Email already exists." });

            user.Password = _passwordHelper.HashPassword(user.Password);
            user.CreatedAt = DateTime.UtcNow;

            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            return Ok( "User registered successfully." );
        }

        // ========================
        // 🔐 LOGIN
        // ========================
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _context.UserTables.FirstOrDefault(u => u.Email == request.Email);
            if (user == null || !_passwordHelper.VerifyPassword(user.Password, request.Password))
                return Unauthorized(new { message = "Invalid email or password." });

            var token = _jwtHelper.GenerateToken(user.Email, user.Id);
            return Ok(new { token });
        }

        // ========================
        // 🔒 PROTECTED PROFILE
        // ========================
        [Authorize]
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var email = User.Identity?.Name ?? "Unknown";
            var userId = User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value ?? "0";

            return Ok(new
            {
                message = $"Welcome, {email}",
                userId
            });
        }

        // ========================
        // ✅ GET: All Users
        // ========================
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserTable>>> GetUserTables()
        {
            return await _context.UserTables.ToListAsync();
        }

        // ========================
        // ✅ GET: User by ID
        // ========================
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTable>> GetUserTable(int id)
        {
            var user = await _context.UserTables.FindAsync(id);

            if (user == null)
                return NotFound();

            return user;
        }

        // ========================
        // ✅ POST: Create User
        // ========================
        [HttpPost]
        public async Task<ActionResult<UserTable>> PostUserTable(UserTable user)
        {
            user.CreatedAt = DateTime.UtcNow;

            _context.UserTables.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUserTable), new { id = user.Id }, user);
        }

        // ========================
        // ✅ PUT: Update User
        // ========================
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserTable(int id, UserTable user)
        {
            if (id != user.Id)
                return BadRequest("ID mismatch.");

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.UserTables.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return NoContent();
        }

        // ========================
        // ✅ DELETE: Delete User
        // ========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTable(int id)
        {
            var user = await _context.UserTables.FindAsync(id);
            if (user == null)
                return NotFound();

            _context.UserTables.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

}

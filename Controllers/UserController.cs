using Microsoft.AspNetCore.Mvc;
using AccTion.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccTion.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly PostgresContext _context;

        public UserController(PostgresContext context)
        {
            _context = context;
        }

        // GET: api/user/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var users = await _context.UserTables
                    .Select(u => new
                    {
                        u.Id,
                        u.Name,
                        u.Email,
                        u.PNo,
                        u.UserTypeId,
                        u.SubscriptionTypeId,
                        u.CreatedAt,
                        u.PhotoPath
                    })
                    .ToListAsync();

                if (users == null || users.Count == 0)
                    return Ok(new { message = "No users found", data = users });

                return Ok(new { message = "Users retrieved successfully", data = users });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving users", error = ex.Message });
            }
        }

        // GET: api/user/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _context.UserTables
                    .Where(u => u.Id == id)
                    .Select(u => new
                    {
                        u.Id,
                        u.Name,
                        u.Email,
                        u.PNo,
                        u.UserTypeId,
                        u.SubscriptionTypeId,
                        u.CreatedAt,
                        u.PhotoPath
                    })
                    .FirstOrDefaultAsync();

                if (user == null)
                    return NotFound(new { message = "User not found" });

                return Ok(new { message = "User retrieved successfully", data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving user", error = ex.Message });
            }
        }
        // POST: api/user/{id}/upload-photo
// POST: api/user/{id}/upload-photo
[HttpPost("{id}/upload-photo")]
public async Task<IActionResult> UploadPhoto(int id, [FromForm] IFormFile file)
{
    try
    {
        Console.WriteLine($"=== UPLOAD PHOTO ENDPOINT HIT - User ID: {id} ===");
        
        if (file == null || file.Length == 0)
        {
            Console.WriteLine("ERROR: No file uploaded");
            return BadRequest(new { message = "No file uploaded" });
        }

        Console.WriteLine($"File received: {file.FileName}, Size: {file.Length} bytes");

        // Validate file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();
        
        if (!allowedExtensions.Contains(fileExtension))
        {
            Console.WriteLine($"ERROR: Invalid file type: {fileExtension}");
            return BadRequest(new { message = "Only image files are allowed (jpg, jpeg, png, gif)" });
        }

        // Validate file size (max 5MB)
        if (file.Length > 5 * 1024 * 1024)
        {
            Console.WriteLine("ERROR: File too large");
            return BadRequest(new { message = "File size must be less than 5MB" });
        }

        var user = await _context.UserTables.FindAsync(id);
        if (user == null)
        {
            Console.WriteLine($"ERROR: User not found - ID: {id}");
            return NotFound(new { message = "User not found" });
        }

        Console.WriteLine($"User found: {user.Name} ({user.Email})");

        // Create uploads directory if it doesn't exist
        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
        Console.WriteLine($"Upload folder path: {uploadsFolder}");
        
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
            Console.WriteLine("Created uploads directory");
        }

        // Generate unique filename
        var fileName = $"{user.Id}_{Guid.NewGuid()}{fileExtension}";
        var filePath = Path.Combine(uploadsFolder, fileName);
        
        Console.WriteLine($"Saving file to: {filePath}");

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        Console.WriteLine("File saved successfully");

        // Delete old photo if exists
        if (!string.IsNullOrEmpty(user.PhotoPath))
        {
            var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.PhotoPath);
            if (System.IO.File.Exists(oldFilePath))
            {
                System.IO.File.Delete(oldFilePath);
                Console.WriteLine($"Deleted old photo: {oldFilePath}");
            }
        }

        // Update user with new photo path
        var newPhotoPath = $"uploads/profiles/{fileName}";
        user.PhotoPath = newPhotoPath;
        
        Console.WriteLine($"Updating user PhotoPath to: {newPhotoPath}");
        
        _context.UserTables.Update(user);
        await _context.SaveChangesAsync();

        Console.WriteLine("Database updated successfully");
        Console.WriteLine($"=== UPLOAD COMPLETE - PhotoPath: {user.PhotoPath} ===");

        return Ok(new { 
            message = "Photo uploaded successfully", 
            photoPath = user.PhotoPath,
            userId = user.Id 
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"=== ERROR IN UPLOAD ===");
        Console.WriteLine($"Exception: {ex.Message}");
        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        
        return StatusCode(500, new { 
            message = "Error uploading photo", 
            error = ex.Message,
            stackTrace = ex.StackTrace 
        });
    }
}

        // PUT: api/user/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
        {
            try
            {
                var user = await _context.UserTables.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                if (!string.IsNullOrWhiteSpace(dto.Name))
                    user.Name = dto.Name;
                if (!string.IsNullOrWhiteSpace(dto.Email))
                    user.Email = dto.Email;
                if (!string.IsNullOrWhiteSpace(dto.PNo))
                    user.PNo = dto.PNo;
                if (dto.UserTypeId.HasValue)
                    user.UserTypeId = dto.UserTypeId.Value;
                if (dto.SubscriptionTypeId.HasValue)
                    user.SubscriptionTypeId = dto.SubscriptionTypeId.Value;

                _context.UserTables.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User updated successfully", data = user });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating user", error = ex.Message });
            }
        }

        // DELETE: api/user/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.UserTables.FindAsync(id);
                if (user == null)
                    return NotFound(new { message = "User not found" });

                _context.UserTables.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting user", error = ex.Message });
            }
        }
    }

    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PNo { get; set; }
        public int? UserTypeId { get; set; }
        public int? SubscriptionTypeId { get; set; }
    }
}
using Microsoft.AspNetCore.Mvc;
using AccTion.Models;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AccTion.Controllers
{
    public class PostController : Controller
    {
        private readonly PostgresContext _context;
        private readonly IWebHostEnvironment _environment;

        public PostController(PostgresContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Post post, IFormFile? ImageFile, IFormFile? VideoFile)
        {
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var imagePath = Path.Combine("uploads/images", ImageFile.FileName);
                var savePath = Path.Combine(_environment.WebRootPath, imagePath);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }
                post.Image = "/" + imagePath;
            }

            if (VideoFile != null && VideoFile.Length > 0)
            {
                var videoPath = Path.Combine("uploads/videos", VideoFile.FileName);
                var savePath = Path.Combine(_environment.WebRootPath, videoPath);
                Directory.CreateDirectory(Path.GetDirectoryName(savePath)!);
                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await VideoFile.CopyToAsync(stream);
                }
                post.Video = "/" + videoPath;
            }

            post.CreatedAt = DateTime.Now;
            post.LikeCount = 0;
            post.CommentCount = 0;

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
public async Task<IActionResult> Index()
{
            var posts = await _context.Posts
            .Include(p => p.UserTable)  // Make sure this is included
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    return View(posts);
}

    }
}

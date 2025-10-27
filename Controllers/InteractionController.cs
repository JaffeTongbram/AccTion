using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AccTion.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace AccTion.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class InteractionController : ControllerBase
    {
        private readonly PostgresContext _context;

        public InteractionController(PostgresContext context)
        {
            _context = context;
        }

        // ✅ LIKE POST
        [HttpPost("like/{postId}")]
        public async Task<IActionResult> LikePost(int postId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                var existingLike = await _context.Interactions
                    .FirstOrDefaultAsync(i => i.PostId == postId && 
                                            i.InteractionUserId == userId && 
                                            i.Likenum == 1);

                if (existingLike != null)
                {
                    _context.Interactions.Remove(existingLike);
                    await _context.SaveChangesAsync();
                    
                    var post = await _context.Posts.FindAsync(postId);
                    return Ok(new { 
                        success = true, 
                        liked = false, 
                        likeCount = post?.LikeCount ?? 0 
                    });
                }
                else
                {
                    var interaction = new Interaction
                    {
                        PostId = postId,
                        InteractionUserId = userId,
                        Likenum = 1,
                        Commentnum = 0,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _context.Interactions.Add(interaction);
                    await _context.SaveChangesAsync();
                    
                    var post = await _context.Posts.FindAsync(postId);
                    return Ok(new { 
                        success = true, 
                        liked = true, 
                        likeCount = post?.LikeCount ?? 0 
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ CHECK IF USER LIKED POST
        [HttpGet("check-like/{postId}")]
        public async Task<IActionResult> CheckLike(int postId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                var liked = await _context.Interactions
                    .AnyAsync(i => i.PostId == postId && 
                                  i.InteractionUserId == userId && 
                                  i.Likenum == 1);
                
                return Ok(new { liked });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ✅ GET ALL USERS WHO LIKED A POST
        [AllowAnonymous]
        [HttpGet("likes/{postId}")]
        public async Task<IActionResult> GetLikes(int postId)
        {
            try
            {
                var likes = await _context.Interactions
                    .Where(i => i.PostId == postId && i.Likenum == 1)
                    .Join(_context.UserTables,
                          interaction => interaction.InteractionUserId,
                          user => user.Id,
                          (interaction, user) => new
                          {
                              userId = user.Id,
                              username = user.Name,
                              photoPath = user.PhotoPath,
                              likedAt = interaction.CreatedAt
                          })
                    .OrderByDescending(x => x.likedAt)
                    .ToListAsync();

                return Ok(new { success = true, likes });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ ADD COMMENT
        [HttpPost("comment/{postId}")]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                if (string.IsNullOrWhiteSpace(request.CommentText))
                {
                    return BadRequest(new { success = false, message = "Comment cannot be empty" });
                }

                var interaction = new Interaction
                {
                    PostId = postId,
                    InteractionUserId = userId,
                    Likenum = 0,
                    Commentnum = 1,
                    CommentText = request.CommentText,
                    CreatedAt = DateTime.UtcNow
                };
                
                _context.Interactions.Add(interaction);
                await _context.SaveChangesAsync();
                
                var post = await _context.Posts.FindAsync(postId);
                
                // Get user info for the response
                var user = await _context.UserTables.FindAsync(userId);
                
                return Ok(new { 
                    success = true, 
                    commentCount = post?.CommentCount ?? 0,
                    comment = new
                    {
                        interactionId = interaction.InteractionId,
                        userId = user?.Id,
                        username = user?.Name,
                        photoPath = user?.PhotoPath,
                        commentText = interaction.CommentText,
                        createdAt = interaction.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ GET ALL COMMENTS FOR A POST
        [AllowAnonymous]
        [HttpGet("comments/{postId}")]
        public async Task<IActionResult> GetComments(int postId)
        {
            try
            {
                var comments = await _context.Interactions
                    .Where(i => i.PostId == postId && i.Commentnum == 1)
                    .Join(_context.UserTables,
                          interaction => interaction.InteractionUserId,
                          user => user.Id,
                          (interaction, user) => new
                          {
                              interactionId = interaction.InteractionId,
                              userId = user.Id,
                              username = user.Name,
                              photoPath = user.PhotoPath,
                              commentText = interaction.CommentText,
                              createdAt = interaction.CreatedAt
                          })
                    .OrderByDescending(x => x.createdAt)
                    .ToListAsync();

                return Ok(new { success = true, comments });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ DELETE COMMENT (only owner can delete)
        [HttpDelete("comment/{interactionId}")]
        public async Task<IActionResult> DeleteComment(int interactionId)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                
                var interaction = await _context.Interactions.FindAsync(interactionId);
                
                if (interaction == null)
                {
                    return NotFound(new { success = false, message = "Comment not found" });
                }
                
                if (interaction.InteractionUserId != userId)
                {
                    return Unauthorized(new { success = false, message = "You can only delete your own comments" });
                }

                _context.Interactions.Remove(interaction);
                await _context.SaveChangesAsync();
                
                var post = await _context.Posts.FindAsync(interaction.PostId);
                
                return Ok(new { 
                    success = true, 
                    commentCount = post?.CommentCount ?? 0,
                    message = "Comment deleted successfully" 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class CommentRequest
    {
        public string? CommentText { get; set; }
    }
}
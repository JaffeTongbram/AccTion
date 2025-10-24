using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AccTion.Models;
using Microsoft.AspNetCore.Authorization;

namespace AccTion.Controllers

{    
    
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionTypesController : ControllerBase
    {
        private readonly PostgresContext _context;

        public SubscriptionTypesController(PostgresContext context)
        {
            _context = context;
        }

        // ✅ GET: api/SubscriptionTypes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SubscriptionType>>> GetSubscriptionTypes()
        {
            return await _context.SubscriptionTypes.ToListAsync();
        }

        // ✅ GET: api/SubscriptionTypes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SubscriptionType>> GetSubscriptionType(int id)
        {
            var subscriptionType = await _context.SubscriptionTypes.FindAsync(id);

            if (subscriptionType == null)
            {
                return NotFound();
            }

            return subscriptionType;
        }

        // ✅ POST: api/SubscriptionTypes
        [HttpPost]
        public async Task<ActionResult<SubscriptionType>> PostSubscriptionType(SubscriptionType subscriptionType)
        {
            _context.SubscriptionTypes.Add(subscriptionType);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetSubscriptionType), new { id = subscriptionType.Id }, subscriptionType);
        }

        // ✅ PUT: api/SubscriptionTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubscriptionType(int id, SubscriptionType subscriptionType)
        {
            if (id != subscriptionType.Id)
            {
                return BadRequest("ID mismatch.");
            }

            _context.Entry(subscriptionType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.SubscriptionTypes.Any(e => e.Id == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // ✅ DELETE: api/SubscriptionTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubscriptionType(int id)
        {
            var subscriptionType = await _context.SubscriptionTypes.FindAsync(id);
            if (subscriptionType == null)
            {
                return NotFound();
            }

            _context.SubscriptionTypes.Remove(subscriptionType);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}

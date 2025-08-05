using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using habitaai.webapi.domain;

namespace habitaai.webapi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PropertiesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PropertiesController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Property>>> GetAll() =>
            await _context.Properties.OrderByDescending(p => p.CreatedAt).ToListAsync();

        [HttpGet("{id}")]
        public async Task<ActionResult<Property>> Get(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            return property is null ? NotFound() : Ok(property);
        }

        [HttpPost]
        public async Task<ActionResult<Property>> Create(Property property)
        {
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = property.Id }, property);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Property updated)
        {
            if (id != updated.Id) return BadRequest();
            _context.Entry(updated).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property is null) return NotFound();
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}

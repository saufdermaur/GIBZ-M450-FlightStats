using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigsController : ControllerBase
    {
        private readonly FlightStatsDbContext _context;

        public ConfigsController(FlightStatsDbContext context)
        {
            _context = context;
        }

        // GET: api/Configs
        [HttpGet]
        public async Task<IActionResult> GetConfigs()
        {
            List<Config> configs = await _context.Configs.ToListAsync();
            return Ok(configs);
        }

        // GET: api/Configs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfig(int id)
        {
            Config? config = await _context.Configs
                .FirstOrDefaultAsync(m => m.ConfigId == id);

            if (config == null)
            {
                return NotFound();
            }

            return Ok(config);
        }

        // POST: api/Configs
        [HttpPost]
        public async Task<IActionResult> CreateConfig([FromBody] Config config)
        {
            if (ModelState.IsValid)
            {
                _context.Configs.Add(config);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetConfig), new { id = config.ConfigId }, config);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/Configs/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfig(int id, [FromBody] Config config)
        {
            if (id != config.ConfigId)
            {
                return BadRequest();
            }

            _context.Entry(config).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConfigExists(id))
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

        // DELETE: api/Configs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            Config? config = await _context.Configs.FindAsync(id);
            if (config == null)
            {
                return NotFound();
            }

            _context.Configs.Remove(config);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ConfigExists(int id)
        {
            return _context.Configs.Any(e => e.ConfigId == id);
        }
    }
}

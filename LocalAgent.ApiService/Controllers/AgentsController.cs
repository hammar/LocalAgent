using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocalAgent.ApiService.Data;
using LocalAgent.ApiService.Models;

namespace LocalAgent.ApiService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AgentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Agent>>> GetAgents()
        {
            return await _context.Agents.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Agent>> GetAgent(Guid id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();
            return agent;
        }

        [HttpPost]
        public async Task<ActionResult<Agent>> CreateAgent(Agent agent)
        {
            agent.Id = Guid.NewGuid();
            _context.Agents.Add(agent);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, agent);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgent(Guid id, Agent agent)
        {
            if (id != agent.Id) return BadRequest();
            _context.Entry(agent).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Agents.Any(e => e.Id == id))
                    return NotFound();
                else
                    throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(Guid id)
        {
            var agent = await _context.Agents.FindAsync(id);
            if (agent == null) return NotFound();
            _context.Agents.Remove(agent);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

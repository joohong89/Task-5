#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cart.Models;

namespace Cart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartMastersController : ControllerBase
    {
        private readonly CartContext _context;

        public CartMastersController(CartContext context)
        {
            _context = context;
        }

        // GET: api/CartMasters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CartMaster>>> GetCarts()
        {
            return await _context.Carts.ToListAsync();
        }

        // GET: api/CartMasters/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CartMaster>> GetCartMaster(int id)
        {
            var cartMaster = await _context.Carts.FindAsync(id);

            if (cartMaster == null)
            {
                return NotFound();
            }

            return cartMaster;
        }

        // PUT: api/CartMasters/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCartMaster(int id, CartMaster cartMaster)
        {
            if (id != cartMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(cartMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartMasterExists(id))
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

        // POST: api/CartMasters
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CartMaster>> PostCartMaster(CartMaster cartMaster)
        {
            _context.Carts.Add(cartMaster);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCartMaster", new { id = cartMaster.Id }, cartMaster);
        }

        // DELETE: api/CartMasters/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCartMaster(int id)
        {
            var cartMaster = await _context.Carts.FindAsync(id);
            if (cartMaster == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cartMaster);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartMasterExists(int id)
        {
            return _context.Carts.Any(e => e.Id == id);
        }
    }
}

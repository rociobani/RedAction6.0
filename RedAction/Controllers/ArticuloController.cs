using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RedAction.Models;

namespace RedAction.Controllers
{
    public class ArticuloController : Controller
    {
        private readonly RedActionDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ArticuloController(RedActionDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [Authorize (Roles = "JEFE_DE_REDACCION")]
        // GET: Articulo
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Articulo.ToListAsync(); 
                                                                                                                             // ver que pasa si el autor es el Administrador
            return View(lista);
        }

        [Authorize(Roles = "REDACTOR")]
        [HttpGet("Usuarios/ArticulosPropios")]
        public async Task<IActionResult> ArticulosPropios()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || _context.Usuario == null)
            {
                return NotFound();
            }

            var lista = await _context.Articulo.Where(c => c.autor.mail == user.NormalizedEmail).ToListAsync();


            return View("Index", lista);

        }











        // GET: Articulo/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Articulo == null)
            {
                return NotFound();
            }

            var articulo = await _context.Articulo
                .Include(a => a.autor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (articulo == null)
            {
                return NotFound();
            }

            return View(articulo);
        }

        // GET: Articulo/Create
        public IActionResult Create()
        {
            ViewData["AutorId"] = new SelectList(_context.Usuario, "Id", "nombreCompleto");
            return View();
        }

        // POST: Articulo/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,AutorId,contenido,seccion,observaciones")] Articulo articulo)
        {
            if (ModelState.IsValid)
            {
                articulo.estado = EstadoArticulo.BORRADOR;
                _context.Add(articulo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ArticulosPropios));
            }
            ViewData["AutorId"] = new SelectList(_context.Usuario, "Id", "Dni", articulo.AutorId);
            return View(articulo);
        }

        // GET: Articulo/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Articulo == null)
            {
                return NotFound();
            }

            var articulo = await _context.Articulo.FindAsync(id);
            if (articulo == null)
            {
                return NotFound();
            }
            ViewData["AutorId"] = new SelectList(_context.Usuario, "Id", "Dni", articulo.AutorId);
            return View(articulo);
        }

        // POST: Articulo/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AutorId,contenido,seccion,estado,observaciones")] Articulo articulo)
        {
            if (id != articulo.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(articulo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArticuloExists(articulo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["AutorId"] = new SelectList(_context.Usuario, "Id", "Dni", articulo.AutorId);
            return View(articulo);
        }

        // GET: Articulo/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Articulo == null)
            {
                return NotFound();
            }

            var articulo = await _context.Articulo
                .Include(a => a.autor)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (articulo == null)
            {
                return NotFound();
            }

            return View(articulo);
        }

        // POST: Articulo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Articulo == null)
            {
                return Problem("Entity set 'RedActionDBContext.Articulo'  is null.");
            }
            var articulo = await _context.Articulo.FindAsync(id);
            if (articulo != null)
            {
                _context.Articulo.Remove(articulo);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(ArticulosPropios));
        }

        private bool ArticuloExists(int id)
        {
          return (_context.Articulo?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

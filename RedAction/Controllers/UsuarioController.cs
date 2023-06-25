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
    public class UsuarioController : Controller
    {
        private readonly RedActionDBContext _context;
        //Agregamos el IdentityUser tanto en atributos como en Constructor
        private readonly UserManager<IdentityUser> _userManager;

        public UsuarioController(RedActionDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Usuario/Index --> Como esto muestra la lista COMPLETA de usuarios, sólo quiero que lo vea el Jefe de Redaccion
        [Authorize(Roles = "JDR")]
        public async Task<IActionResult> Index()
        {
              return _context.Usuario != null ? 
                          View(await _context.Usuario.ToListAsync()) :
                          Problem("Entity set 'RedActionDBContext.Usuario'  is null.");
        }

        // GET: Usuario/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Usuario == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuario/Create
        // Modificamos los parámetros para que reciba un user (de tipo IdentityUser)
        // Si el user es null --> Mensaje error
        // Sino crea un nuevo usuario y le setea el email
        public IActionResult Create(IdentityUser? user)
        {
            if (user == null) return RedirectToAction("MensajeError", "Home");

            Usuario usuario = new Usuario
            {
                mail = user.Email
            };
            
            return View(usuario);
        }

        // POST: Usuario/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Dni,nombreCompleto,mail")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                //TO-DO Falta modificar métodos de creación de usuario
                if (await UsuarioDuplicado(usuario.Dni))
                {
                    return RedirectToAction("MensajeError", "Home");
                }

                usuario.nomUsuario = usuario.Dni;
                usuario.pass = usuario.Dni;
                usuario.tipo = TipoUsuario.REDACTOR;
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        // GET: Usuario/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Usuario == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return View(usuario);
        }

        // POST: Usuario/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Dni,nombreCompleto,mail,tipo,nomUsuario,pass")] Usuario usuario)
        {
            if (id != usuario.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
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
            return View(usuario);
        }

        // GET: Usuario/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Usuario == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuario/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Usuario == null)
            {
                return Problem("Entity set 'RedActionDBContext.Usuario'  is null.");
            }
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuario.Remove(usuario);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
          return (_context.Usuario?.Any(e => e.Id == id)).GetValueOrDefault();
        }
        private async Task<bool> UsuarioDuplicado(string dni)
        {
            var resultado = false;
            var usuario = await _context.Usuario.Where(u => u.Dni.Equals(dni)).FirstOrDefaultAsync();
            if (usuario != null)
            {
                resultado = true;
            }

            return resultado;
        }

        public async Task<bool> EsJefeDeRedaccion(int id)
        {
            var resultado = false;
            var jdr = await _context.Usuario.FindAsync(id);
            if (jdr != null && jdr.tipo == TipoUsuario.JEFE_DE_REDACCION)
            {
                resultado = true;
            }

            return resultado;
        }

    }
}

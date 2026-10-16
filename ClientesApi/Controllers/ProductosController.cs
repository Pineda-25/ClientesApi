using ClientesApi.Data;
using ClientesApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Productos>>> Listar()
        {
            var productos = await _context.Productos.AsNoTracking().ToListAsync();
            return Ok(productos);
        }

        //Get : api/productos/1 -> Busca productos por su id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Productos>> ObtenerPorId(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                //error 404
                return NotFound(new { mensaje = $"No existe el producto con ID : {id}" });
            }

            //Ok (metodo 200)
            return Ok(producto);
        }

        //POST : api/prodcutos -> registrar
        [HttpPost]
        public async Task<ActionResult<Productos>> Registrar([FromBody] Productos producto)
        {
            //Evitar que el cliente envie un ID manualmente 
            producto.Id = 0;

            //Validar que el numserie no esta repetido (solo si numserie no viene nulo o vacio)
            if (!string.IsNullOrEmpty(producto.Numserie) &&
                await _context.Productos.AnyAsync(p => p.Numserie == producto.Numserie))
            {
                //Error 409
                return Conflict(new { mensaje = "Ya existe un producto con ese numero de serie " });
            }

            //Procedemos al registro 
            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            //Server code 201 (save ok) - Ejecutamos en segundo plano el buscador por id , pasamos el id entrada , modelo 
            return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id }, producto);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Productos producto)
        {
            // Validacion simple para el Id
            if (id != producto.Id)
            {
                //Server code 400
                return BadRequest(new { mensaje = "El ID de la url no coincide con el id del cuerpo " });
            }

            //Validacion 2: ¿Existe el id?
            var existente = await _context.Productos.FindAsync(id); //FindAsync buscador por el key 

            if (existente == null)
            {
                //server cod 404
                return NotFound(new { mensaje = $"No existe producto con id: {id}" });
            }

            //Validacion 3: conflicto numserie repetido (solo si viene un numero de serie)
            if (!string.IsNullOrEmpty(producto.Numserie) &&
                await _context.Productos.AnyAsync(p => p.Numserie == producto.Numserie && p.Id != id))
            {
                //Error 409
                return Conflict(new { mensaje = "Ya existe un producto con ese numero de serie " });
            }

            //Procedemos a realizar la actualizacion
            existente.Nombre = producto.Nombre;
            existente.Descripcion = producto.Descripcion;
            existente.Precio = producto.Precio;
            existente.Garantia = producto.Garantia;
            existente.Numserie = producto.Numserie;

            //Ahora confirmemos los cambios utilizando el contenedor "existente"
            await _context.SaveChangesAsync(); // esta enlazado a <<<FindAsync>>>

            //Server code 204
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                //server code 404
                return NotFound(new { mensaje = $"No existe producto con ID : {id}" });
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            //Server code 204 (accion ejecutada correctamente)
            return NoContent();
        }

    }
}



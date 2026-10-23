using ClientesApi.Data;
using ClientesApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClientesApi.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        //Todas las funciones GET - POST - PUT - DELETE necesitan acceso a DB (ORM)
        private readonly AppDbContext _context;
        //Constructor (inicializar)
        public ClientesController(AppDbContext context)
        {
            _context = context;
        }

        //Get: api/clientes -> retornar la lista  de clientes 
        //async Task    : asincrono (tiene latencia
        //IENumerable   : define un set de datos (coleccion)
        //ActionResul   : valor a obtner (coleccion definida en el modelo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cliente>>> Listar()
        {
            var clientes = await _context.Clientes.AsNoTracking().ToListAsync();
            return Ok(clientes);
        }

        //Get : api/clientes/1 -> Busca clientes por su id
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Cliente>> ObtenerPorId(int id) 
        {
            var cliente = await _context.Clientes.FindAsync(id);
            
            if(cliente == null)
            {
                //error 404
                return NotFound(new { mensaje = $"No existe el cliente con ID : {id}" });
            }

            //Ok (metodo 200)
            return Ok(cliente);
        }

        //POST : api/clientes -> registrar (no olvidatr en body json 
        [HttpPost]
        public async Task<ActionResult<Cliente>> Registrar([FromBody] Cliente cliente)
        {
            //Evitar que el cliente envie u  ID manualmente 
            cliente.Id = 0;

            //Validar que el DNI no esta repetido...
            if (await _context.Clientes.AnyAsync(c => c.Dni == cliente.Dni))
            {
                //Error 409
                return Conflict(new { mensaje = "Ya existe un cliente con ese dni "});
            }

            //Procedemos al registro 
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            //Server code 201 (save ok) - Ejecutamos en segundo plano el buscador por id , pasamos el id entrada , modelo 
            return CreatedAtAction(nameof(ObtenerPorId), new {    id =  cliente.Id}, cliente);
        }

        //Put : api/cliente/1 -> actualiaar (ademas lleva un JSON en el body)
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] Cliente cliente)
        {

            //  Validacion simple para el Id
            if (id != cliente.Id)
            {
                //Serve code 400
                return BadRequest(new { mensaje = "El ID de la url no coincide con el id del cuerpo "});
            }

            //Validacion 2: ¿Existe el id?
            var existente =  await _context.Clientes.FindAsync(id); //FindAsync  buscador por el key 

            if (existente == null)
            {
                //server cod 404
                return NotFound(new { mensaje = $"No existe cliente con id: {id}" });
            }

            //Validacion 3: conflicto DNI repetido 
            if (await _context.Clientes.AnyAsync(c => c.Dni == cliente.Dni && c.Id != id))
            {
                //Error 409
                return Conflict(new { mensaje = "Ya existe un cliente con ese dni " });
            }

            //Procedemos a realizar la actualizacion
            existente.Apellidos = cliente.Apellidos;
            existente.Nombres = cliente.Nombres;
            existente.Telefono = cliente.Telefono;
            existente.Correo = cliente.Correo;
            existente.Dni = cliente.Dni;
            existente.Direccion = cliente.Direccion;

            //Ahora confirmemos los ca,mbios utlizando el contenedor "existente"
            await _context.SaveChangesAsync(); // esta enlazado  a <<<FindAsysnc>>>

            //Server code 204
            return NoContent();
        }

        //DELETE: api/clientes/1 -> eliminar
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                //server code 404
                return NotFound(new { mensaje = $"No existe cliente con ID : {id}" });
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            //Server code 204 (accion ejecutada correctamente)
            return NoContent();
        }

    }
}

using ClientesApi.Data;
using ClientesApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using System.Security.Claims;

namespace ClientesApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context; //Acceso a la base datos
        private readonly IConfiguration _config; //Configuracion

        //Constructor 
        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        [HttpPost("registrar")]
        //Registrar
        //POST: api/auth/registrar
        public async Task<IActionResult> Registrar([FromBody] RegistroRequest request)
        {
            //Si el usuario ya existe, cancelamos el proceso
            if (await _context.Usuarios.AnyAsync(u => u.Username == request.Username))
            {
                //Serve code 409
                return Conflict(new { mensaje = "El Usuario ya existe" });
            }

            //Sino existe , creamos uno nuevo utlizando authDTos
            var usuario = new Usuario
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Rol = "Usuario"
            };

            _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario registrado correctamente " });

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(U => U.Username == request.Username);
            
            //La constrasea se desencripta?
            //No , se utiliza un algoritmo matematico para verficar su autenciadad
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            {
                //Server code 401
                return Unauthorized(new { mensaje = "Usuario o contrasea es incorrecto no fue encontrado"});
            }

            //var => declarancion denamica objetos
            //Deseralizacion
            var (token, expira) = GenerarToken(usuario);

            return Ok(new { token, expira });
        }

        //El metodo retornara:
        //token:            cadena larga de caracteres
        //fecha hora:       vigencia del token
        private (string token, DateTime expira) GenerarToken(Usuario usuario)
        {
            //Json Web Token
            var jwt = _config.GetSection("Jwt");

            //Nota: Los paramentros [Param] vienen del archuivo de configuracion
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));
            var expira = DateTime.UtcNow.AddMinutes(double.Parse(jwt["ExpireMinutes"]!));

            //  CONFIGURACIONES DEL ALGORIMTO QUE IMTE EL CERTIIFCadio y la firma de seguridad
            var descriptor = new SecurityTokenDescriptor
            {
                //IDusuario, Nombre, Rol, Permiso
               Subject = new ClaimsIdentity(new[] 
               {
                   new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                   new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Username),
                   new Claim(ClaimTypes.Role, usuario.Rol),
                   new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
               }),
               Issuer = jwt["Issuer"],
               Audience = jwt["Audience"],
               Expires = expira,
               SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };

            var token = new JsonWebTokenHandler().CreateToken(descriptor);
            return (token, expira);
        }
    }
}

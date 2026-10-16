using ClientesApi.Models;  //Modelo (Definicion tabla)
using Microsoft.EntityFrameworkCore; //Acceder DB

namespace ClientesApi.Data
{
    //Herencia => subClase : superClase
    public class AppDbContext : DbContext
    {
        //  Constructor (inicializar)
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        //ORM tendra acceso al modelo cliente
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Productos> Productos { get; set; }
    }
}

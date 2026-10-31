using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClientesApi.Models
{
    [Table("Proveedores")]
    public class Proveedor
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required, StringLength(11)]
        public string Ruc { get; set; } = string.Empty;

        [StringLength(15)]
        public string? Telefono { get; set; }

        [StringLength(100)]
        public string? Correo { get; set; }
    }
}

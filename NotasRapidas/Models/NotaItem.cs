using SQLite;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotasRapidas.Models
{
    [SQLite.Table("Notas")]
    public class NotaItem
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(250)]
        public string TextoNota { get; set; }        
        public DateTime FechaCreacion { get; set; }
        // Indexado para mejorar el redimiento
        // en la búsqueda por Categoría
        [Indexed]
        public int CategoriaId { get; set; }
        // La propiedad CategoriaNombre no se mapea
        // a un campo de la tabla Notas en la base de datos
        // solo se utiliza para mostrar en la vista
        [Ignore]
        public string CategoriaNombre { get; set; }
    }
}

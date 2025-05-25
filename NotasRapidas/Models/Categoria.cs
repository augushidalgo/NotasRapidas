using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotasRapidas.Models
{
    [SQLite.Table("Categorias")]
    public class Categoria
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(100), Unique] // Nombre de la categoría único
        public string Nombre { get; set; }
        // sobreescribimos el método ToString
        // para que devuelva el nombre de la categoría
        public override string ToString()
        {
            return Nombre;
        }

    }
}

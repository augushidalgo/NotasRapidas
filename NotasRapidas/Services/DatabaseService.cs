using NotasRapidas.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotasRapidas.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;
        private bool _isInitialized = false;
        private readonly string _dbPath = 
            System.IO.Path.Combine(FileSystem.AppDataDirectory,"NotasSQLite.db3");

        public DatabaseService() { }

        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;

            // Eliminar BD para pruebas limpias durante el desarrollo
            //if (File.Exists(_dbPath))
            //{
            //     File.Delete(_dbPath);
            //}

            _database = new SQLiteAsyncConnection(_dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            await _database.CreateTableAsync<NotaItem>();
            await _database.CreateTableAsync<Categoria>();
            _isInitialized = true;
        }

        // Métodos para CRUD de Notas

        // Agregar una nueva Nota
        public async Task<int> AddNotaAsync(NotaItem nota)
        {
            await InitializeAsync();
            return await _database.InsertAsync(nota);
        }

        // Obtener todas las Notas
        public async Task<List<NotaItem>> GetNotasAsync()
        {
            await InitializeAsync();

            return await _database.Table<NotaItem>().OrderByDescending(n => n.FechaCreacion).ToListAsync();
        }

        // Obtener una Nota por Id
        public async Task<NotaItem> GetNotaAsync(int id)
        {
            await InitializeAsync();
            return await _database.Table<NotaItem>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        // Actualizar una Nota
        public async Task<int> UpdateNotaAsync(NotaItem nota)
        {
            await InitializeAsync();
            return await _database.UpdateAsync(nota);
        }

        // Eliminar una Nota
        public async Task<int> DeleteNotaAsync(NotaItem nota)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(nota);
        }

        // Métodos para CRUD de Categorías

        // Agregar una nueva Categoría
        public async Task<int> AddCategoriaAsync(Categoria categoria)
        {
            await InitializeAsync();
            try
            {
                return await _database.InsertAsync(categoria);
            }
            catch (SQLiteException ex) when (ex.Message.ToLower().Contains("unique constraint failed"))
            {
                // Manejo de excepción para nombre de categoría duplicado
                // Lanzamos una excepción personalizada
                Console.WriteLine($"Error: Categoría '{categoria.Nombre}' ya existe. {ex.Message}");
                return -1; // Indicamos que hubo un error
            }
        }

        // Obtener todas las Categorías
        public async Task<List<Categoria>> GetCategoriasAsync()
        {
            await InitializeAsync();
            return await _database.Table<Categoria>().OrderBy(c => c.Nombre).ToListAsync();
        }

        // Obtener una Categoría por Id
        public async Task<Categoria> GetCategoriaAsync(int id)
        {
            await InitializeAsync();
            return await _database.Table<Categoria>().Where(c => c.Id == id).FirstOrDefaultAsync();
        }

        // Actualizar una Categoría
        public async Task<int> UpdateCategoriaAsync(Categoria categoria)
        {
            await InitializeAsync();
            return await _database.UpdateAsync(categoria);
        }

        // Eliminar una Categoría
        public async Task<int> DeleteCategoriaAsync(Categoria categoria)
        {
            await InitializeAsync();
            return await _database.DeleteAsync(categoria);
        }
    }
}

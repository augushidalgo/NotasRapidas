using NotasRapidas.Models;
using NotasRapidas.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NotasRapidas
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService;
        public ObservableCollection<NotaItem> Notas { get; set; }
        // Para el picker de categorías
        public ObservableCollection<Categoria> Categorias { get; set; }
        private NotaItem _notaSeleccionadaParaEditar;
        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
            Notas = new ObservableCollection<NotaItem>();
            Categorias = new ObservableCollection<Categoria>();

            NotasCollectionView.ItemsSource = Notas;
            CategoriaPicker.ItemsSource = Categorias;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarCategoriasAsync();
            await CargarNotasAsync();
            StatusLabel.Text = "Bienvenido a Notas con Caetegorías.";

            // Preseleccionamos la primera categoría si existe
            // o maneja el caso sin categorías
            if (Categorias.Any())
            {
                CategoriaPicker.SelectedIndex = 0;
            }
            else
            {
                StatusLabel.Text = "Añade una categoría para empezar.";
            }
        }

        private async Task CargarCategoriasAsync()
        {
            var categoriasDb = await _databaseService.GetCategoriasAsync();
            Categorias.Clear();
            if (!categoriasDb.Any()) // Si no hay categorías añadimos una por defecto
            {
                await _databaseService.AddCategoriaAsync(new Categoria { Nombre = "Trabajo" });
                await _databaseService.AddCategoriaAsync(new Categoria { Nombre = "Personal" });
                await _databaseService.AddCategoriaAsync(new Categoria { Nombre = "Estudio" });
                categoriasDb = await _databaseService.GetCategoriasAsync(); // Recargamos las categorías
            }

            foreach (var categoria in categoriasDb)
            {
                Categorias.Add(categoria);
            }
            if (Categorias.Any() && CategoriaPicker.SelectedIndex == -1)
            {
                CategoriaPicker.SelectedIndex = 0; // Selecciona la primera categoría por defecto
            }
        }

        private async Task CargarNotasAsync()
        {
            var notasDb = await _databaseService.GetNotasAsync();
            Notas.Clear();
           
            foreach (var nota in notasDb)
            {
                var categoria = Categorias.FirstOrDefault(c => c.Id == nota.CategoriaId);
                nota.CategoriaNombre = categoria?.Nombre ?? "Sin categoría";
                Notas.Add(nota);
            }
        }

        private async void OnAddCategoriaClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NuevaCategoriaEntry.Text))
            {
                await DisplayAlert("Error", "El nombre de la categoría no puede estar vacío.", "OK");
                return;
            }

            var nuevaCategoria = new Categoria { Nombre = NuevaCategoriaEntry.Text.Trim() };
            int result = await _databaseService.AddCategoriaAsync(nuevaCategoria);

            if (result == -1) // Indica que la categoría ya existe
            {
                await DisplayAlert("Duplicado", $"La categoría '{nuevaCategoria.Nombre}' ya existe.", "OK");
            }
            else if (result > 0) // Se añadió correctamente
            {
                NuevaCategoriaEntry.Text = string.Empty;
                await CargarCategoriasAsync();
                var catAdded = Categorias.FirstOrDefault(c => c.Nombre == nuevaCategoria.Nombre);
                if (catAdded != null)
                {
                    CategoriaPicker.SelectedItem = catAdded; // Selecciona la nueva categoría
                }

                StatusLabel.Text = $"Categoría '{nuevaCategoria.Nombre}' añadida.";
            }
            else
            {
                await DisplayAlert("Error", "No se pudo añadir la categoría.", "OK");
            }
        }

        private async void OnAddNotaClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NotaEntry.Text))
            {
                await DisplayAlert("Entrada Vacía", "El texto de la nota no puede estar vacío.", "OK");
                return;
            }

            if (CategoriaPicker.SelectedItem == null)
            {
                await DisplayAlert("Sin Categoría", "Por favor, selecciona una categoría para la nota.", "OK");
                return;
            }

            var categoriaSeleccionada = (Categoria)CategoriaPicker.SelectedItem;

            var nuevaNota = new NotaItem
            {
                TextoNota = NotaEntry.Text,
                FechaCreacion = DateTime.Now,
                CategoriaId = categoriaSeleccionada.Id
            };

            await _databaseService.AddNotaAsync(nuevaNota);
            StatusLabel.Text = "Nota añadida con éxito.";
            NotaEntry.Text = string.Empty;
            // CategoriaPicker.SelectedIndex = 0; // Opcional: resetear picker
            await CargarNotasAsync();
        }
        private async void OnUpdateNotaClicked(object sender, EventArgs e)
        {
            if (_notaSeleccionadaParaEditar == null) return;

            if (string.IsNullOrWhiteSpace(NotaEntry.Text))
            {
                await DisplayAlert("Entrada Vacía", "El texto de la nota no puede estar vacío.", "OK");
                return;
            }
            if (CategoriaPicker.SelectedItem == null)
            {
                await DisplayAlert("Sin Categoría", "Por favor, selecciona una categoría para la nota.", "OK");
                return;
            }

            var categoriaSeleccionada = (Categoria)CategoriaPicker.SelectedItem;

            _notaSeleccionadaParaEditar.TextoNota = NotaEntry.Text;
            _notaSeleccionadaParaEditar.CategoriaId = categoriaSeleccionada.Id;
            // _notaSeleccionadaParaEditar.FechaCreacion = DateTime.Now; // Opcional: actualizar fecha de modificación

            await _databaseService.UpdateNotaAsync(_notaSeleccionadaParaEditar);
            StatusLabel.Text = "Nota actualizada con éxito.";

            NotaEntry.Text = string.Empty;
            UpdateButton.IsEnabled = false;
            AddButton.IsEnabled = true;
            if (Categorias.Any()) CategoriaPicker.SelectedIndex = 0;
            _notaSeleccionadaParaEditar = null;
            await CargarNotasAsync();
        }
        private async void OnEditNotaClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is NotaItem notaParaEditar)
            {
                _notaSeleccionadaParaEditar = notaParaEditar;
                NotaEntry.Text = notaParaEditar.TextoNota;

                // Seleccionar la categoría correcta en el Picker
                var categoriaDeNota = Categorias.FirstOrDefault(c => c.Id == notaParaEditar.CategoriaId);
                if (categoriaDeNota != null)
                {
                    CategoriaPicker.SelectedItem = categoriaDeNota;
                }
                else if (Categorias.Any()) // Si no se encuentra la categoría, selecciona la primera
                {
                    CategoriaPicker.SelectedIndex = 0;
                }


                UpdateButton.IsEnabled = true;
                AddButton.IsEnabled = false;
                StatusLabel.Text = $"Editando nota ID: {notaParaEditar.Id}";
            }
        }

        private async void OnDeleteNotaClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is NotaItem notaParaEliminar)
            {
                bool confirm = await DisplayAlert("Confirmar Eliminación",
                                                  $"¿Eliminar nota: \"{notaParaEliminar.TextoNota}\"?",
                                                  "Sí, Eliminar", "No, Cancelar");
                if (confirm)
                {
                    await _databaseService.DeleteNotaAsync(notaParaEliminar);
                    StatusLabel.Text = "Nota eliminada.";
                    await CargarNotasAsync();

                    if (_notaSeleccionadaParaEditar != null && _notaSeleccionadaParaEditar.Id == notaParaEliminar.Id)
                    {
                        NotaEntry.Text = string.Empty;
                        UpdateButton.IsEnabled = false;
                        AddButton.IsEnabled = true;
                        if (Categorias.Any()) CategoriaPicker.SelectedIndex = 0;
                        _notaSeleccionadaParaEditar = null;
                    }
                }
            }
        }
    }
}

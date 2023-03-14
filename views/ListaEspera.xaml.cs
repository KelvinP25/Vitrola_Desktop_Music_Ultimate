using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Vitrola_Desktop_Music_Ultimate.models;

namespace Vitrola_Desktop_Music_Ultimate.views
{
    /// <summary>
    /// Interaction logic for ListaEspera.xaml
    /// </summary>


    public partial class ListaEspera : Window
    {
        private ObservableCollection<Vitrola> _vitrola = new ObservableCollection<Vitrola>();
        public List<Vitrola> vitrolas;
        public ListaEspera()
        {
            InitializeComponent();
            songListView.ItemsSource = _vitrola;
            Loaded += GetVitrola;

        }
        private async void GetVitrola(object sender, RoutedEventArgs e)
        {
            string url = "http://127.0.0.1:8000/myapp/music/"; // URL de la API
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(); // obtener la respuesta de la API como una cadena de caracteres
                JObject jsonObject = JObject.Parse(responseBody); // convertir la cadena de caracteres en un objeto JSON

                if (jsonObject.ContainsKey("music"))
                {
                    JArray jsonArray = (JArray)jsonObject["music"]; // obtener el arreglo JSON que contiene los objetos Vitrola

                    vitrolas = jsonArray.ToObject<List<Vitrola>>(); // deserializar el arreglo JSON en una lista de objetos Vitrola

                    foreach (Vitrola vitrola in vitrolas)
                    {
                        _vitrola.Add(vitrola); // agregar el objeto Vitrola a la lista observable
                    }
                }
                else
                {
                    MessageBox.Show("La respuesta de la API no tiene el formato esperado.");
                }
            }
            else
            {
                MessageBox.Show("Error al obtener los datos de la API.");
            }
        }
        private int select()
        {
            int id = 0;
            if (songListView.SelectedItem != null)
            {
                Vitrola selectedSong = (Vitrola)songListView.SelectedItem;
                id = selectedSong.id;
                // Aquí puede hacer lo que desee con el ID seleccionado
            }
            return id;
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnVitrola_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btn_Agregar_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el objeto Vitrola seleccionado del ListView
            var selectedVitrola = _vitrola.FirstOrDefault(v => v.id == select());

            // Crear un objeto HttpClient
            HttpClient client = new HttpClient();

            // Crear un objeto HttpContent con los datos del objeto Vitrola seleccionado
            var content = new StringContent(JsonConvert.SerializeObject(selectedVitrola), Encoding.UTF8, "application/json");

            // Realizar una solicitud POST
            var result = await client.PostAsync("http://127.0.0.1:8000/myapp/waitlist/", content);

            // Leer la respuesta como una cadena
            string responseString = await result.Content.ReadAsStringAsync();
        }
     
    }
}
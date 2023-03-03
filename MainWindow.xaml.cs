using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Linq;
using Vitrola_Desktop_Music_Ultimate.models;
using NAudio.Wave;
using System.Net;

namespace Vitrola_Desktop_Music_Ultimate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<Vitrola> _vitrola = new ObservableCollection<Vitrola>();
        public List<Vitrola> vitrolas;
        private MediaPlayer _mediaPlayer;

        public MainWindow()
        {
            InitializeComponent();
            songListView.ItemsSource = _vitrola;
            Loaded += MainWindow_Loaded;
        }
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
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
        private WaveOut PlayAudio()
        {
            int id = 0;
            if (songListView.SelectedItem != null)
            {
                Vitrola selectedSong = (Vitrola)songListView.SelectedItem;
                id = selectedSong.id;
                // Aquí puede hacer lo que desee con el ID seleccionado
            }

            string track = vitrolas.Where(p => p.id == id).Select(p => p.track).FirstOrDefault();
            byte[] audioBytes;
            string audioUrl = $"http://127.0.0.1:8000/media/{track}";

            using (WebClient webClient = new WebClient())
            {
                audioBytes = webClient.DownloadData(audioUrl);
            }
            MemoryStream audioStream = new MemoryStream(audioBytes);

            // Crear un objeto WaveStream a partir del MemoryStream
            WaveStream waveStream = new Mp3FileReader(audioStream);

            // Crear un objeto WaveOut para reproducir el archivo
            WaveOut waveOut = new WaveOut();
            waveOut.Init(waveStream);
            return waveOut;
        }
        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            
            if (playIcon.Kind == MaterialDesignThemes.Wpf.PackIconKind.Play)
            {
                // Cambiar el icono a "Pause"
                playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Pause;
                // Agregar aquí la lógica de reproducción
            }
            else
            {
                // Cambiar el icono a "Play"

                playIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Play;
                // Agregar aquí la lógica de pausa
            }
        }

        private void btnFile_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

﻿using Newtonsoft.Json.Linq;
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
using System.Windows.Threading;
using System.Diagnostics;
using System.Timers;
using Vitrola_Desktop_Music_Ultimate.views;
using System.Reflection.PortableExecutable;
using Newtonsoft.Json;

namespace Vitrola_Desktop_Music_Ultimate
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {

        private ObservableCollection<WaitList> _waitList = new ObservableCollection<WaitList>();
        public List<WaitList> waitLists;

        private ObservableCollection<Vitrola> _vitrola = new ObservableCollection<Vitrola>();
        public List<Vitrola> vitrolas;

        private WaveStream waveStream;
        private WaveOut waveOut;

        private int currentTrackIndex = 0;
        private readonly int interval = 3000; // 3 segundos
        private System.Timers.Timer timer;
        private bool mpaso = true;
        private bool mpaso1 = true;
        private bool HaveMusic = false;
        public MainWindow()
        {
            InitializeComponent();
            songListView.ItemsSource = _waitList;
            Loaded += ListaEspera_Loaded;

            // Crear y configurar el temporizador
            timer = new Timer(interval);
            timer.Elapsed += Timer_Elapsed;
            timer.AutoReset = true;
            timer.Start();
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.InvokeAsync(async () =>
            {
                await RefreshWaitList();
            });
        }

        private async void ListaEspera_Loaded(object sender, RoutedEventArgs e)
        {
            GetVitrola();
            await RefreshWaitList();
           
        }
        private async Task RefreshWaitList()
        {
            string url = "http://127.0.0.1:8000/myapp/waitlist/"; // URL de la API
            HttpClient client = new HttpClient();
            HttpResponseMessage response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(); // obtener la respuesta de la API como una cadena de caracteres
                JObject jsonObject = JObject.Parse(responseBody); // convertir la cadena de caracteres en un objeto JSON

                if (jsonObject.ContainsKey("music"))
                {
                    JArray jsonArray = (JArray)jsonObject["music"]; // obtener el arreglo JSON que contiene los objetos Vitrola

                    waitLists = jsonArray.ToObject<List<WaitList>>(); // deserializar el arreglo JSON en una lista de objetos Vitrola

                    _waitList.Clear();
                    foreach (WaitList waitList in waitLists)
                    {
                        _waitList.Add(waitList); // agregar el objeto Vitrola a la lista observable
                    }

                    // Comenzar la reproducción del primer elemento de la lista de espera
                    if (mpaso)
                    {
                        PlayNextSong();
                        mpaso = false;
                    }
                    HaveMusic = false;
                }
                else
                {
                    _waitList.Clear();
                    if (mpaso1)
                    {
                        PlayRandomSong();
                        mpaso1 = false;
                        HaveMusic = true;
                    }

                }
            }
            else
            {
                MessageBox.Show("Error al obtener los datos de la API.");
            }
        }

        private async void GetVitrola()
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
                    if(waitLists.Count <= 0)
                    {
                        PlayRandomSong();
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
        private string trackWaitList()
        {

            string audioUrl = $"http://127.0.0.1:8000/media/{waitLists[currentTrackIndex].track}";
            return audioUrl;
        }
        private void PlayNextSong()
        {
            // Obtener el índice de la siguiente pista en la lista de espera


            if (currentTrackIndex < waitLists.Count)
            {
                // Descargar el archivo de audio de la API
                byte[] audioBytes;
                using (WebClient webClient = new WebClient())
                {
                    audioBytes = webClient.DownloadData(trackWaitList());
                }
                MemoryStream audioStream = new MemoryStream(audioBytes);

                // Crear un objeto WaveStream a partir del MemoryStream
                waveStream = new Mp3FileReader(audioStream);

                // Crear un objeto WaveOut para reproducir el archivo
                waveOut = new WaveOut();
                waveOut.PlaybackStopped += WaveOut_PlaybackStopped;
                waveOut.Init(waveStream);
                waveOut.Play();
                // Actualizar la interfaz de usuario
                songListView.SelectedIndex = currentTrackIndex;
                currentTrackIndex++; // incrementar el índice para la siguiente canción
            }
            else
            {
                PlayRandomSong();
            }
        }
        private async void PlayRandomSong()
        {
            Random random = new Random();
            int randomTrackIndex = random.Next(0, vitrolas.Count - 1);
            // Obtener el objeto Vitrola seleccionado del ListView
            var selectedVitrola = _vitrola.FirstOrDefault(v => v.id == randomTrackIndex);

            // Crear un objeto HttpClient
            HttpClient client = new HttpClient();

            // Crear un objeto HttpContent con los datos del objeto Vitrola seleccionado
            var content = new StringContent(JsonConvert.SerializeObject(selectedVitrola), Encoding.UTF8, "application/json");

            // Realizar una solicitud POST
            var result = await client.PostAsync("http://127.0.0.1:8000/myapp/waitlist/", content);

            // Leer la respuesta como una cadena
            string responseString = await result.Content.ReadAsStringAsync();


        }
        private async Task<bool> DeleteTrack(int trackId)
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"http://127.0.0.1:8000/myapp/waitlist/{trackId}/";
                HttpResponseMessage response = await client.DeleteAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Error al eliminar la canción de la lista de espera.");
                    return false;
                }
            }
        }
        private async void WaveOut_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_waitList.Count > 0)
            {
                int trackId = _waitList[0].id;
                _waitList.RemoveAt(0); // eliminar la canción que se acaba de reproducir de la lista de espera

                bool deleted = await DeleteTrack(trackId);
                if (_waitList.Count > 0)
                {
                    // obtener la próxima canción y reproducirla
                    PlayNextSong();
                }
                else
                {
                    // no hay canciones restantes, detener la reproducción y mostrar un mensaje al usuario
                    PlayRandomSong();
                }
            }
            else
            {
                PlayRandomSong();
            }
        }
        private void btnFile_Click(object sender, RoutedEventArgs e)
        {
            ListaEspera lis = new ListaEspera();
            lis.Show();
        }

        private void Card_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
      
    }
        
}

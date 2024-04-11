using Newtonsoft.Json;
using WeatherApp1.Models;


namespace WeatherApp1
{

        public partial class MainPage : ContentPage
        {
            private double latitude;
            private double longitude;

            private string _currentLocation;
            public string CurrentLocation
            {
                get { return _currentLocation; }
                set { _currentLocation = value; OnPropertyChanged(); }
            }

            private double _temperature;
            public double Temperature
            {
                get { return _temperature; }
                set { _temperature = value; OnPropertyChanged(); }
            }

            private int _humidity;
            public int Humidity
            {
                get { return _humidity; }
                set { _humidity = value; OnPropertyChanged(); }
            }

            private OpenWeather.Wind _wind;
            public OpenWeather.Wind Wind
            {
                get { return _wind; }
                set { _wind = value; OnPropertyChanged(); }
            }

            private int _pressure;
            public int Pressure
            {
                get { return _pressure; }
                set { _pressure = value; OnPropertyChanged(); }
            }

            private HttpClient _client;

            public MainPage()
            {
                InitializeComponent();
                BindingContext = this;
                _client = new HttpClient();
                _client.DefaultRequestHeaders.Add("Accept", "application/json");

                GetCurrentLocation();
            }

            public async void GetWeather(object parameters)
            {
                string response = await _client.GetStringAsync(new Uri($"https://api.openweathermap.org/data/2.5/weather?lat={latitude}&lon={longitude}&appid=be2b16401dbbbc18a980caf4afe0ba12&units=metric"));
                OpenWeather.Root responseWeather = JsonConvert.DeserializeObject<OpenWeather.Root>(response);

                if (responseWeather != null)
                {
                    Temperature = responseWeather.main.temp;
                    Humidity = responseWeather.main.humidity;
                    Wind = responseWeather.wind;
                    Pressure = responseWeather.main.pressure;
                }
            }

            private CancellationTokenSource _cancelTokenSource;
            private bool _isCheckingLocation;

            public async Task GetCurrentLocation()
            {
                try
                {
                    _isCheckingLocation = true;

                    GeolocationRequest request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));

                    _cancelTokenSource = new CancellationTokenSource();

                    Location location = await Geolocation.GetLocationAsync(request, _cancelTokenSource.Token);

                    if (location != null)
                    {
                        latitude = location.Latitude;
                        longitude = location.Longitude;
                        CurrentLocation = $"Latitude: {latitude}, Longitude: {longitude}";

                        GetWeather(null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting location: {ex.Message}");
                }
                finally
                {
                    _isCheckingLocation = false;
                }
            }

            public void CancelRequest()
            {
                if (_isCheckingLocation && _cancelTokenSource != null && !_cancelTokenSource.IsCancellationRequested)
                    _cancelTokenSource.Cancel();
            }

            private void Button_Clicked(object sender, EventArgs e)
            {
                if (!string.IsNullOrWhiteSpace(entryCity.Text))
                {
                    GetWeatherForCity(entryCity.Text);
                }
            }

            public async void GetWeatherForCity(string city)
            {
                try
                {
                    HttpResponseMessage response = await _client.GetAsync($"https://api.openweathermap.org/data/2.5/weather?q={city}&appid=be2b16401dbbbc18a980caf4afe0ba12&units=metric");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    OpenWeather.Root weatherData = JsonConvert.DeserializeObject<OpenWeather.Root>(responseBody);

                    if (weatherData != null)
                    {
                        Temperature = weatherData.main.temp;
                        Humidity = weatherData.main.humidity;
                        Wind = weatherData.wind;
                        Pressure = weatherData.main.pressure;
                        CurrentLocation = $"City: {weatherData.name}, Country: {weatherData.sys.country}";
                    }
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Error getting weather data: {ex.Message}");
                }
            }
        }
    }


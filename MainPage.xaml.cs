using GeolocatorPlugin.Abstractions;
using GeolocatorPlugin;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Diagnostics;
using Microsoft.Maui.Devices.Sensors;

namespace UPSGeoApp
{
    public partial class MainPage : ContentPage
    {
        private CancellationTokenSource _token;
        private readonly HttpClient _httpClient;
        private readonly GeolocationRequest _geolocationRequest;
        public MainPage()
        {
            InitializeComponent();
            _httpClient = new();
            _geolocationRequest = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(1));
        }

        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();
            PermissionStatus statusInUse = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted 
                && statusInUse != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.LocationAlways>();
                await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }
        }

        public async Task GetCurrentLocation()
        {
            var dispatcher = Application.Current.Dispatcher;
            try
            {
                _token = new CancellationTokenSource();
                Location location = await Geolocation.Default.GetLocationAsync(_geolocationRequest, _token.Token);
                if (location != null)
                {
                    dispatcher.Dispatch(() =>
                    {
                        LatitudeText.Text = $"Latitude: {location.Latitude}";
                        LongitudeText.Text = $"Longitude {location.Longitude}";
                    });
                    await UpdateToAPI(location.Latitude, location.Longitude);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            while (true)
            {
                await GetCurrentLocation();
                await Task.Delay(100);
            }
        }

        private async Task UpdateToAPI(double latitude, double longitude)
        {
            var request = await _httpClient.PostAsync(
                $"http://172.22.81.182:8080/rfid/updategeo?lat={latitude}&lon={longitude}", null
            );
            Debug.WriteLine(request.StatusCode);
        }
    }
}
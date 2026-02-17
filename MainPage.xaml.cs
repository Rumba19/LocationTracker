using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using LocationTracker.Models;
using LocationTracker.Services;

namespace LocationTracker;

public partial class MainPage : ContentPage
{
    private DatabaseService _database;
    private IDispatcherTimer _timer;
    private bool _isTracking = false;
    
    // Test mode variables
    private double _testStartLat;
    private double _testStartLon;
    private int _testPointIndex = 0;
    private const double _testDistanceStep = 0.002; 

    public MainPage()
    {
        InitializeComponent();
        
        // Initialize database
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
        _database = new DatabaseService(dbPath);
        
        // Setup timer (but don't start it yet)
        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(5); // Track every 5 seconds
        _timer.Tick += async (s, e) => await TrackLocation();
    }

    private async void OnStartTracking(object sender, EventArgs e)
    {
        // Get initial location for test data
        var currentLocation = await Geolocation.GetLocationAsync(new GeolocationRequest
        {
            DesiredAccuracy = GeolocationAccuracy.Best,
            Timeout = TimeSpan.FromSeconds(10)
        });

        if (currentLocation == null)
        {
            await DisplayAlert("Error", "Could not get your current location", "OK");
            return;
        }

        // Store starting position for test data
        _testStartLat = currentLocation.Latitude;
        _testStartLon = currentLocation.Longitude;
        _testPointIndex = 0;

        // Start tracking
        _isTracking = true;
        StartButton.IsEnabled = false;  // Disable Start button
        StopButton.IsEnabled = true;    // Enable Stop button
        _timer.Start();                 // Start the timer
        
        await DisplayAlert("Tracking Started", "Generating test locations every 5 seconds along a path", "OK");
    }

    private void OnStopTracking(object sender, EventArgs e)
    {
        // Stop tracking
        _isTracking = false;
        _timer.Stop();                  // Stop the timer
        StartButton.IsEnabled = true;   // Enable Start button
        StopButton.IsEnabled = false;   // Disable Stop button
        
        DisplayAlert("Tracking Stopped", "Location tracking has been stopped", "OK");
    }

    private async Task TrackLocation()
    {
        try
        {
            // Generate test location along a straight path (North)
            double lat = _testStartLat + (_testPointIndex * _testDistanceStep);
            double lon = _testStartLon;

            // Save to database
            var locationPoint = new LocationPoint
            {
                Latitude = lat,
                Longitude = lon,
                Timestamp = DateTime.Now
            };
            await _database.SaveLocationAsync(locationPoint);
            
            // Add circle to map
            var circle = new Circle
            {
                Center = new Location(lat, lon),
                Radius = new Distance(40),
                StrokeColor = Color.FromRgba(59, 130, 246, 220),
                FillColor = Color.FromRgba(59, 130, 246, 120),
                StrokeWidth = 2
            };
            map.MapElements.Add(circle);
            
            // Center map on current location
            map.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(lat, lon),
                Distance.FromKilometers(2)));
            
            // Increment for next point
            _testPointIndex++;
            
            // Log to console
            System.Diagnostics.Debug.WriteLine($"Test location {_testPointIndex} saved: {lat}, {lon}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error tracking location: {ex.Message}");
        }
    }

    private async void OnClearData(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Clear Data", "Remove all tracked locations from map and database?", "Yes", "No");
        if (confirm)
        {
            // Clear database
            await _database.ClearLocationsAsync();
            
            // Clear all circles from map
            map.MapElements.Clear();
            
            // Reset test counter
            _testPointIndex = 0;
            
            await DisplayAlert("Success", "All location data cleared!", "OK");
        }
    }
 
private void OnZoomIn(object sender, EventArgs e)
{
    var currentRegion = map.VisibleRegion;
    if (currentRegion != null)
    {
        var center = currentRegion.Center;
        var currentRadius = currentRegion.Radius.Meters;
        
        // Zoom in by reducing radius by half
        map.MoveToRegion(MapSpan.FromCenterAndRadius(
            center,
            Distance.FromMeters(currentRadius / 2)));
    }
}


}
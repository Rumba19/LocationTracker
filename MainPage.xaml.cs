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
        // Request location permission
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Permission Denied", "Location permission is required to track your location", "OK");
            return;
        }

        // Start tracking
        _isTracking = true;
        StartButton.IsEnabled = false;  // Disable Start button
        StopButton.IsEnabled = true;    // Enable Stop button
        _timer.Start();                 // Start the timer
        
        await DisplayAlert("Tracking Started", "Location is being tracked every 5 seconds", "OK");
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
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Best,
                Timeout = TimeSpan.FromSeconds(10)
            });

            if (location != null)
            {
                // Save to database
                var locationPoint = new LocationPoint
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Timestamp = DateTime.Now
                };

                await _database.SaveLocationAsync(locationPoint);
                
                // Add a blue circle to the map (heat map point)
                var circle = new Circle
                {
                    Center = new Location(location.Latitude, location.Longitude),
                    Radius = new Distance(50), // 50 meters radius
                    StrokeColor = Color.FromRgba(59, 130, 246, 200), // Blue with transparency
                    FillColor = Color.FromRgba(59, 130, 246, 100),
                    StrokeWidth = 2
                };
                map.MapElements.Add(circle);
                
                // Center map on current location
                map.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(location.Latitude, location.Longitude),
                    Distance.FromKilometers(1)));
                
                // Log to console (for debugging)
                System.Diagnostics.Debug.WriteLine($"Location saved: {location.Latitude}, {location.Longitude}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error tracking location: {ex.Message}");
        }
    }

    // Placeholder for Clear Data (we'll implement this next)
    private void OnClearData(object sender, EventArgs e)
    {
        DisplayAlert("Info", "Clear Data - Not implemented yet", "OK");
    }

	private async void OnTestHeatMap(object sender, EventArgs e)
	{
		try
		{
			// Get current location first
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

			await DisplayAlert("Test Mode", "Generating locations along a straight path...", "OK");

			double startLat = currentLocation.Latitude;
			double startLon = currentLocation.Longitude;

			// Create a straight line going North (you can change direction)
			int pointCount = 50;
			double distanceStep = 0.0002; // About 20-25 meters per step

			for (int i = 0; i < pointCount; i++)
			{
				// Option 1: Go North (increase latitude)
				double lat = startLat + (i * distanceStep);
				double lon = startLon;

				// Option 2: Go East (uncomment to use instead)
				// double lat = startLat;
				// double lon = startLon + (i * distanceStep);

				// Option 3: Diagonal North-East (uncomment to use instead)
				// double lat = startLat + (i * distanceStep);
				// double lon = startLon + (i * distanceStep * 0.5);

				// Save to database
				var locationPoint = new LocationPoint
				{
					Latitude = lat,
					Longitude = lon,
					Timestamp = DateTime.Now.AddSeconds(i * -10) // 10 seconds apart
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

				// Small delay for visual effect
				await Task.Delay(50);
			}

			// Center map to show the entire path
			map.MoveToRegion(MapSpan.FromCenterAndRadius(
				new Location(startLat + (pointCount * distanceStep / 2), startLon),
				Distance.FromKilometers(1.5)));

			await DisplayAlert("Test Complete", $"{pointCount} locations generated along a straight path!", "OK");
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Test failed: {ex.Message}", "OK");
		}
	}
}
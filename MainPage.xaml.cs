using LocationTracker.Services;

namespace LocationTracker;

public partial class MainPage : ContentPage
{
 private DatabaseService _database;

	public MainPage()
	{
		InitializeComponent();
		 // Initialize database
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "locations.db3");
        _database = new DatabaseService(dbPath);
	}

	  // Empty handlers for other buttons (we'll implement these later)
	private void OnStartTracking(object sender, EventArgs e)
    {
        DisplayAlert("Info", "Start Tracking - Not implemented yet", "OK");
    }

    private void OnStopTracking(object sender, EventArgs e)
    {
        DisplayAlert("Info", "Stop Tracking - Not implemented yet", "OK");
    }

    private void OnClearData(object sender, EventArgs e)
    {
        DisplayAlert("Info", "Clear Data - Not implemented yet", "OK");
    }
}

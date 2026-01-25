using SQLite;
using LocationTracker.Models;

namespace LocationTracker.Services;

public class DatabaseService
{
    private readonly SQLiteAsyncConnection _database;

    public DatabaseService(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);
        _database.CreateTableAsync<LocationPoint>().Wait();
    }

    public Task<List<LocationPoint>> GetLocationsAsync()
    {
        return _database.Table<LocationPoint>().ToListAsync();
    }

    public Task<int> SaveLocationAsync(LocationPoint location)
    {
        return _database.InsertAsync(location);
    }

    public Task<int> ClearLocationsAsync()
    {
        return _database.DeleteAllAsync<LocationPoint>();
    }
}
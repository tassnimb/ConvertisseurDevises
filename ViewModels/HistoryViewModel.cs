
using ConvertisseurDevises.Models;
using ConvertisseurDevises.Services;
using System.Collections.ObjectModel;

namespace ConvertisseurDevises.ViewModels;

public class HistoryViewModel
{
    private readonly DatabaseService _database;
    private readonly int _userId;

    public ObservableCollection<ConversionHistory> Items { get; } = new();

    public HistoryViewModel(DatabaseService database, int userId)
    {
        _database = database;
        _userId = userId;
    }

    public async Task LoadAsync()
    {
        Items.Clear();

        var history = await _database.GetHistoryAsync(_userId);

        foreach (var conversion in history)
        {
            Items.Add(conversion);
        }
    }

    public async Task DeleteAsync(ConversionHistory conversion)
    {
        await _database.DeleteConversionAsync(
            conversion.Id,
            _userId);

        Items.Remove(conversion);
    }

    public async Task ClearAsync()
    {
        await _database.ClearHistoryAsync(_userId);

        Items.Clear();
    }
}

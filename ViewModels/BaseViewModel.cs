using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConvertisseurDevises.ViewModels;

/// <summary>
/// Classe de base MVVM : implémente INotifyPropertyChanged.
/// Quand une propriété change, l'interface liée (Binding) se met à jour automatiquement.
/// </summary>
public abstract class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>Affecte la valeur et notifie l'UI uniquement si elle a réellement changé.</summary>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

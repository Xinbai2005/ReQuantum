using CommunityToolkit.Mvvm.ComponentModel;
using ReQuantum.Infrastructure.Services;
using System.ComponentModel;
using System.Globalization;

namespace ReQuantum.Infrastructure.Abstractions;

// Only used to mark ViewModel types
public interface IViewModel;

public abstract class ViewModelBase : ObservableObject, IViewModel
{
    public ILocalizer Localizer { get; }
    public IWindowService WindowService { get; }
    public INavigator Navigator { get; }
    public INotificationDispatcher Dispatcher;

    protected ViewModelBase()
    {
        Localizer = SingletonManager.Instance.GetInstance<ILocalizer>();
        WindowService = SingletonManager.Instance.GetInstance<IWindowService>();
        Navigator = SingletonManager.Instance.GetInstance<INavigator>();
        Dispatcher = SingletonManager.Instance.GetInstance<INotificationDispatcher>();

        Localizer.CultureChanged += OnCultureChanged;
        WindowService.PlatformModeChanged += OnPlatformModeChanged;
    }

    protected virtual void OnCultureChanged(CultureInfo cultureInfo)
    {
        Refresh();
    }

    protected virtual void OnPlatformModeChanged(bool isDesktop)
    {
        Refresh();
    }

    protected void Refresh()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(null));
    }
}

public abstract class ViewModelBase<TView> : ViewModelBase;
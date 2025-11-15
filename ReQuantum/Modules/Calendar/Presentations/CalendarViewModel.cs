using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.Material;
using ReQuantum.Attributes;
using ReQuantum.Infrastructure.Abstractions;
using ReQuantum.Infrastructure.Entities;
using ReQuantum.Infrastructure.Services;
using ReQuantum.Modules.Menu.Abstractions;
using ReQuantum.Resources.I18n;
using ReQuantum.Services;
using ReQuantum.Views;
using System;

namespace ReQuantum.ViewModels;

public record CalendarSelectedDateChanged(DateOnly Date) : INotification;

[AutoInject(Lifetime.Singleton, RegisterTypes = [typeof(CalendarViewModel), typeof(IMenuItemProvider)])]
public partial class CalendarViewModel : ViewModelBase<CalendarView>, IMenuItemProvider
{
    #region MenuItemProvider APIs
    public MenuItem MenuItem { get; }
    public uint Order => 2;
    #endregion

    [ObservableProperty]
    private Modules.Calendar.Presentations.CalendarPartViewModel _calendarPartViewModel;

    [ObservableProperty]
    private TodoListViewModel _todoListViewModel;

    [ObservableProperty]
    private Modules.Calendar.Presentations.EventListViewModel _eventListViewModel;

    [ObservableProperty]
    private NoteListViewModel _noteListViewModel;

    public CalendarViewModel(
        Modules.Calendar.Presentations.CalendarPartViewModel calendarPartViewModel,
        TodoListViewModel todoListViewModel,
        Modules.Calendar.Presentations.EventListViewModel eventListViewModel,
        NoteListViewModel noteListViewModel)
    {
        MenuItem = new MenuItem
        {
            Title = new LocalizedText { Key = nameof(UIText.Calendar) },
            IconKind = PackIconMaterialKind.Calendar,
            OnSelected = () => Navigator.NavigateTo<CalendarViewModel>()
        };
        CalendarPartViewModel = calendarPartViewModel;
        TodoListViewModel = todoListViewModel;
        EventListViewModel = eventListViewModel;
        NoteListViewModel = noteListViewModel;
    }
}

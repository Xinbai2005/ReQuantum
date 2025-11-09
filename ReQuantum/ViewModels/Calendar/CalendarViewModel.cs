using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.Material;
using ReQuantum.Abstractions;
using ReQuantum.Attributes;
using ReQuantum.Models;
using ReQuantum.Resources.I18n;
using ReQuantum.Views;
using System;

namespace ReQuantum.ViewModels;

[AutoInject(Lifetime.Transient, RegisterTypes = [typeof(CalendarViewModel), typeof(IMenuItemProvider)])]
public partial class CalendarViewModel : ViewModelBase<CalendarView>, IMenuItemProvider
{
    #region MenuItemProvider APIs
    public string Title => UIText.Calendar;
    public PackIconMaterialKind IconKind => PackIconMaterialKind.Calendar;
    public uint Order => 2;
    public Type ViewModelType => typeof(CalendarViewModel);
    Action<MenuItem> IMenuItemProvider.OnCultureChanged => item => item.Title = UIText.Calendar;
    #endregion

    [ObservableProperty]
    private CalendarPartViewModel _calendarPartViewModel;

    [ObservableProperty]
    private TodoListViewModel _todoListViewModel;

    [ObservableProperty]
    private EventListViewModel _eventListViewModel;

    [ObservableProperty]
    private NoteListViewModel _noteListViewModel;

    public CalendarViewModel(
        CalendarPartViewModel calendarPartViewModel,
        TodoListViewModel todoListViewModel,
        EventListViewModel eventListViewModel,
        NoteListViewModel noteListViewModel)
    {
        CalendarPartViewModel = calendarPartViewModel;
        TodoListViewModel = todoListViewModel;
        EventListViewModel = eventListViewModel;
        NoteListViewModel = noteListViewModel;

        // 订阅日历部分的日期变化，同步到其他组件
        CalendarPartViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CalendarPartViewModel.SelectedDate))
            {
                var selectedDate = CalendarPartViewModel.SelectedDate;
                TodoListViewModel.SelectedDate = selectedDate;
                EventListViewModel.SelectedDate = selectedDate;
            }
        };
        
        // 订阅待办列表的对话框关闭事件，刷新日历
        TodoListViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TodoListViewModel.IsAddDialogOpen) && !TodoListViewModel.IsAddDialogOpen)
            {
                CalendarPartViewModel.UpdateCalendarDays();
            }
        };
        
        // 订阅日程列表的对话框关闭事件，刷新日历
        EventListViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(EventListViewModel.IsAddDialogOpen) && !EventListViewModel.IsAddDialogOpen)
            {
                CalendarPartViewModel.UpdateCalendarDays();
            }
        };
        
        // 订阅便签列表的对话框关闭事件，刷新日历
        NoteListViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(NoteListViewModel.IsAddDialogOpen) && !NoteListViewModel.IsAddDialogOpen)
            {
                CalendarPartViewModel.UpdateCalendarDays();
            }
        };
    }
}

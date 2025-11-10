using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReQuantum.Controls;
using ReQuantum.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ReQuantum.ViewModels;

/// <summary>
/// 周视图日历ViewModel
/// </summary>
public partial class WeekCalendarViewModel : ViewModelBase<WeekCalendarView>
{
    [ObservableProperty]
    private DateOnly _weekStartDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private ObservableCollection<WeekDay> _weekDays = [];

    public event EventHandler<DateOnly>? DateSelected;

    private WeekDay? _previousSelectedDay;

    public WeekCalendarViewModel()
    {
        UpdateWeek();
    }

    partial void OnWeekStartDateChanged(DateOnly value)
    {
        UpdateWeek();
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        UpdateSelectionState(value);
    }

    private void UpdateWeek()
    {
        var days = GenerateWeekDays(WeekStartDate);
        WeekDays = new ObservableCollection<WeekDay>(days);
        _previousSelectedDay = WeekDays.FirstOrDefault(d => d.IsSelected);
    }

    public void SelectDate(DateOnly date)
    {
        SelectedDate = date;
        DateSelected?.Invoke(this, date);
    }

    /// <summary>
    /// 更新指定日期的时间线事项
    /// </summary>
    public void UpdateDayItems(DateOnly date, List<WeekTimelineItem> items)
    {
        var day = WeekDays.FirstOrDefault(d => d.Date == date);
        if (day != null)
        {
            day.TimelineItems = items;
        }
    }

    /// <summary>
    /// 批量更新所有日期的时间线事项
    /// </summary>
    public void UpdateAllDayItems(Dictionary<DateOnly, List<WeekTimelineItem>> itemsDict)
    {
        foreach (var day in WeekDays)
        {
            if (itemsDict.TryGetValue(day.Date, out var items))
            {
                day.TimelineItems = items;
            }
            else
            {
                day.TimelineItems = [];
            }
        }
    }

    private void UpdateSelectionState(DateOnly newSelectedDate)
    {
        // 取消上一个选中的日期
        if (_previousSelectedDay != null)
        {
            _previousSelectedDay.IsSelected = false;
        }

        // 选中新日期
        var newSelectedDay = WeekDays.FirstOrDefault(d => d.Date == newSelectedDate);
        if (newSelectedDay != null)
        {
            newSelectedDay.IsSelected = true;
            _previousSelectedDay = newSelectedDay;
        }
    }

    private List<WeekDay> GenerateWeekDays(DateOnly weekStartDate)
    {
        var days = new List<WeekDay>();
        var today = DateOnly.FromDateTime(DateTime.Now);
        var selectedDate = SelectedDate;

        for (var i = 0; i < 7; i++)
        {
            var date = weekStartDate.AddDays(i);
            days.Add(new WeekDay
            {
                Date = date,
                Day = date.Day,
                DayOfWeek = date.DayOfWeek.ToString()[..3],
                IsToday = date == today,
                IsSelected = date == selectedDate,
                ViewModel = this
            });
        }

        return days;
    }
}

public partial class WeekDay : ObservableObject
{
    public DateOnly Date { get; set; }
    public int Day { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public bool IsToday { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private List<WeekTimelineItem> _timelineItems = [];

    public WeekCalendarViewModel? ViewModel { get; set; }

    private RelayCommand? _selectCommand;
    public RelayCommand Select => _selectCommand ??= new RelayCommand(() => ViewModel?.SelectDate(Date));
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ReQuantum.Controls;
using ReQuantum.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace ReQuantum.ViewModels;

public partial class MonthCalendarViewModel : ViewModelBase<MonthCalendarView>
{
    [ObservableProperty]
    private int _year = DateTime.Now.Year;

    [ObservableProperty]
    private int _month = DateTime.Now.Month;

    [ObservableProperty]
    private DateOnly _selectedDate = DateOnly.FromDateTime(DateTime.Now);

    [ObservableProperty]
    private ObservableCollection<CalendarDay> _calendarDays = [];

    public event EventHandler<DateOnly>? DateSelected;

    private CalendarDay? _previousSelectedDay;

    public MonthCalendarViewModel()
    {
        UpdateCalendar();
    }

    partial void OnYearChanged(int value)
    {
        UpdateCalendar();
    }

    partial void OnMonthChanged(int value)
    {
        UpdateCalendar();
    }

    partial void OnSelectedDateChanged(DateOnly value)
    {
        UpdateSelectionState(value);
    }

    private void UpdateCalendar()
    {
        var days = GenerateCalendarDays(Year, Month);
        CalendarDays = new ObservableCollection<CalendarDay>(days);
        _previousSelectedDay = CalendarDays.FirstOrDefault(d => d.IsSelected);
    }

    public void SelectDate(DateOnly date)
    {
        SelectedDate = date;
        DateSelected?.Invoke(this, date);
    }

    /// <summary>
    /// 更新指定日期的事项列表
    /// </summary>
    public void UpdateDayItems(DateOnly date, List<CalendarDayItem> items)
    {
        var day = CalendarDays.FirstOrDefault(d => d.Date == date);
        if (day != null)
        {
            day.Items = items;
        }
    }

    /// <summary>
    /// 批量更新所有日期的事项
    /// </summary>
    public void UpdateAllDayItems(Dictionary<DateOnly, List<CalendarDayItem>> itemsDict)
    {
        foreach (var day in CalendarDays)
        {
            if (itemsDict.TryGetValue(day.Date, out var items))
            {
                day.Items = items;
            }
            else
            {
                day.Items = [];
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
        var newSelectedDay = CalendarDays.FirstOrDefault(d => d.Date == newSelectedDate);
        if (newSelectedDay != null)
        {
            newSelectedDay.IsSelected = true;
            _previousSelectedDay = newSelectedDay;
        }
    }

    private List<CalendarDay> GenerateCalendarDays(int year, int month)
    {
        var days = new List<CalendarDay>();
        var firstDay = new DateOnly(year, month, 1);
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var selectedDate = SelectedDate;

        // 获取本月第一天是星期几（0=Sunday, 1=Monday, ...）
        var firstDayOfWeek = (int)firstDay.DayOfWeek;

        // 添加上个月的日期（填充前面的空白）
        if (firstDayOfWeek > 0)
        {
            var prevMonth = month == 1 ? 12 : month - 1;
            var prevYear = month == 1 ? year - 1 : year;
            var daysInPrevMonth = DateTime.DaysInMonth(prevYear, prevMonth);

            for (var i = firstDayOfWeek - 1; i >= 0; i--)
            {
                var day = daysInPrevMonth - i;
                var date = new DateOnly(prevYear, prevMonth, day);
                days.Add(new CalendarDay
                {
                    Date = date,
                    Day = day,
                    IsCurrentMonth = false,
                    IsToday = false,
                    IsSelected = date == selectedDate,
                    ViewModel = this
                });
            }
        }

        // 添加本月的日期
        var today = DateOnly.FromDateTime(DateTime.Now);
        for (var day = 1; day <= daysInMonth; day++)
        {
            var date = new DateOnly(year, month, day);
            days.Add(new CalendarDay
            {
                Date = date,
                Day = day,
                IsCurrentMonth = true,
                IsToday = date == today,
                IsSelected = date == selectedDate,
                ViewModel = this
            });
        }

        // 添加下个月的日期（填充后面的空白，确保总共6周42天）
        var remainingDays = 42 - days.Count;
        var nextMonth = month == 12 ? 1 : month + 1;
        var nextYear = month == 12 ? year + 1 : year;

        for (var day = 1; day <= remainingDays; day++)
        {
            var date = new DateOnly(nextYear, nextMonth, day);
            days.Add(new CalendarDay
            {
                Date = date,
                Day = day,
                IsCurrentMonth = false,
                IsToday = false,
                IsSelected = date == selectedDate,
                ViewModel = this
            });
        }

        return days;
    }
}

public partial class CalendarDay : ObservableObject
{
    public DateOnly Date { get; set; }
    public int Day { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private List<CalendarDayItem> _items = [];

    public MonthCalendarViewModel? ViewModel { get; set; }

    private RelayCommand? _selectCommand;
    public RelayCommand Select => _selectCommand ??= new RelayCommand(() => ViewModel?.SelectDate(Date));
}

using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ReQuantum.Controls;

/// <summary>
/// 日历日期事项（用于月视图显示）
/// </summary>
public partial class CalendarDayItem : ObservableObject
{
    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private bool _isTodo;

    [ObservableProperty]
    private bool _isEvent;

    [ObservableProperty]
    private bool _isMore;

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private int _remainingCount;
}

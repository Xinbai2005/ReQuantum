using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;

namespace ReQuantum.Controls;

/// <summary>
/// 周视图时间线事项
/// </summary>
public partial class WeekTimelineItem : ObservableObject
{
    [ObservableProperty]
    private bool _isTodo;

    [ObservableProperty]
    private bool _isEvent;

    [ObservableProperty]
    private string _content = string.Empty;

    [ObservableProperty]
    private double _topPosition; // 在时间线上的位置（0-24小时）

    [ObservableProperty]
    private double _height; // 高度（小时数）

    [ObservableProperty]
    private bool _isCompleted;

    [ObservableProperty]
    private string _timeLabel = string.Empty; // 时间标注（例如："09:00 - 10:30"）
}

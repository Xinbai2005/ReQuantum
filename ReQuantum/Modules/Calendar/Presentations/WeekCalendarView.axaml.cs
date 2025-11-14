using Avalonia.Controls;

namespace ReQuantum.Views;

public partial class WeekCalendarView : UserControl
{
    public WeekCalendarView()
    {
        InitializeComponent();

        MobileTimeGrid?.AddHandler(RequestBringIntoViewEvent, Grid_RequestBringIntoView);
        DesktopTimeGrid?.AddHandler(RequestBringIntoViewEvent, Grid_RequestBringIntoView);
    }

    private void Grid_RequestBringIntoView(object? sender, RequestBringIntoViewEventArgs e)
    {
        // Cancel the bring into view request to prevent viewport jumping
        e.Handled = true;
    }
}

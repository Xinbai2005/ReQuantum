using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.Material;
using ReQuantum.Infrastructure.Abstractions;
using ReQuantum.Infrastructure.Entities;
using ReQuantum.Infrastructure.Services;
using ReQuantum.Modules.Common.Attributes;
using ReQuantum.Modules.Menu.Abstractions;
using ReQuantum.Resources.I18n;
using ReQuantum.Services;
using ReQuantum.Views;
using System;
using System.Diagnostics.CodeAnalysis;

namespace ReQuantum.ViewModels;

[AutoInject(Lifetime.Singleton, RegisterTypes = [typeof(DashboardViewModel), typeof(IMenuItemProvider)])]
public partial class DashboardViewModel : ViewModelBase<DashboardView>, IMenuItemProvider
{
    #region MenuItemProvider APIs
    public MenuItem MenuItem { get; }
    public uint Order => 0;

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    public Type ViewModelType => typeof(DashboardViewModel);
    #endregion

    private readonly ILocalizer _localizer;

    public string Welcome => _localizer[UIText.HelloWorld];

    public DashboardViewModel(ILocalizer localizer)
    {
        MenuItem = new MenuItem
        {
            Title = new LocalizedText { Key = nameof(UIText.Home) },
            IconKind = PackIconMaterialKind.HomeOutline,
            OnSelected = () => Navigator.NavigateTo<DashboardViewModel>()
        };
        _localizer = localizer;
    }

    [RelayCommand]
    private void UpdateWelcome()
    {
        _localizer.SetCulture("zh-CN");
    }
}

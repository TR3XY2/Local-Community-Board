// <copyright file="AdminPanelView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents the administrative panel view for managing users.
/// </summary>
/// <remarks>This control is a part of the application's user interface and is responsible for displaying and
/// managing user data. It initializes required services and loads user information asynchronously when the control is
/// instantiated.</remarks>
public partial class AdminPanelView : UserControl
{
    private readonly IUserService userService;

    public AdminPanelView()
    {
        this.InitializeComponent();
        this.userService = App.Services.GetRequiredService<IUserService>();
        _ = this.LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        var users = await this.userService.GetAllUsersAsync();
        this.UsersGrid.ItemsSource = users;
    }
}

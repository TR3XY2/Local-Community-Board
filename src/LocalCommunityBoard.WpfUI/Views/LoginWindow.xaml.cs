// <copyright file="LoginWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.WpfUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents the login window of the application, allowing users to authenticate with their credentials.
/// </summary>
/// <remarks>This window provides a user interface for entering an email and password using MVVM pattern
/// with data binding and commands. The authentication logic is handled by <see cref="LoginViewModel"/>.</remarks>
public partial class LoginWindow : Window
{
    private readonly LoginViewModel viewModel;

    public LoginWindow()
    {
        this.InitializeComponent();

        var userService = App.Services.GetRequiredService<IUserService>();
        var session = App.Services.GetRequiredService<UserSession>();

        this.viewModel = new LoginViewModel(userService, session, this);
        this.DataContext = this.viewModel;

        // Handle PasswordBox binding (PasswordBox doesn't support direct binding for security reasons)
        this.PasswordBox.PasswordChanged += this.PasswordBox_PasswordChanged;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            this.viewModel.Password = passwordBox.Password;
        }
    }
}

// <copyright file="LoginWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents the login window of the application, allowing users to authenticate with their credentials.
/// </summary>
/// <remarks>This window provides a user interface for entering an email and password, and attempts to
/// authenticate the user using the provided credentials. Upon successful login, the user's session is initialized, and
/// the window closes with a positive dialog result. If authentication fails, an error message is displayed.</remarks>
public partial class LoginWindow : Window
{
    private readonly IUserService userService;
    private readonly UserSession session;

    public LoginWindow()
    {
        this.InitializeComponent();
        this.userService = App.Services.GetRequiredService<IUserService>();
        this.session = App.Services.GetRequiredService<UserSession>();
    }

    private async void Login_Click(object sender, RoutedEventArgs e)
    {
        var email = this.EmailBox.Text;
        var password = this.PasswordBox.Password;

        var user = await this.userService.LoginAsync(email, password);
        if (user == null)
        {
            MessageBox.Show("Invalid email or password.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (user.Status == UserStatus.Blocked)
        {
            MessageBox.Show("Your account is blocked.", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        // Успішний вхід
        this.session.Login(user);
        MessageBox.Show($"Welcome, {user.Username}!", "Login Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        this.DialogResult = true;
        this.Close();
    }
}

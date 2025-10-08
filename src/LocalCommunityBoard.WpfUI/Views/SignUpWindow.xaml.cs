// <copyright file="SignUpWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System;
using System.Windows;
using LocalCommunityBoard.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a window that allows users to sign up by providing their username, email, and password.
/// </summary>
/// <remarks>This window provides a user interface for registering a new account. It interacts with an
/// IUserService to handle the registration process. Upon successful registration,  the window displays a success
/// message and closes. If an error occurs during registration,  an error message is displayed.</remarks>
public partial class SignUpWindow : Window
{
    private readonly IUserService userService;

    public SignUpWindow()
    {
        this.InitializeComponent();
        this.userService = App.Services.GetRequiredService<IUserService>();
    }

    private async void SignUp_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var username = this.UsernameBox.Text;
            var email = this.EmailBox.Text;
            var password = this.PasswordBox.Password;

            var user = await this.userService.RegisterAsync(username, email, password);
            MessageBox.Show($"User {user.Username} registered successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

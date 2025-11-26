// <copyright file="LoginViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.ViewModels;

using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Enums;

/// <summary>
/// View model for the login window, handling user authentication through data binding and commands.
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly IUserService userService;
    private readonly UserSession session;
    private readonly Window window;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginViewModel"/> class.
    /// </summary>
    /// <param name="userService">The user service for authentication.</param>
    /// <param name="session">The user session to store authenticated user.</param>
    /// <param name="window">The login window instance for closing after successful login.</param>
    public LoginViewModel(IUserService userService, UserSession session, Window window)
    {
        this.userService = userService;
        this.session = session;
        this.window = window;
    }

    /// <summary>
    /// Command to handle user login attempt.
    /// </summary>
    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(this.Email))
        {
            MessageBox.Show(
                "Please enter your email.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(this.Password))
        {
            MessageBox.Show(
                "Please enter your password.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        this.IsLoading = true;

        try
        {
            var user = await this.userService.LoginAsync(this.Email, this.Password);

            if (user == null)
            {
                MessageBox.Show(
                    "Invalid email or password.",
                    "Login Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (user.Status == UserStatus.Blocked)
            {
                MessageBox.Show(
                    "Your account is blocked.",
                    "Login Failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            // Successful login
            this.session.Login(user);
            MessageBox.Show(
                $"Welcome, {user.Username}!",
                "Login Successful",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            this.window.DialogResult = true;
            this.window.Close();
        }
        finally
        {
            this.IsLoading = false;
        }
    }
}

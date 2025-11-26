// <copyright file="SignUpWindowViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalCommunityBoard.Application.Interfaces;

/// <summary>
/// Represents the view model for the Sign Up window, providing properties for username, email, and password,
/// as well as a command to register a new user.
/// </summary>
public partial class SignUpViewModel : ObservableObject
{
    private readonly IUserService userService;
    private readonly Func<Task>? onSuccessCallback;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string email = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    public SignUpViewModel(IUserService userService, Func<Task>? onSuccessCallback = null)
    {
        this.userService = userService;
        this.onSuccessCallback = onSuccessCallback;
    }

    /// <summary>
    /// Command to register a new user.
    /// </summary>
    [RelayCommand]
    private async Task SignUpAsync()
    {
        try
        {
            var user = await this.userService.RegisterAsync(this.Username, this.Email, this.Password);

            MessageBox.Show(
                $"User {user.Username} registered successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (this.onSuccessCallback != null)
            {
                await this.onSuccessCallback();
            }
        }
        catch (Exception ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            MessageBox.Show(message, "Registration Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

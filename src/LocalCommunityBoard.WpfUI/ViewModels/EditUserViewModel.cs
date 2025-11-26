// <copyright file="EditUserViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.ViewModels;

using System;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;
using LocalCommunityBoard.WpfUI.Views;

/// <summary>
/// ViewModel for editing user information in the admin panel.
/// Demonstrates data binding and commands using MVVM Toolkit.
/// </summary>
public partial class EditUserViewModel : ObservableObject
{
    private const string ValidationErrorTitle = "Validation Error";

    private readonly IUserService userService;
    private readonly User user;
    private readonly Func<Task>? onSaveCallback;

    private string username = string.Empty;
    private string email = string.Empty;
    private string? newPassword;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditUserViewModel"/> class.
    /// </summary>
    /// <param name="user">The user to edit.</param>
    /// <param name="userService">User service used to update user data.</param>
    /// <param name="onSaveCallback">Callback executed after successful save.</param>
    public EditUserViewModel(User user, IUserService userService, Func<Task>? onSaveCallback = null)
    {
        this.user = user ?? throw new ArgumentNullException(nameof(user));
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.onSaveCallback = onSaveCallback;

        this.username = user.Username;
        this.email = user.Email;
    }

    /// <summary>
    /// Gets or sets the username of the user being edited.
    /// </summary>
    public string Username
    {
        get => this.username;
        set => this.SetProperty(ref this.username, value);
    }

    /// <summary>
    /// Gets or sets the email of the user being edited.
    /// </summary>
    public string Email
    {
        get => this.email;
        set => this.SetProperty(ref this.email, value);
    }

    /// <summary>
    /// Gets or sets the new password entered by the admin.
    /// </summary>
    public string? NewPassword
    {
        get => this.newPassword;
        set => this.SetProperty(ref this.newPassword, value);
    }

    /// <summary>
    /// Saves changes made to the user and updates the database
    /// using <see cref="IUserService"/>.
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            string currentUsername = this.Username?.Trim() ?? string.Empty;
            string currentEmail = this.Email?.Trim() ?? string.Empty;
            string? currentNewPassword = this.NewPassword?.Trim();

            if (string.IsNullOrWhiteSpace(currentUsername))
            {
                MessageBox.Show(
                    "Username cannot be empty.",
                    ValidationErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(currentEmail))
            {
                MessageBox.Show(
                    "Email cannot be empty.",
                    ValidationErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!currentEmail.Contains('@') || !currentEmail.Contains('.'))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    ValidationErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            string oldUsername = this.user.Username;
            string oldEmail = this.user.Email;
            bool passwordChanged = !string.IsNullOrWhiteSpace(currentNewPassword);

            bool success = await this.userService.AdminUpdateUserAsync(
                this.user.Id,
                currentUsername,
                currentEmail,
                passwordChanged ? currentNewPassword : null);

            if (!success)
            {
                MessageBox.Show(
                    "Failed to update user. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            this.user.Username = currentUsername;
            this.user.Email = currentEmail;

            string message = $"User '{currentUsername}' updated successfully.\n\n" +
                             $"Username: {oldUsername} → {currentUsername}\n" +
                             $"Email: {oldEmail} → {currentEmail}\n";

            if (passwordChanged)
            {
                message += "\nPassword: Updated ✓";
            }

            MessageBox.Show(
                message,
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            if (this.onSaveCallback != null)
            {
                await this.onSaveCallback();
            }

            this.NavigateBack();
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(
                ex.Message,
                ValidationErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(
                ex.Message,
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while updating the user:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Cancels editing and navigates back to the admin panel.
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        MessageBoxResult result = MessageBox.Show(
            "Are you sure you want to cancel? All unsaved changes will be lost.",
            "Confirm Cancel",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            this.NavigateBack();
        }
    }

    private void NavigateBack()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.MainContent.Content = new AdminPanelView();
        }
    }
}

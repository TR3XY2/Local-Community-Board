// <copyright file="EditUserView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Security;
using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Represents a view for editing user information by administrators.
/// </summary>
public partial class EditUserView : UserControl
{
    private readonly User user;
    private readonly IUserService userService;
    private readonly Func<Task>? onSaveCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditUserView"/> class.
    /// </summary>
    /// <param name="user">The user to edit.</param>
    /// <param name="userService">The user service for database operations.</param>
    /// <param name="onSaveCallback">Async callback to execute after successful save.</param>
    public EditUserView(User user, IUserService userService, Func<Task>? onSaveCallback = null)
    {
        this.InitializeComponent();
        this.user = user ?? throw new ArgumentNullException(nameof(user));
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.onSaveCallback = onSaveCallback;

        this.LoadUserData();
    }

    /// <summary>
    /// Loads the user's current data into the form fields.
    /// </summary>
    private void LoadUserData()
    {
        this.UsernameBox.Text = this.user.Username;
        this.EmailBox.Text = this.user.Email;
    }

    /// <summary>
    /// Handles the Save button click event.
    /// </summary>
    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var username = this.UsernameBox.Text?.Trim();
            var email = this.EmailBox.Text?.Trim();
            var newPassword = this.NewPasswordBox.Password?.Trim();

            // Validate username
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "Username cannot be empty.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Email cannot be empty.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains("@") || !email.Contains("."))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Validate password if provided
            if (!string.IsNullOrWhiteSpace(newPassword) && newPassword.Length < 6)
            {
                MessageBox.Show(
                    "Password must be at least 6 characters long.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Store old values for display
            var oldUsername = this.user.Username;
            var oldEmail = this.user.Email;
            var passwordChanged = false;

            // Update user
            this.user.Username = username;
            this.user.Email = email;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                this.user.Password = PasswordHasher.HashPassword(newPassword);
                passwordChanged = true;
            }

            var success = await this.userService.UpdateAsync(this.user);

            if (success)
            {
                var message = $"User '{username}' updated successfully.\n\n" +
                              $"Username: {oldUsername} → {username}\n" +
                              $"Email: {oldEmail} → {email}\n";

                if (passwordChanged)
                {
                    message += "\nPassword: Updated ✓";
                }

                MessageBox.Show(
                    message,
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Call the callback to refresh the admin panel
                if (this.onSaveCallback != null)
                {
                    await this.onSaveCallback();
                }

                // Navigate back to admin panel
                this.NavigateBack();
            }
            else
            {
                MessageBox.Show(
                    "Failed to update user. Please try again.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(
                $"Validation error: {ex.Message}",
                "Validation Error",
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
    /// Handles the Cancel button click event.
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to cancel? All unsaved changes will be lost.",
            "Confirm Cancel",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            this.NavigateBack();
        }
    }

    /// <summary>
    /// Navigates back to the admin panel.
    /// </summary>
    private void NavigateBack()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.MainContent.Content = new AdminPanelView();
        }
    }
}

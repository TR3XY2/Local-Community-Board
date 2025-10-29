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
    private const string ValidationErrorTitle = "Validation Error";
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

    private static void NavigateBack()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.MainContent.Content = new AdminPanelView();
        }
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
                    ValidationErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "Email cannot be empty.",
                    ValidationErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            if (!email.Contains('@') || !email.Contains('.'))
            {
                MessageBox.Show(
                    "Please enter a valid email address.",
                    ValidationErrorTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Store old values for display
            var oldUsername = this.user.Username;
            var oldEmail = this.user.Email;
            var passwordChanged = !string.IsNullOrWhiteSpace(newPassword);

            // Use service method for admin update
            var success = await this.userService.AdminUpdateUserAsync(
                this.user.Id,
                username,
                email,
                string.IsNullOrWhiteSpace(newPassword) ? null : newPassword);

            if (success)
            {
                // Update local user object for display
                this.user.Username = username;
                this.user.Email = email;

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
                NavigateBack();
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
            NavigateBack();
        }
    }
}

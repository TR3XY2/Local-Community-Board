// <copyright file="ProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System;
using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Profile view for the logged-in user.
/// </summary>
public partial class ProfileView : UserControl
{
    private readonly IUserService userService;
    private readonly UserSession session;

    public ProfileView()
    {
        this.InitializeComponent();
        this.userService = App.Services.GetRequiredService<IUserService>();
        this.session = App.Services.GetRequiredService<UserSession>();

        if (this.session.IsLoggedIn && this.session.CurrentUser != null)
        {
            this.UsernameBox.Text = this.session.CurrentUser.Username;
            this.EmailBox.Text = this.session.CurrentUser.Email;
        }
    }

    private async void SaveChanges_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser == null)
        {
            MessageBox.Show("You must be logged in to edit your profile.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            var userId = this.session.CurrentUser.Id;
            var newUsername = this.UsernameBox.Text.Trim();
            var newEmail = this.EmailBox.Text.Trim();

            // Always save basic info
            await this.userService.UpdatePersonalInfoAsync(userId, newUsername, newEmail);

            // Optional password change
            var oldPassword = this.CurrentPasswordBox.Password;
            var newPassword = this.NewPasswordBox.Password;
            var confirmPassword = this.ConfirmPasswordBox.Password;

            if (!string.IsNullOrWhiteSpace(oldPassword) ||
                !string.IsNullOrWhiteSpace(newPassword) ||
                !string.IsNullOrWhiteSpace(confirmPassword))
            {
                if (string.IsNullOrWhiteSpace(oldPassword) ||
                    string.IsNullOrWhiteSpace(newPassword) ||
                    string.IsNullOrWhiteSpace(confirmPassword))
                {
                    throw new InvalidOperationException("Please fill in all password fields to change your password.");
                }

                if (newPassword != confirmPassword)
                {
                    throw new InvalidOperationException("New passwords do not match.");
                }

                await this.userService.ChangePasswordAsync(userId, oldPassword, newPassword);
            }

            this.session.CurrentUser.Username = newUsername;
            this.session.CurrentUser.Email = newEmail;

            this.StatusText.Visibility = Visibility.Visible;
            this.StatusText.Foreground = System.Windows.Media.Brushes.Green;
            this.StatusText.Text = "Changes saved successfully.";
        }
        catch (Exception ex)
        {
            this.StatusText.Visibility = Visibility.Visible;
            this.StatusText.Foreground = System.Windows.Media.Brushes.Red;
            this.StatusText.Text = $"Error: {ex.Message}";
        }
    }
}

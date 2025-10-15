// <copyright file="ProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;
using LocalCommunityBoard.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Represents a user interface for viewing and editing the profile of the currently logged-in user.
/// </summary>
/// <remarks>The <see cref="ProfileView"/> control provides functionality for displaying and updating user profile
/// information,  including username, email, and password. It also allows the user to view their announcements.  This
/// control depends on the <see cref="IUserService"/> for user-related operations,  <see cref="IAnnouncementService"/>
/// for managing announcements, and <see cref="UserSession"/> for session management.</remarks>
public partial class ProfileView : UserControl
{
    private readonly IUserService userService;
    private readonly IAnnouncementService announcementService;
    private readonly UserSession session;

    public ProfileView()
    {
        this.InitializeComponent();
        this.userService = App.Services.GetRequiredService<IUserService>();
        this.announcementService = App.Services.GetRequiredService<IAnnouncementService>();
        this.session = App.Services.GetRequiredService<UserSession>();

        if (this.session.IsLoggedIn && this.session.CurrentUser != null)
        {
            this.UsernameBox.Text = this.session.CurrentUser.Username;
            this.EmailBox.Text = this.session.CurrentUser.Email;
            _ = this.LoadMyAnnouncementsAsync();
        }
    }

    private async Task LoadMyAnnouncementsAsync()
    {
        try
        {
            if (!this.session.IsLoggedIn || this.session.CurrentUser == null)
            {
                return;
            }

            var userId = this.session.CurrentUser.Id;
            var myAnnouncements = await this.announcementService.GetAnnouncementsByUserIdAsync(userId);

            this.MyAnnouncementsList.ItemsSource = myAnnouncements;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load announcements: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteMyAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser is null)
        {
            MessageBox.Show(
                "You must be logged in to delete your announcements.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (sender is not Button btn || btn.Tag is not int announcementId)
        {
            return;
        }

        var confirm = MessageBox.Show(
            "Delete this announcement?",
            "Confirm",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            var userId = this.session.CurrentUser.Id;
            var ok = await this.announcementService.DeleteAnnouncementAsync(announcementId, userId);

            if (!ok)
            {
                MessageBox.Show(
                    "You don't have permission or the announcement was not found.",
                    "Cannot delete",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Після успішного видалення — оновлюємо список
            await this.LoadMyAnnouncementsAsync();

            this.StatusText.Visibility = Visibility.Visible;
            this.StatusText.Foreground = System.Windows.Media.Brushes.Green;
            this.StatusText.Text = "Announcement deleted.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to delete: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

            await this.userService.UpdatePersonalInfoAsync(userId, newUsername, newEmail);

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

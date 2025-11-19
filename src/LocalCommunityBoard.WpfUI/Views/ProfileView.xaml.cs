// <copyright file="ProfileView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
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
    private const string ErrorTitle = "Error";
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
            MessageBox.Show($"Failed to load announcements: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void EditMyAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser is null)
        {
            MessageBox.Show(
                "You must be logged in to edit your announcements.",
                ErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (sender is not Button btn || btn.Tag is not int announcementId)
        {
            return;
        }

        var announcements = await this.announcementService.GetAnnouncementsByUserIdAsync(this.session.CurrentUser.Id);
        var announcement = announcements?.FirstOrDefault(a => a.Id == announcementId);
        if (announcement == null)
        {
            MessageBox.Show("Announcement not found.", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var newTitle = Microsoft.VisualBasic.Interaction.InputBox("New title:", "Edit Announcement", announcement.Title);
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            return;
        }

        var newBody = Microsoft.VisualBasic.Interaction.InputBox("New body:", "Edit Announcement", announcement.Body);
        if (string.IsNullOrWhiteSpace(newBody))
        {
            return;
        }

        var newPhoto = Microsoft.VisualBasic.Interaction.InputBox("New photo URL:", "Edit Announcement", announcement.ImageUrl ?? string.Empty);
        if (string.IsNullOrWhiteSpace(newPhoto))
        {
            newPhoto = announcement.Title;
        }

        try
        {
            var ok = await this.announcementService.UpdateAnnouncementAsync(
                announcement.Id,
                this.session.CurrentUser.Id,
                newTitle,
                newBody,
                announcement.CategoryId,
                newPhoto);

            if (ok)
            {
                await this.LoadMyAnnouncementsAsync();
                this.StatusText.Visibility = Visibility.Visible;
                this.StatusText.Foreground = System.Windows.Media.Brushes.Green;
                this.StatusText.Text = "Announcement updated.";
            }
            else
            {
                MessageBox.Show("You don't have permission or the announcement was not found.", "Cannot edit", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DeleteMyAnnouncement_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser is null)
        {
            MessageBox.Show(
                "You must be logged in to delete your announcements.",
                ErrorTitle,
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

            await this.LoadMyAnnouncementsAsync();

            this.StatusText.Visibility = Visibility.Visible;
            this.StatusText.Foreground = System.Windows.Media.Brushes.Green;
            this.StatusText.Text = "Announcement deleted.";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Failed to delete: {ex.Message}",
                ErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async void SaveChanges_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser == null)
        {
            MessageBox.Show("You must be logged in to edit your profile.", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
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

    private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
    {
        if (!this.session.IsLoggedIn || this.session.CurrentUser == null)
        {
            MessageBox.Show(
                "You must be logged in to delete your account.",
                ErrorTitle,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var confirm = MessageBox.Show(
            "Are you sure you want to permanently delete your account?\nThis action cannot be undone.",
            "Confirm Deletion",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes)
        {
            return;
        }

        string password = Microsoft.VisualBasic.Interaction.InputBox(
            "Please enter your password to confirm:",
            "Password Confirmation",
            string.Empty);

        if (string.IsNullOrWhiteSpace(password))
        {
            MessageBox.Show("Password is required.", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            var userId = this.session.CurrentUser.Id;

            bool ok = await this.userService.DeleteOwnAccountAsync(userId, password);

            if (!ok)
            {
                MessageBox.Show("Failed to delete your account.", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            this.session.Logout();

            if (Application.Current.MainWindow is MainWindow mw)
            {
                mw.TopBar.UpdateUIFromOutside();
                mw.NavigateHome();
            }

            MessageBox.Show("Your account has been deleted.", "Account Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", ErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

// <copyright file="CreatePostView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Application.Services;

/// <summary>
/// Represents a user interface for creating a new post or announcement.
/// </summary>
/// <remarks>This control provides functionality for users to input details such as title, body, category,  and
/// location information for a new post. It validates user input and interacts with the  <see
/// cref="IAnnouncementService"/> to publish the post. The control also ensures that the user  is logged in before
/// allowing the post to be created.</remarks>
public partial class CreatePostView : UserControl
{
    private readonly IAnnouncementService announcementService;
    private readonly UserSession userSession;

    public CreatePostView(IAnnouncementService announcementService, UserSession userSession)
    {
        this.InitializeComponent();
        this.announcementService = announcementService;
        this.userSession = userSession;
    }

    private static void TryNavigateHome()
    {
        try
        {
            if (Application.Current?.MainWindow is { } mw)
            {
                var navigateMethod = mw.GetType().GetMethod("NavigateHome");
                navigateMethod?.Invoke(mw, null);
            }
        }
        catch
        {
            // ignore
        }
    }

    private async void Publish_Click(object sender, RoutedEventArgs e)
    {
        var title = this.TitleTextBox.Text?.Trim() ?? string.Empty;
        var body = this.BodyTextBox.Text?.Trim() ?? string.Empty;
        var category = (this.CategoryComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        var city = this.CityTextBox.Text?.Trim() ?? string.Empty;
        var district = this.DistrictTextBox.Text?.Trim();
        var street = this.StreetTextBox.Text?.Trim();
        var imageUrl = this.ImagesTextBox.Text?.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()?.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            MessageBox.Show("Title cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            MessageBox.Show("Body cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(category))
        {
            MessageBox.Show("Select a category.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(city))
        {
            MessageBox.Show("City is required.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (this.userSession?.IsLoggedIn != true || this.userSession.CurrentUser == null)
        {
            MessageBox.Show("You must be logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        int userId = this.userSession.CurrentUser.Id;

        try
        {
            await this.announcementService.CreateAnnouncementAsync(
                userId,
                title,
                body,
                category,
                city,
                district,
                street,
                imageUrl);

            MessageBox.Show("Announcement created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            TryNavigateHome();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        TryNavigateHome();
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
    }
}

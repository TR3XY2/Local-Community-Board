// <copyright file="MainView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Represents the main view for displaying announcements in the application.
/// </summary>
/// <remarks>This view is a user control that interacts with an <see cref="IAnnouncementService"/> to load and
/// display a collection of announcements. The announcements are displayed in a bound list, and the data is loaded
/// asynchronously when the view is initialized.</remarks>
public partial class MainView : UserControl
{
    private readonly IAnnouncementService announcementService;

    public MainView(IAnnouncementService announcementService)
    {
        this.InitializeComponent();
        this.announcementService = announcementService;

        this.Loaded += this.MainView_Loaded;
    }

    public ObservableCollection<Announcement> Announcements { get; set; } = new();

    private async void MainView_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        await this.LoadAnnouncementsAsync();
    }

    private void CleaFilters_Click(object sender, RoutedEventArgs e)
    {
        this.CityInput.Text = string.Empty;
        this.DistrictInput.Text = string.Empty;
        this.StreetInput.Text = string.Empty;

        this.NewCheck.IsChecked = false;
        this.PostsCheck.IsChecked = false;
        this.EventsCheck.IsChecked = false;
    }

    private async Task LoadAnnouncementsAsync()
    {
        string? city = string.IsNullOrWhiteSpace(this.CityInput.Text) ? null : this.CityInput.Text.Trim();
        string? district = string.IsNullOrWhiteSpace(this.DistrictInput.Text) ? null : this.DistrictInput.Text.Trim();
        string? street = string.IsNullOrWhiteSpace(this.StreetInput.Text) ? null : this.StreetInput.Text.Trim();

        var selectedCategories = new List<int>();
        if (this.NewCheck.IsChecked == true)
        {
            selectedCategories.Add(1);
        }

        if (this.EventsCheck.IsChecked == true)
        {
            selectedCategories.Add(2);
        }

        if (this.PostsCheck.IsChecked == true)
        {
            selectedCategories.Add(3);
        }

        var data = await this.announcementService.GetAnnouncementsAsync(
            city: city,
            district: district,
            street: street,
            categoryIds: selectedCategories.Any() ? selectedCategories : null);

        this.Announcements.Clear();
        foreach (var item in data)
        {
            this.Announcements.Add(item);
        }

        this.AnnouncementsList.ItemsSource = this.Announcements;
    }

    private async void ApplyFilters_Click(object sender, RoutedEventArgs e)
    {
        await this.LoadAnnouncementsAsync();
    }

    private void Announcement_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is Announcement selected)
        {
            var window = Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.MainContent.Content = new AnnouncementDetailsView(selected);
            }
        }
    }
}

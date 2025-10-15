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
using LocalCommunityBoard.WpfUI.ViewModels;

/// <summary>
/// Represents the main view for displaying announcements in the application.
/// </summary>
/// <remarks>This view is a user control that interacts with an <see cref="IAnnouncementService"/> to load and
/// display a collection of announcements. The announcements are displayed in a bound list, and the data is loaded
/// asynchronously when the view is initialized.</remarks>
public partial class MainView : UserControl
{
    private const int PageSize = 9;
    private readonly IAnnouncementService announcementService;
    private int currentPage = 1;
    private int totalPages = 1;

    public MainView(IAnnouncementService announcementService)
    {
        this.InitializeComponent();
        this.announcementService = announcementService;
        this.DataContext = this;
        this.Loaded += this.MainView_Loaded;
    }

    public string CurrentPageText => $"Page {this.currentPage} of {this.totalPages}";

    public ObservableCollection<AnnouncementViewModel> Announcements { get; } = new();

    private async void PrevPage_Click(object sender, RoutedEventArgs e)
    {
        if (this.currentPage > 1)
        {
            this.currentPage--;
            await this.LoadAnnouncementsAsync();
        }
    }

    private async void NextPage_Click(object sender, RoutedEventArgs e)
    {
        if (this.currentPage < this.totalPages)
        {
            this.currentPage++;
            await this.LoadAnnouncementsAsync();
        }
    }

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
        var (items, totalCount) = await this.announcementService.GetAnnouncementsPagedAsync(
            city: this.CityInput.Text,
            district: this.DistrictInput.Text,
            street: this.StreetInput.Text,
            categoryIds: this.GetSelectedCategoryIds(),
            date: null,
            pageNumber: this.currentPage,
            pageSize: PageSize);

        this.totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
        this.PageInfoText.Text = $"Page {this.currentPage} of {this.totalPages}";

        this.Announcements.Clear();
        foreach (var item in items)
        {
            var vm = new AnnouncementViewModel
            {
                Id = item.Id,
                Title = item.Title,
                Body = item.Body,
                CreatedAt = item.CreatedAt,
                ImageUrl = item.ImageUrl,
                Category = item.Category,
                CategoryId = item.CategoryId,
                Location = item.Location,
                User = item.User,
            };

            this.Announcements.Add(vm);
            _ = vm.LoadImageAsync();
        }
    }

    private async void ApplyFilters_Click(object sender, RoutedEventArgs e)
    {
        await this.LoadAnnouncementsAsync();
    }

    private void Announcement_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is AnnouncementViewModel selected)
        {
            var window = Application.Current.MainWindow as MainWindow;
            if (window != null)
            {
                window.MainContent.Content = new AnnouncementDetailsView(selected);
            }
        }
    }

    private IEnumerable<int> GetSelectedCategoryIds()
    {
        var ids = new List<int>();
        if (this.NewCheck.IsChecked == true)
        {
            ids.Add(1);
        }

        if (this.EventsCheck.IsChecked == true)
        {
            ids.Add(2);
        }

        if (this.PostsCheck.IsChecked == true)
        {
            ids.Add(3);
        }

        return ids;
    }
}

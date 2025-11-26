// <copyright file="EditAnnouncementViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Represents the view model for editing an announcement, providing properties for the announcement's title, body, and
/// image URL, as well as a command to save changes.
/// </summary>
/// <remarks>This view model is designed to be used in scenarios where an announcement can be edited and saved. It
/// interacts with an <see cref="IAnnouncementService"/> to persist changes and optionally invokes a callback upon
/// successful save.</remarks>
public partial class EditAnnouncementViewModel : ObservableObject
{
    private readonly IAnnouncementService announcementService;
    private readonly Announcement announcement;
    private readonly Func<Task>? onSaveCallback;

    [ObservableProperty]
    private string title;

    [ObservableProperty]
    private string body;

    [ObservableProperty]
    private string imageUrl;

    public EditAnnouncementViewModel(
        Announcement announcement,
        IAnnouncementService announcementService,
        Func<Task>? onSaveCallback)
    {
        this.announcement = announcement;
        this.announcementService = announcementService;
        this.onSaveCallback = onSaveCallback;

        this.Title = announcement.Title;
        this.Body = announcement.Body;
        this.ImageUrl = announcement.ImageUrl ?? string.Empty;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        bool updated = await this.announcementService.UpdateAnnouncementAsync(
            this.announcement.Id,
            this.announcement.UserId,
            this.Title.Trim(),
            this.Body.Trim(),
            this.announcement.CategoryId,
            this.ImageUrl.Trim());

        if (updated)
        {
            MessageBox.Show(
                "Оголошення успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

            if (this.onSaveCallback != null)
            {
                await this.onSaveCallback();
            }
        }
        else
        {
            MessageBox.Show("Не вдалося оновити оголошення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

// <copyright file="EditAnnouncementView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using LocalCommunityBoard.Application.Interfaces;
    using LocalCommunityBoard.Domain.Entities;

#pragma warning disable SA1601 // Partial elements should be documented
    public partial class EditAnnouncementView : UserControl
#pragma warning restore SA1601 // Partial elements should be documented
    {
        private readonly IAnnouncementService announcementService;
        private readonly Announcement announcement;
        private readonly Func<Task> onSaveCallback;

        public EditAnnouncementView(
            Announcement announcement,
            IAnnouncementService announcementService,
            Func<Task> onSaveCallback)
        {
            this.InitializeComponent();
            this.announcement = announcement;
            this.announcementService = announcementService;
            this.onSaveCallback = onSaveCallback;

            this.TitleTextBox.Text = announcement.Title;
            this.BodyTextBox.Text = announcement.Body;
            this.ImageUrlTextBox.Text = announcement.ImageUrl ?? string.Empty;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool updated = await this.announcementService.UpdateAnnouncementAsync(
                    this.announcement.Id,
                    this.announcement.UserId, // власник оголошення, бо Admin діє від імені системи
                    this.TitleTextBox.Text.Trim(),
                    this.BodyTextBox.Text.Trim(),
                    this.announcement.CategoryId,
                    this.ImageUrlTextBox.Text.Trim());

                if (updated)
                {
                    MessageBox.Show("Оголошення успішно оновлено.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                    if (this.onSaveCallback is not null)
                    {
                        await this.onSaveCallback();
                    }
                }
                else
                {
                    MessageBox.Show("Не вдалося оновити оголошення.", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка при оновленні оголошення: {ex.Message}", "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

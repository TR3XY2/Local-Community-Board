// <copyright file="CreatePostView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views
{
    using System.Windows;
    using System.Windows.Controls;
    using LocalCommunityBoard.Application.Interfaces;
    using LocalCommunityBoard.Application.Services;

    /// <summary>
    /// Interaction logic for CreatePostView.xaml.
    /// </summary>
    public partial class CreatePostView : UserControl
    {
        private const string ValidationTitle = "Validation";

        private static readonly char[] commaSeparator = { ',' };
        private readonly IAnnouncementService announcementService;
        private readonly UserSession userSession;

        public CreatePostView(IAnnouncementService announcementService, UserSession userSession)
        {
            this.InitializeComponent();
            this.announcementService = announcementService;
            this.userSession = userSession;
        }

        private async void Publish_Click(object sender, RoutedEventArgs e)
        {
            // Валідація простих полів
            var title = this.TitleTextBox.Text?.Trim() ?? string.Empty;
            var body = this.BodyTextBox.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Title cannot be empty.", ValidationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                MessageBox.Show("Body cannot be empty.", ValidationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(this.CategoryIdTextBox.Text?.Trim(), out var categoryId))
            {
                MessageBox.Show("Please enter a valid numeric Category ID.", ValidationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(this.LocationIdTextBox.Text?.Trim(), out var locationId))
            {
                MessageBox.Show("Please enter a valid numeric Location ID.", ValidationTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Парсинг optional полів (images / links)
            var imageUrls = string.IsNullOrWhiteSpace(this.ImagesTextBox.Text)
                ? Enumerable.Empty<string>()
                : this.ImagesTextBox.Text.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

            var links = string.IsNullOrWhiteSpace(this.LinksTextBox.Text)
                ? Enumerable.Empty<string>()
                : this.LinksTextBox.Text.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

            // Отримати userId — декілька способів (fallback)
            if (this.userSession?.IsLoggedIn != true || this.userSession.CurrentUser == null)
            {
                MessageBox.Show("You must be logged in to create a post. Please log in and try again.", "Authentication required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            int userId = this.userSession.CurrentUser.Id;

            try
            {
                // Виклик вашого сервісу
                await this.announcementService.CreateAnnouncementAsync(
                    userId,
                    title,
                    body,
                    categoryId,
                    locationId,
                    imageUrls,
                    links);

                MessageBox.Show("Announcement created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                // ПОВЕРНУТИСЬ НА ГОЛОВНУ СТОРІНКУ:
                // Припущення: вікно MainWindow має ContentControl з ім'ям MainContent
                // і його DataContext / логіка дозволяє навігацію назад.
                this.TryNavigateHome();
            }
            catch (ArgumentException aex)
            {
                MessageBox.Show(aex.Message, "Validation error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                // Логіка логування тут (якщо є ILogger) — наразі просто повідомлення
                MessageBox.Show($"Failed to create announcement: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.TryNavigateHome();
        }

        /// <summary>
        /// Attempt to navigate back to the home/main view.
        /// </summary>
        private void TryNavigateHome()
        {
            try
            {
                if (Application.Current?.MainWindow != null)
                {
                    var mw = Application.Current.MainWindow;
                    var mainContentProp = mw.GetType().GetProperty("MainContent");
                    if (mainContentProp != null)
                    {
                        var contentControl = mainContentProp.GetValue(mw) as ContentControl;
                        if (contentControl != null)
                        {
                            // Якщо у вас є метод NavigateHome на MainWindow — краще викликати його
                            var navigateMethod = mw.GetType().GetMethod("NavigateHome", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                            if (navigateMethod != null)
                            {
                                navigateMethod.Invoke(mw, null);
                                return;
                            }

                            // Інакше очищуємо / показуємо пустий
                            contentControl.Content = null;
                        }
                    }
                }
            }
            catch
            {
                // ignore navigation failure
            }
        }
    }
}

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
                MessageBox.Show("Title cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                MessageBox.Show("Body cannot be empty.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(this.CategoryIdTextBox.Text?.Trim(), out var categoryId))
            {
                MessageBox.Show("Please enter a valid numeric Category ID.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(this.LocationIdTextBox.Text?.Trim(), out var locationId))
            {
                MessageBox.Show("Please enter a valid numeric Location ID.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Парсинг optional полів (images / links)
            var imageUrls = string.IsNullOrWhiteSpace(this.ImagesTextBox.Text)
                ? Enumerable.Empty<string>()
                : this.ImagesTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

            var links = string.IsNullOrWhiteSpace(this.LinksTextBox.Text)
                ? Enumerable.Empty<string>()
                : this.LinksTextBox.Text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim());

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
                var created = await this.announcementService.CreateAnnouncementAsync(
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

        private int? GetCurrentUserIdFallback()
        {
            try
            {
                // 1) Application.Current.Properties["UserId"] — простий підхід
                if (Application.Current?.Properties != null &&
                    Application.Current.Properties.Contains("UserId") &&
                    Application.Current.Properties["UserId"] is int propId)
                {
                    return propId;
                }

                // 2) DataContext could contain a user id property (optional)
                if (this.DataContext != null)
                {
                    var dc = this.DataContext;
                    var idProp = dc.GetType().GetProperty("UserId");
                    if (idProp != null)
                    {
                        var val = idProp.GetValue(dc);
                        if (val is int idFromDc)
                        {
                            return idFromDc;
                        }

                        if (val is long longId)
                        {
                            return Convert.ToInt32(longId);
                        }
                    }
                }

                // 3) Try to find MainWindow and check its DataContext or a public property (if you have one)
                if (Application.Current?.MainWindow != null)
                {
                    var mw = Application.Current.MainWindow;
                    var mwProp = mw.GetType().GetProperty("CurrentUserId");
                    if (mwProp != null)
                    {
                        var val = mwProp.GetValue(mw);
                        if (val is int id)
                        {
                            return id;
                        }
                    }
                }
            }
            catch
            {
                // swallow and return null => means not logged in / not available
            }

            return null;
        }

        /// <summary>
        /// Спроба повернутися на головну — якщо MainWindow має MainContent — переключає його на головний View.
        /// Якщо у вас інший механізм навігації — замініть.
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

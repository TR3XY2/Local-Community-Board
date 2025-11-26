// <copyright file="EditAnnouncementView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Controls;
    using LocalCommunityBoard.Application.Interfaces;
    using LocalCommunityBoard.Domain.Entities;

    /// <summary>
    /// Represents a user control for editing an announcement.
    /// </summary>
    /// <remarks>This control is initialized with the provided <see cref="Announcement"/> object and  binds
    /// its data context to an <see cref="EditAnnouncementViewModel"/>. It provides  functionality for editing the
    /// announcement and invoking a callback upon saving.</remarks>
    public partial class EditAnnouncementView : UserControl
    {
        public EditAnnouncementView(
            Announcement announcement,
            IAnnouncementService announcementService,
            Func<Task> onSaveCallback)
        {
            this.InitializeComponent();

            this.DataContext = new EditAnnouncementViewModel(
                announcement,
                announcementService,
                onSaveCallback);
        }
    }
}

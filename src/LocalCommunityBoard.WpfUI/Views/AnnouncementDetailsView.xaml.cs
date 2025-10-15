// <copyright file="AnnouncementDetailsView.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System.Windows.Controls;
using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Represents a user control for displaying the details of an announcement.
/// </summary>
/// <remarks>This control is designed to display the details of a specific <see cref="Announcement"/>
/// object.  The provided <see cref="Announcement"/> instance is set as the data context for the control,  enabling
/// data binding to its properties.</remarks>
public partial class AnnouncementDetailsView : UserControl
{
    public AnnouncementDetailsView(Announcement announcement)
    {
        this.InitializeComponent();
        this.DataContext = announcement;
    }
}

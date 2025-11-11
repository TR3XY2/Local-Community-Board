// <copyright file="EditCommentWindow.xaml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.WpfUI.Views;

using System;
using System.Threading.Tasks;
using System.Windows;
using LocalCommunityBoard.Application.Interfaces;
using LocalCommunityBoard.Domain.Entities;

/// <summary>
/// Window for editing a comment.
/// </summary>
public partial class EditCommentWindow : Window
{
    private readonly Comment comment;
    private readonly ICommentService commentService;
    private readonly Func<Task>? onSaveCallback;

    /// <summary>
    /// Initializes a new instance of the <see cref="EditCommentWindow"/> class.
    /// </summary>
    /// <param name="comment">The comment to edit.</param>
    /// <param name="commentService">The comment service.</param>
    /// <param name="onSaveCallback">Callback to execute after successful save.</param>
    public EditCommentWindow(Comment comment, ICommentService commentService, Func<Task>? onSaveCallback = null)
    {
        this.InitializeComponent();
        this.comment = comment ?? throw new ArgumentNullException(nameof(comment));
        this.commentService = commentService ?? throw new ArgumentNullException(nameof(commentService));
        this.onSaveCallback = onSaveCallback;

        // Load current comment text
        this.CommentBodyBox.Text = comment.Body;
    }

    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        var newBody = this.CommentBodyBox.Text?.Trim();

        if (string.IsNullOrWhiteSpace(newBody))
        {
            MessageBox.Show(
                "Comment text cannot be empty.",
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        try
        {
            var success = await this.commentService.UpdateReportedCommentAsync(this.comment.Id, newBody);

            if (success)
            {
                MessageBox.Show(
                    "Comment updated successfully.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // Call callback to refresh the admin panel
                if (this.onSaveCallback != null)
                {
                    await this.onSaveCallback();
                }

                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show(
                    "Failed to update comment. It may have been deleted.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(
                ex.Message,
                "Validation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"An error occurred while updating the comment:\n\n{ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to cancel? All unsaved changes will be lost.",
            "Confirm Cancel",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}

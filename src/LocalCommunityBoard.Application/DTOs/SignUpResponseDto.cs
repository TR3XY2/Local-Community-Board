// <copyright file="SignUpResponseDto.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.DTOs
{
    /// <summary>
    /// Data transfer object for user registration response.
    /// </summary>
    public class SignUpResponseDto
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        public required string Username { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        public required string Email { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public required string Password { get; set; }

        /// <summary>
        /// Gets or sets the JWT token for immediate authentication after signup.
        /// </summary>
        public required string Token { get; set; }
    }
}

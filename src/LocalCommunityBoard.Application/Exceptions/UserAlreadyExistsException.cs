// <copyright file="UserAlreadyExistsException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Exceptions
{
    using System;

    /// <summary>
    /// Exception thrown when a user already exists in the system.
    /// </summary>
    public class UserAlreadyExistsException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAlreadyExistsException"/> class.
        /// </summary>
        public UserAlreadyExistsException()
            : base("User with this username or email already exists.")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public UserAlreadyExistsException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserAlreadyExistsException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public UserAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

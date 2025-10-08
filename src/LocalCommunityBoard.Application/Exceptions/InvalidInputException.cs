// <copyright file="InvalidInputException.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Application.Exceptions
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Exception thrown when input validation fails.
    /// </summary>
    public class InvalidInputException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        public InvalidInputException()
            : base("Invalid input data.")
        {
            this.Errors = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public InvalidInputException(string message)
            : base(message)
        {
            this.Errors = new List<string> { message };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        /// <param name="errors">The list of validation errors.</param>
        public InvalidInputException(List<string> errors)
            : base("Multiple validation errors occurred.")
        {
            this.Errors = errors;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidInputException"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidInputException(string message, Exception innerException)
            : base(message, innerException)
        {
            this.Errors = new List<string> { message };
        }

        /// <summary>
        /// Gets the list of validation errors.
        /// </summary>
        public List<string> Errors { get; }
    }
}

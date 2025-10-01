// <copyright file="ReportStatus.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LocalCommunityBoard.Domain.Enums
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents the status of a report in the system.
    /// </summary>
    public enum ReportStatus
    {
        /// <summary>
        /// The report is open and has not been reviewed yet.
        /// </summary>
        Open,

        /// <summary>
        /// The report has been reviewed.
        /// </summary>
        Reviewed,

        /// <summary>
        /// The report is closed and no further action is required.
        /// </summary>
        Closed,
    }
}

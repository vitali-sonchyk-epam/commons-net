﻿using ReportPortal.Shared.Reporter;

namespace ReportPortal.Shared
{
    public class ContextInfo
    {
        /// <summary>
        /// Current reporter to send results.
        /// </summary>
        public ILaunchReporter LaunchReporter { get; set; }
    }
}
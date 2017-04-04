﻿using System;

namespace Jobbr.Server.RavenDB.Model
{
    public class RecurringTrigger : JobTriggerBase
    {
        public DateTime? StartDateTimeUtc { get; set; }
        public DateTime? EndDateTimeUtc { get; set; }
        public string Definition { get; set; }
        public bool NoParallelExecution { get; set; }
    }
}
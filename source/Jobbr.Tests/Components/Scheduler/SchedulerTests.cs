﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jobbr.ComponentModel.JobStorage.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Tests.Components.Scheduler
{
    [TestClass]
    public class SchedulerTests : TestBase
    {
        private void AddJobRun(DateTime plannedStartDateTimeUtc, JobRunStates state)
        {
            var scheduledTrigger = new InstantTrigger() { JobId = this.demoJob1Id, IsActive = true };
            var demoJob = this.repository.GetJob(this.demoJob1Id);

            var jobRun = this.repository.SaveNewJobRun(demoJob, scheduledTrigger, plannedStartDateTimeUtc);
            jobRun.State = state;
            this.repository.Update(jobRun);
        }

        [TestMethod]
        public void SchedulerStarts_HasScheduledJobsFromPast_WillSetToOmitted()
        {
            var currentTime = new DateTime(2017, 04, 06, 0, 0, 0);
            this.currentTimeProvider.Set(currentTime);

            // Add a couple of jobruns
            this.AddJobRun(currentTime.AddDays(-1), JobRunStates.Scheduled);
            this.AddJobRun(currentTime.AddDays(-1), JobRunStates.Starting);
            this.AddJobRun(currentTime.AddDays(-1), JobRunStates.Completed);
            this.AddJobRun(currentTime.AddDays(-1), JobRunStates.Finishing);
            this.AddJobRun(currentTime.AddDays(-1), JobRunStates.Failed);

            this.scheduler.Start();

            var schedulerJobRuns = this.repository.GetJobRunsByState(JobRunStates.Scheduled);
            var omittedJobRuns = this.repository.GetJobRunsByState(JobRunStates.Omitted);

            Assert.AreEqual(0, schedulerJobRuns.Count, "The only past scheduled jobrun should have been set to omitted");
            Assert.AreEqual(1, omittedJobRuns.Count, "It's assumed that the previous scheduled jobrun is now omitted");
        }

        [TestMethod]
        public void SchedulerStarts_HasScheduledJobsInFuture_WillNotTouch()
        {
            var currentTime = new DateTime(2017, 04, 06, 0, 0, 0);
            this.currentTimeProvider.Set(currentTime);

            // Add a couple of jobruns
            this.AddJobRun(currentTime.AddDays(1), JobRunStates.Scheduled);

            this.scheduler.Start();

            var schedulerJobRuns = this.repository.GetJobRunsByState(JobRunStates.Scheduled);

            Assert.AreEqual(1, schedulerJobRuns.Count, "A future scheduled jobrun should not be omitted");

        }
    }
}

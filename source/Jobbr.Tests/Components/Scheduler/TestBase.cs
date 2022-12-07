using System.Collections.Generic;
using Jobbr.ComponentModel.Execution;
using Jobbr.ComponentModel.Execution.Model;
using Jobbr.ComponentModel.JobStorage.Model;
using Jobbr.Server.Scheduling;
using Jobbr.Server.Scheduling.Planer;
using Jobbr.Server.Storage;
using Moq;

namespace Jobbr.Tests.Components.Scheduler
{
    public class TestBase
    {
        protected long demoJob1Id = 1;
        protected JobbrRepository repository;
        protected DefaultScheduler scheduler;
        protected List<PlannedJobRun> lastIssuedPlan;
        protected PeriodicTimerMock periodicTimer;
        protected ManualTimeProvider currentTimeProvider;

        public TestBase()
        {
            this.repository = new JobbrRepository(null, new InMemoryJobStorageProvider());

            var executorMock = new Mock<IJobExecutor>();
            executorMock.Setup(e => e.OnPlanChanged(It.IsNotNull<List<PlannedJobRun>>())).Callback<List<PlannedJobRun>>(p => this.lastIssuedPlan = p);

            this.periodicTimer = new PeriodicTimerMock();

            this.currentTimeProvider = new ManualTimeProvider();

            var job = new Job();
            this.repository.AddJob(job);
            this.demoJob1Id = job.Id;

            this.scheduler = new DefaultScheduler(null, this.repository, executorMock.Object,
                new InstantJobRunPlaner(this.currentTimeProvider), new ScheduledJobRunPlaner(this.currentTimeProvider),
                new RecurringJobRunPlaner(null, this.repository, this.currentTimeProvider), new DefaultSchedulerConfiguration(),
                this.periodicTimer, this.currentTimeProvider);
        }
    }
}
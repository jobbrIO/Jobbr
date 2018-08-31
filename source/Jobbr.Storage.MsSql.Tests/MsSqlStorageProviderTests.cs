﻿using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using Jobbr.ComponentModel.JobStorage.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.SqlServer;

namespace Jobbr.Storage.MsSql.Tests
{
    [TestClass]
    public class MsSqlStorageProviderTests
    {
        private SqlConnection _sqlConnection;
        private MsSqlStorageProvider _storageProvider;
        private LocalDb _localDb;

        const string ConnectionString = "Data Source=localhost\\sqlexpress;Initial Catalog=JobbrTest;Integrated Security=True";

        [TestInitialize]
        public void SetupDatabaseInstance()
        {
            //this._localDb = new LocalDb("local");
            //this._sqlConnection = this._localDb.CreateSqlConnection();
            //this._sqlConnection.Open();

            //var sqlStatements = SqlHelper.SplitSqlStatements(File.ReadAllText("CreateSchemaAndTables.sql")).ToList();

            //foreach (var statement in sqlStatements)
            //{
            //    using (var command = this._sqlConnection.CreateCommand())
            //    {
            //        command.CommandText = statement;
            //        command.ExecuteNonQuery();
            //    }
            //}

            DropTablesIfExists();

            this._storageProvider = new MsSqlStorageProvider(new JobbrMsSqlConfiguration
            {
                ConnectionString = ConnectionString,
                Schema = "Jobbr",
                DialectProvider = new SqlServer2017OrmLiteDialectProvider()
            });
        }

        private static void DropTablesIfExists()
        {
            var factory = new OrmLiteConnectionFactory(ConnectionString, new SqlServer2017OrmLiteDialectProvider());
            var connection = factory.CreateDbConnection();
            connection.Open();

            if (connection.TableExists<Entities.JobRun>())
            {
                connection.DropTable<Entities.JobRun>();
            }

            if (connection.TableExists<Entities.Trigger>())
            {
                connection.DropTable<Entities.Trigger>();
            }

            if (connection.TableExists<Entities.Job>())
            {
                connection.DropTable<Entities.Job>();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            /*  this._sqlConnection.Close();*/
        }

        [TestMethod]
        public void GivenEmptyDatabase_WhenAddingJob_IdIsSet()
        {
            var job1 = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test"
            };

            var job2 = new Job
            {
                UniqueName = "testjob2",
                Type = "Jobs.Test2"
            };

            _storageProvider.AddJob(job1);
            _storageProvider.AddJob(job2);

            Assert.AreNotEqual(job1.Id, job2.Id);
        }

        [TestMethod]
        public void GivenJob_WhenQueryingById_IsReturned()
        {
            var job = new Job
            {
                UniqueName = "testjob",
                Type = "Jobs.Test"
            };

            _storageProvider.AddJob(job);

            var job2 = _storageProvider.GetJobById(job.Id);

            Assert.AreEqual(job.Id, job2.Id);
            Assert.AreEqual("testjob", job2.UniqueName);
            Assert.AreEqual("Jobs.Test", job2.Type);
        }

        [TestMethod]
        public void UniqueName_Is_Unique()
        {
            var job1 = new Job
            {
                UniqueName = "i-am-unique",
            };

            var job2 = new Job
            {
                UniqueName = "i-am-unique",
            };

            _storageProvider.AddJob(job1);

            try
            {
                _storageProvider.AddJob(job2);

                Assert.Fail("SqlException should have been raised");
            }
            catch (SqlException)
            {
            }
        }

        //[TestMethod]
        //public void GivenJob_WhenQueryingByUniqueName_IsReturned()
        //{
        //    var job = new Job
        //    {
        //        UniqueName = "testjob",
        //        Type = "Jobs.Test"
        //    };

        //    this._storageProvider.AddJob(job);

        //    var job2 = this._storageProvider.GetJobByUniqueName(job.UniqueName);

        //    Assert.AreEqual(job.Id, job2.Id);
        //    Assert.AreEqual("testjob", job2.UniqueName);
        //    Assert.AreEqual("Jobs.Test", job2.Type);
        //}

        //[TestMethod]
        //[Ignore]
        //public void GivenTwoJobs_WhenQueryingPaged_ResultIsPaged()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
        //    var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };
        //    var job3 = new Job { UniqueName = "testjob3", Type = "Jobs.Test3" };

        //    this._storageProvider.AddJob(job1);
        //    this._storageProvider.AddJob(job2);
        //    this._storageProvider.AddJob(job3);

        //    var jobs = this._storageProvider.GetJobs(0, 1);

        //    Assert.AreEqual(1, jobs.Count);
        //    Assert.AreEqual(job1.Id, jobs[0].Id);
        //}

        //[TestMethod]
        //[Ignore]
        //public void GivenTwoJobs_WhenQueryingPageTwo_PageTwoIsReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
        //    var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };
        //    var job3 = new Job { UniqueName = "testjob3", Type = "Jobs.Test3" };

        //    this._storageProvider.AddJob(job1);
        //    this._storageProvider.AddJob(job2);
        //    this._storageProvider.AddJob(job3);

        //    var jobs = this._storageProvider.GetJobs(1, 1);

        //    Assert.AreEqual(1, jobs.Count);
        //    Assert.AreEqual(job2.Id, jobs[0].Id);
        //}

        //[TestMethod]
        //public void GivenSomeTriggers_WhenQueryingForActiveTriggers_AllActiveTriggersAreReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
        //    var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

        //    this._storageProvider.AddJob(job1);
        //    this._storageProvider.AddJob(job2);

        //    var trigger1 = new InstantTrigger { IsActive = false };
        //    var trigger2 = new InstantTrigger { IsActive = true };
        //    var trigger3 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);
        //    this._storageProvider.AddTrigger(job1.Id, trigger2);
        //    this._storageProvider.AddTrigger(job2.Id, trigger3);

        //    var activeTriggers = this._storageProvider.GetActiveTriggers();

        //    Assert.AreEqual(2, activeTriggers.Count);
        //}

        //[TestMethod]
        //public void GivenSomeTriggers_WhenQueryingTriggersByJobId_OnlyTriggersOfThatJobAreReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };
        //    var job2 = new Job { UniqueName = "testjob2", Type = "Jobs.Test2" };

        //    this._storageProvider.AddJob(job1);
        //    this._storageProvider.AddJob(job2);

        //    var trigger1 = new InstantTrigger();
        //    var trigger2 = new InstantTrigger();
        //    var trigger3 = new InstantTrigger();

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);
        //    this._storageProvider.AddTrigger(job1.Id, trigger2);
        //    this._storageProvider.AddTrigger(job2.Id, trigger3);

        //    var triggersOfJob1 = this._storageProvider.GetTriggersByJobId(job1.Id);
        //    var triggersOfJob2 = this._storageProvider.GetTriggersByJobId(job2.Id);

        //    Assert.AreEqual(2, triggersOfJob1.Count);
        //    Assert.AreEqual(1, triggersOfJob2.Count);
        //}

        //[TestMethod]
        //public void GivenJobRun_WhenQueryingById_IsReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { Job = job1, Trigger = trigger1, PlannedStartDateTimeUtc = DateTime.UtcNow };

        //    this._storageProvider.AddJobRun(jobRun1);

        //    var jobRun2 = this._storageProvider.GetJobRunById(jobRun1.Id);

        //    Assert.AreEqual(jobRun1.Id, jobRun2.Id);
        //}

        //[TestMethod]
        //public void GivenTwoJobRuns_WhenQueryingPaged_ResultIsPaged()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = Create(job1.Id, trigger1.Id, DateTime.UtcNow);
        //    ////var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);

        //    var jobRuns = this._storageProvider.GetJobRuns(0, 1);

        //    Assert.AreEqual(1, jobRuns.Count);

        //    jobRuns = this._storageProvider.GetJobRuns(0, 2);

        //    Assert.AreEqual(2, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenTwoJobRuns_WhenQueryingForSpecificState_OnlyThoseJobRunsAreReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByState(JobRunStates.Failed);

        //    Assert.AreEqual(1, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenThreeJobRuns_WhenQueryingForSpecificStatePaged_ResultIsPaged()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByState(JobRunStates.Completed, 0, 1);

        //    Assert.AreEqual(1, jobRuns.Count);

        //    jobRuns = this._storageProvider.GetJobRunsByState(JobRunStates.Completed, 0, 2);

        //    Assert.AreEqual(2, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenThreeJobRuns_WhenQueryingByTrigger_AllJobRunsOfTriggerAreReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByTriggerId(job1.Id, trigger1.Id);

        //    Assert.AreEqual(3, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenThreeJobRuns_WhenQueryingByTriggerPaged_ResultIsPaged()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Failed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByTriggerId(job1.Id, trigger1.Id, 0, 2);

        //    Assert.AreEqual(2, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenThreeJobRunsOfChefkoch_WhenQueryingByUserDisplayName_ReturnsOnlyJobRunsOfThatUser()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true, UserDisplayName = "chefkoch" };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByUserDisplayName("chefkoch");

        //    Assert.AreEqual(3, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenThreeJobRunsOfChefkoch_WhenQueryingByUserDisplayNamePaged_ResultIsPaged()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true, UserDisplayName = "chefkoch" };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByUserDisplayName("chefkoch", 0, 2);

        //    Assert.AreEqual(2, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenThreeJobRunsOfozu_WhenQueryingByUserId_ReturnsOnlyJobRunsOfozu()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true, UserId = "ozu" };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByUserId("ozu");

        //    Assert.AreEqual(3, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenThreeJobRunsOfozu_WhenQueryingByUserIdPaged_ResultIsPaged()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true, UserId = "ozu" };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = DateTime.UtcNow, State = JobRunStates.Completed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var jobRuns = this._storageProvider.GetJobRunsByUserId("ozu", 0, 2);

        //    Assert.AreEqual(2, jobRuns.Count);
        //}

        //[TestMethod]
        //public void GivenEnabledTrigger_WhenDisabling_TriggerIsDisabled()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    this._storageProvider.DisableTrigger(job1.Id, trigger1.Id);

        //    var triggerFromDb = this._storageProvider.GetTriggerById(job1.Id, trigger1.Id);

        //    Assert.IsFalse(triggerFromDb.IsActive);
        //}

        //[TestMethod]
        //public void GivenDisabledTrigger_WhenEnabling_TriggerIsEnabled()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = false };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);
        //    this._storageProvider.EnableTrigger(job1.Id, trigger1.Id);

        //    var triggerFromDb = this._storageProvider.GetTriggerById(job1.Id, trigger1.Id);

        //    Assert.IsTrue(triggerFromDb.IsActive);
        //}

        //[TestMethod]
        //public void GivenJobRuns_WhenQueryingForLastJobRunByTrigger_LastJobRunIsReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var now = DateTime.UtcNow;

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = now, ActualStartDateTimeUtc = now, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = now.AddMinutes(1), ActualStartDateTimeUtc = now.AddMinutes(1), State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = now.AddMinutes(2), ActualStartDateTimeUtc = now.AddMinutes(2), State = JobRunStates.Completed };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var lastJobRun = this._storageProvider.GetLastJobRunByTriggerId(job1.Id, trigger1.Id, now.AddSeconds(30));

        //    Assert.AreEqual(jobRun1.Id, lastJobRun.Id);
        //}

        //[TestMethod]
        //public void GivenJobRuns_WhenQueryingForNextJobRunByTrigger_NextJobRunIsReturned()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var now = DateTime.UtcNow;

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = now, ActualStartDateTimeUtc = now, State = JobRunStates.Completed };
        //    var jobRun2 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = now.AddMinutes(1), ActualStartDateTimeUtc = now.AddMinutes(1), State = JobRunStates.Completed };
        //    var jobRun3 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = now.AddMinutes(2), State = JobRunStates.Scheduled };

        //    this._storageProvider.AddJobRun(jobRun1);
        //    this._storageProvider.AddJobRun(jobRun2);
        //    this._storageProvider.AddJobRun(jobRun3);

        //    var lastJobRun = this._storageProvider.GetNextJobRunByTriggerId(job1.Id, trigger1.Id, now.AddMinutes(1));
        //    Assert.AreEqual(jobRun3.Id, lastJobRun.Id);
        //}

        //[TestMethod]
        //public void GivenJobRun_WhenUpdatingProgress_ProgressIsUpdated()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger1 = new InstantTrigger { IsActive = true };

        //    this._storageProvider.AddTrigger(job1.Id, trigger1);

        //    var now = DateTime.UtcNow;

        //    var jobRun1 = new JobRun { JobId = job1.Id, TriggerId = trigger1.Id, PlannedStartDateTimeUtc = now, ActualStartDateTimeUtc = now, State = JobRunStates.Completed };

        //    this._storageProvider.AddJobRun(jobRun1);

        //    this._storageProvider.UpdateProgress(jobRun1.Id, 50);

        //    var jobRun2 = this._storageProvider.GetJobRunById(jobRun1.Id);

        //    Assert.AreEqual(50, jobRun2.Progress);
        //}

        //[TestMethod]
        //public void GivenJob_WhenUpdating_JobIsUpdated()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    job1.UniqueName = "test-uniquename";
        //    job1.Title = "test-title";
        //    job1.Type = "test-type";
        //    job1.Parameters = "test-parameters";

        //    this._storageProvider.Update(job1);

        //    var job1Reloaded = this._storageProvider.GetJobById(job1.Id);

        //    Assert.AreEqual("test-uniquename", job1Reloaded.UniqueName);
        //    Assert.AreEqual("test-title", job1Reloaded.Title);
        //    Assert.AreEqual("test-type", job1Reloaded.Type);
        //    Assert.AreEqual("test-parameters", job1Reloaded.Parameters);
        //}

        //[TestMethod]
        //public void GivenJobRun_WhenUpdating_JobRunIsUpdated()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger = new InstantTrigger {IsActive = true};

        //    this._storageProvider.AddTrigger(job1.Id, trigger);

        //    var jobRun = new JobRun {JobId = job1.Id, TriggerId = trigger.Id, PlannedStartDateTimeUtc = DateTime.UtcNow};

        //    this._storageProvider.AddJobRun(jobRun);

        //    var newPlannedStartDate = DateTime.UtcNow;
        //    var newActualStartDate = newPlannedStartDate.AddSeconds(1);
        //    var newEstimatedStartDate = newPlannedStartDate.AddMilliseconds(1);
        //    var newActualEndDate = newPlannedStartDate.AddMinutes(1);

        //    jobRun.JobParameters = "test-jobparameters";
        //    jobRun.InstanceParameters = "test-instanceparameters";
        //    jobRun.PlannedStartDateTimeUtc = newPlannedStartDate;
        //    jobRun.ActualStartDateTimeUtc = newActualStartDate;
        //    jobRun.EstimatedEndDateTimeUtc = newEstimatedStartDate;
        //    jobRun.ActualEndDateTimeUtc = newActualEndDate;

        //    this._storageProvider.Update(jobRun);

        //    var job1Reloaded = this._storageProvider.GetJobRunById(job1.Id);

        //    Assert.AreEqual("test-jobparameters", job1Reloaded.JobParameters);
        //    Assert.AreEqual("test-instanceparameters", job1Reloaded.InstanceParameters);
        //    Assert.AreEqual(newPlannedStartDate.ToString(CultureInfo.InvariantCulture), job1Reloaded.PlannedStartDateTimeUtc.ToString(CultureInfo.InvariantCulture));
        //    Assert.AreEqual(newActualStartDate.ToString(CultureInfo.InvariantCulture), job1Reloaded.ActualStartDateTimeUtc.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
        //    Assert.AreEqual(newEstimatedStartDate.ToString(CultureInfo.InvariantCulture), job1Reloaded.EstimatedEndDateTimeUtc.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
        //    Assert.AreEqual(newActualEndDate.ToString(CultureInfo.InvariantCulture), job1Reloaded.ActualEndDateTimeUtc.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
        //}

        //[TestMethod]
        //public void GivenInstantTrigger_WhenUpdating_TriggerIsUpdated()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger = new InstantTrigger();

        //    this._storageProvider.AddTrigger(job1.Id, trigger);

        //    var trigger2 = (InstantTrigger)this._storageProvider.GetTriggerById(job1.Id, trigger.Id);
        //    trigger2.Comment = "bla";
        //    trigger2.IsActive = true;
        //    trigger2.Parameters = "test-parameters";
        //    trigger2.UserId = "ozu";
        //    trigger2.DelayedMinutes = 5;

        //    this._storageProvider.Update(job1.Id, trigger2);

        //    var trigger2Reloaded = (InstantTrigger)this._storageProvider.GetTriggerById(job1.Id, trigger2.Id);

        //    Assert.AreEqual("bla", trigger2Reloaded.Comment);
        //    Assert.IsTrue(trigger2Reloaded.IsActive);
        //    Assert.AreEqual("test-parameters", trigger2Reloaded.Parameters);
        //    Assert.AreEqual("ozu", trigger2Reloaded.UserId);
        //    Assert.AreEqual(5, trigger2Reloaded.DelayedMinutes);
        //}

        //[TestMethod]
        //public void GivenScheduledTrigger_WhenUpdating_TriggerIsUpdated()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger = new ScheduledTrigger {StartDateTimeUtc = DateTime.UtcNow};

        //    this._storageProvider.AddTrigger(job1.Id, trigger);

        //    var trigger2 = (ScheduledTrigger)this._storageProvider.GetTriggerById(job1.Id, trigger.Id);

        //    var startDateTime = DateTime.UtcNow.AddHours(5);

        //    trigger2.Comment = "bla";
        //    trigger2.IsActive = true;
        //    trigger2.Parameters = "test-parameters";
        //    trigger2.UserId = "ozu";
        //    trigger2.StartDateTimeUtc = startDateTime;

        //    this._storageProvider.Update(job1.Id, trigger2);

        //    var trigger2Reloaded = (ScheduledTrigger)this._storageProvider.GetTriggerById(job1.Id, trigger2.Id);

        //    Assert.AreEqual("bla", trigger2Reloaded.Comment);
        //    Assert.IsTrue(trigger2Reloaded.IsActive);
        //    Assert.AreEqual("test-parameters", trigger2Reloaded.Parameters);
        //    Assert.AreEqual("ozu", trigger2Reloaded.UserId);
        //    Assert.AreEqual(startDateTime.ToString(CultureInfo.InvariantCulture), trigger2Reloaded.StartDateTimeUtc.ToString(CultureInfo.InvariantCulture));
        //}

        //[TestMethod]
        //public void GivenRecurringTrigger_WhenUpdating_TriggerIsUpdated()
        //{
        //    var job1 = new Job { UniqueName = "testjob1", Type = "Jobs.Test1" };

        //    this._storageProvider.AddJob(job1);

        //    var trigger = new RecurringTrigger();

        //    this._storageProvider.AddTrigger(job1.Id, trigger);

        //    var trigger2 = (RecurringTrigger)this._storageProvider.GetTriggerById(job1.Id, trigger.Id);

        //    var startDateTime = DateTime.UtcNow.AddHours(5);
        //    var endDateTime = DateTime.UtcNow.AddHours(7);

        //    trigger2.Comment = "bla";
        //    trigger2.IsActive = true;
        //    trigger2.Parameters = "test-parameters";
        //    trigger2.UserId = "ozu";
        //    trigger2.StartDateTimeUtc = startDateTime;
        //    trigger2.Definition = "* * * * *";
        //    trigger2.EndDateTimeUtc = endDateTime;
        //    trigger2.NoParallelExecution = true;

        //    this._storageProvider.Update(job1.Id, trigger2);

        //    var trigger2Reloaded = (RecurringTrigger)this._storageProvider.GetTriggerById(job1.Id, trigger2.Id);

        //    Assert.AreEqual("bla", trigger2Reloaded.Comment);
        //    Assert.IsTrue(trigger2Reloaded.IsActive);
        //    Assert.AreEqual("test-parameters", trigger2Reloaded.Parameters);
        //    Assert.AreEqual("ozu", trigger2Reloaded.UserId);
        //    Assert.AreEqual(startDateTime.ToString(CultureInfo.InvariantCulture), trigger2Reloaded.StartDateTimeUtc.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
        //    Assert.AreEqual(endDateTime.ToString(CultureInfo.InvariantCulture), trigger2Reloaded.EndDateTimeUtc.GetValueOrDefault().ToString(CultureInfo.InvariantCulture));
        //    Assert.AreEqual("* * * * *", trigger2Reloaded.Definition);
        //    Assert.IsTrue(trigger2Reloaded.NoParallelExecution);
        //}

        //[TestMethod]
        //public void GivenRunningDatabase_WhenCheckingAvailability_IsAvailable()
        //{
        //    Assert.IsTrue(this._storageProvider.IsAvailable());
        //}

        //[TestMethod]
        //public void GivenNonRunningDatabase_WhenCheckingAvailability_IsAvailable()
        //{
        //    var cmd = this._sqlConnection.CreateCommand();
        //    cmd.CommandText = $"DROP TABLE Jobbr.Jobs;";
        //    cmd.ExecuteNonQuery();

        //    Assert.IsFalse(this._storageProvider.IsAvailable());
        //}

        //[TestMethod]
        //public void GivenEmptyDatabase_WhenAddingJob_JobCountIsIncreased()
        //{
        //    var job = new Job
        //    {
        //        UniqueName = "testjob",
        //        Type = "Jobs.Test"
        //    };

        //    this._storageProvider.AddJob(job);

        //    var jobCount = this._storageProvider.GetJobsCount();

        //    Assert.AreEqual(1, jobCount);
        //}

        private static JobRun Create(long jobId, long triggerId, DateTime plannedStartTimeUtc, JobRunStates state = default(JobRunStates))
        {
            return new JobRun
            {
                Job = { Id = jobId },
                Trigger = { Id = triggerId },
                PlannedStartDateTimeUtc = plannedStartTimeUtc,
                State = state
            };
        }
    }
}

using System;
using Jobbr.Runtime.Core.Logging;

namespace Jobbr.Runtime.Core
{
    public class CoreRuntime
    {
        private static readonly ILog Logger = LogProvider.For<CoreRuntime>();

        private JobRunInfo jobInfo;

        private readonly JobActivator jobActivator;

        private RunWrapperFactory runWrapperFactory;

        public event EventHandler<StateChangedEventArgs> StateChanged;

        public event EventHandler<FinishingEventArgs> Finishing;

        public CoreRuntime(RuntimeConfiguration runtimeConfiguration)
        {
            var jobTypeResolver = new JobTypeResolver(runtimeConfiguration.JobTypeSearchAssembly);
            var serviceProvider = runtimeConfiguration.ServiceProvider ?? new DefaultServiceProvider();

            this.jobActivator = new JobActivator(jobTypeResolver, serviceProvider);
        }

        public void Execute(ExecutionMetadata executionMetadata)
        {
            this.jobInfo = jobRunInfo;

            var wasSuccessful = false;

            try
            {
                this.PublishState(JobRunState.Initializing);

                var jobTypeName = this.jobInfo.JobType;

                // Register additional dependencies in the DI if available and activate
                Logger.Debug($"Trying to register additional dependencies if supported.");
                this.jobActivator.AddDependencies(new RuntimeContext
                {
                    UserId = this.jobInfo.UserId,
                    UserDisplayName = this.jobInfo.UserDisplayName
                });

                // Create instance
                Logger.Debug($"Create instance of job based on the typename '{jobTypeName}'");
                var jobClassInstance = this.jobActivator.CreateInstance(jobTypeName);

                if (jobClassInstance == null)
                {
                    return;
                }

                // Create task as wrapper for calling the Run() Method
                Logger.Debug($"Create task as wrapper for calling the Run() Method");
                this.runWrapperFactory = new RunWrapperFactory(jobClassInstance.GetType(), this.jobInfo.JobParameter, this.jobInfo.InstanceParameter);

                var wrapper = this.runWrapperFactory.CreateWrapper(jobClassInstance);
                if (wrapper == null)
                {
                    Logger.Error("Unable to create a wrapper for the job");
                    return;
                }

                // Start 
                Logger.Debug("Starting Task to execute the Run()-Method.");

                wrapper.Start();
                this.PublishState(JobRunState.Processing);

                // Wait for completion
                wasSuccessful = wrapper.WaitForCompletion();
            }
            catch (Exception e)
            {
                Logger.FatalException("Exception in the Jobbr-Runtime. Please see details: ", e);
            }
            finally
            {
                this.PublishState(JobRunState.Finishing);

                this.OnFinishing(new FinishingEventArgs() { Successful = wasSuccessful });

                if (wasSuccessful)
                {
                    this.PublishState(JobRunState.Completed);
                }
                else
                {
                    this.PublishState(JobRunState.Failed);
                }
            }
        }

        private void PublishState(JobRunState state)
        {
            this.OnStateChanged(new StateChangedEventArgs() { State = state });
        }

        protected virtual void OnStateChanged(StateChangedEventArgs e)
        {
            try
            {
                this.StateChanged?.Invoke(this, e);

            }
            catch (Exception exception)
            {
                Logger.ErrorException($"Recipient of the event {nameof(this.OnStateChanged)} threw an execption", exception);
            }
        }

        protected virtual void OnFinishing(FinishingEventArgs e)
        {
            try
            {
                this.Finishing?.Invoke(this, e);

            }
            catch (Exception exception)
            {
                Logger.ErrorException($"Recipient of the event {nameof(this.OnFinishing)} threw an execption", exception);
            }
        }
    }
}
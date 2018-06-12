using System;
using System.Linq;
using Nop.Core.Caching;
using Nop.Core.Domain.Tasks;
using Nop.Core.Infrastructure;
using Nop.Services.Logging;

namespace Nop.Services.Tasks
{
    /// <summary>
    /// Task
    /// </summary>
    public partial class Task
    {
        #region Fields

        private bool? _enabled;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor for Task
        /// </summary>
        /// <param name="task">Task </param>
        public Task(ScheduleTask task)
        {
            ScheduleTask = task;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Initialize and execute task
        /// </summary>
        private void ExecuteTask()
        {
            var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();
            
            if (!Enabled)
                return;

            var type = Type.GetType(ScheduleTask.Type) ??
                //ensure that it works fine when only the type name is specified (do not require fully qualified names)
                AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(ScheduleTask.Type))
                .FirstOrDefault(t => t != null);
            if (type == null)
                throw new Exception($"Schedule task ({ScheduleTask.Type}) cannot by instantiated");

            object instance = null;
            try
            {
                instance = EngineContext.Current.Resolve(type);
            }
            catch
            {
                //try resolve
            }
            if (instance == null)
            {
                //not resolved
                instance = EngineContext.Current.ResolveUnregistered(type);
            }

            var task = instance as IScheduleTask;
            if (task == null)
                return;

            ScheduleTask.LastStart = DateTime.Now;
            //update appropriate datetime properties
            scheduleTaskService.UpdateTask(ScheduleTask);
            task.Execute();
            ScheduleTask.LastEnd = ScheduleTask.LastSuccess = DateTime.Now;
            //update appropriate datetime properties
            scheduleTaskService.UpdateTask(ScheduleTask);
        }

        /// <summary>
        /// Is task already running?
        /// </summary>
        /// <param name="scheduleTask">Schedule task</param>
        /// <returns>Result</returns>
        protected virtual bool IsTaskAlreadyRunning(ScheduleTask scheduleTask)
        {
            //task run for the first time
            if (!scheduleTask.LastStart.HasValue && !scheduleTask.LastEnd.HasValue)
                return false;

            var lastStart = scheduleTask.LastStart ?? DateTime.Now;

            //task already finished
            if (scheduleTask.LastEnd.HasValue && lastStart < scheduleTask.LastEnd)
                return false;

            //task wasn't finished last time
            if (lastStart.AddSeconds(scheduleTask.Seconds) <= DateTime.Now)
                return false;

            return true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="throwException">A value indicating whether exception should be thrown if some error happens</param>
        /// <param name="ensureRunOncePerPeriod">A value indicating whether we should ensure this task is run once per run period</param>
        public void Execute(bool throwException = false, bool ensureRunOncePerPeriod = true)
        {
            if (ScheduleTask == null || !Enabled)
                return;

            if (ensureRunOncePerPeriod)
            {
                //task already running
                if (IsTaskAlreadyRunning(ScheduleTask))
                    return;

                //validation (so nobody else can invoke this method when he wants)
                if (ScheduleTask.LastStart.HasValue && (DateTime.Now - ScheduleTask.LastStart).Value.TotalSeconds < ScheduleTask.Seconds)
                    //too early
                    return;
            }

            try
            {
                //get expiration time
                var expirationInSeconds = Math.Min(ScheduleTask.Seconds, 300) - 1;
                var expiration = TimeSpan.FromSeconds(expirationInSeconds);

                //execute task with lock
                var locker = EngineContext.Current.Resolve<ILocker>();
                locker.PerformActionWithLock(ScheduleTask.Type, expiration, ExecuteTask);
            }
            catch (Exception exc)
            {
                var scheduleTaskService = EngineContext.Current.Resolve<IScheduleTaskService>();

                ScheduleTask.Enabled = !ScheduleTask.StopOnError;
                ScheduleTask.LastEnd = DateTime.Now;
                scheduleTaskService.UpdateTask(ScheduleTask);

                //log error
                var logger = EngineContext.Current.Resolve<ILogger>();
                logger.Error($"Error while running the '{ScheduleTask.Name}' schedule task. {exc.Message}", exc);
                if (throwException)
                    throw;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Schedule task
        /// </summary>
        public ScheduleTask ScheduleTask { get; }

        /// <summary>
        /// A value indicating whether the task is enabled
        /// </summary>
        public bool Enabled
        {
            get
            {
                if (!_enabled.HasValue)
                    _enabled = ScheduleTask?.Enabled;

                    return _enabled.HasValue && _enabled.Value;
            }
            set => _enabled = value;
        }

        #endregion
    }
}

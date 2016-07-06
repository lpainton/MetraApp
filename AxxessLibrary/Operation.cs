using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Metra.Axxess
{
    /// <summary>
    /// An enum of valid op statuses.
    /// </summary>
    public enum OperationStatus
    {
        Ready,
        Working,
        Finished
    };

    /// <summary>
    /// An enum of valid op types.
    /// </summary>
    public enum OperationType
    {
        Boot,
        Firmware,
    };

    /// <summary>
    /// A container object for information needed in the creation of Operations.
    /// </summary>
    public class OpArgs
    {
        public IAxxessBoard Device { get; private set; }
        public string Path { get; private set; }
        public AxxessFirmwareToken Token { get; private set; }

        public OpArgs(IAxxessBoard device, string path = null, AxxessFirmwareToken token = null)
        {
            this.Device = device;
            this.Path = path;
            this.Token = token;
        }
    }

    /// <summary>
    /// Inherit this for op complete handling.
    /// </summary>
    public class OperationEventArgs : EventArgs
    {
        public OperationEventArgs() : base()
        {

        }
    }

    /// <summary>
    /// An event handler for completed operations.
    /// </summary>
    public delegate void OperationCompletedHandler(object sender, OperationEventArgs args);

    /// <summary>
    /// Provides an underlying abstract implementation for IOperation.
    /// See <see cref="IOperation">IOperation</see> for further details.    
    /// </summary>
    abstract class Operation : IOperation
    {
        public event OperationCompletedHandler Completed;

        public OperationStatus Status { get; protected set; }
        public String Message { get; protected set; }
        public OperationType Type { get; protected set; }
        public int OperationsCompleted { get; protected set; }
        public int TotalOperations { get; protected set; }
        public int Progress { get { return (OperationsCompleted * 100) / TotalOperations; } }

        //Timeout related properties and methods.  Provides timeout tracking features.
        public int Timeout { get; protected set; }
        protected int _timeoutCounter;
        protected int TimeoutCounter { get { return _timeoutCounter; } }
        protected void ResetTimeout() { _timeoutCounter = 0; }
        protected void IncrementTimeout() { _timeoutCounter++; }
        protected bool IsTimedOut { get { return _timeoutCounter >= Timeout; } }
        protected bool CanTimeOut { get; set; }

        public Thread WorkerThread { get; private set; }
        public ThreadStart WorkerMethod { get; private set; }

        public IAxxessBoard Device { get; private set; }
        public Exception Error { get; protected set; }

        internal Operation(IAxxessBoard device, OperationType type)
        {
            this.Type = type;

            this.WorkerMethod = this.DoWork;
            this.WorkerThread = new Thread(this.WorkerMethod);

            this.Device = device;

            this.OperationsCompleted = 0;
            this.TotalOperations = 1;
            this.Status = OperationStatus.Ready;
            this.Message = String.Empty;

            this._timeoutCounter = 0;
            this.Timeout = 0;
            this.CanTimeOut = false;

            this.Error = null;
        }

        /// <summary>
        /// Loops on work for the purposes of providing a continuously executing worker thread.
        /// To prevent looping, set Status away from Working.
        /// </summary>
        public virtual void DoWork()
        {
            while(this.Status.Equals(OperationStatus.Working))
            {
                this.Work();
            }
        }

        /// <summary>
        /// Method contains the actual work portion of the worker thread.
        /// </summary>
        public virtual void Work()
        {
            if (Timeout > 0)
            {
                if (CanTimeOut && IsTimedOut)
                {
                    this.Error = new TimeoutException("Error: The current operation timed out!");
                    Stop();
                }                    
                else IncrementTimeout();
            }
            return;
        }

        /// <summary>
        /// Starts the worker thread and sets the status to Working.
        /// </summary>
        public virtual void Start()
        {
            this.Status = OperationStatus.Working;
            this.WorkerThread.Start();
        }

        /// <summary>
        /// Stops the worker thread and sets status to finish, then calls dispose.
        /// </summary>
        public virtual void Stop()
        {
            this.Status = OperationStatus.Finished;
            this.WorkerThread.Abort();
            this.Dispose();
        }

        /// <summary>
        /// Override this method to handle any resources that need to be disposed of.
        /// </summary>
        public virtual void Dispose()
        {
            return;
        }

        /// <summary>
        /// Method calls any OnCompleted handlers added.
        /// </summary>
        public virtual void OnCompleted()
        {
            if (Completed != null)
                Completed(this, new OperationEventArgs());
        }
    
        #region Explicit IOperation Implementation
        OperationStatus IOperation.Status
        {
            get { return this.Status; }
        }

        String IOperation.Message
        {
            get { return this.Message; }
        }

        OperationType IOperation.Type
        {
            get { return this.Type; }
        }

        int IOperation.Progress
        {
            get { return this.Progress; }
        }

        int IOperation.Timeout
        {
            get { return this.Timeout; }
        }

        Thread IOperation.WorkerThread
        {
            get { return this.WorkerThread; }
        }

        Exception IOperation.Error
        {
            get { return this.Error; }
        }

        void IOperation.Start()
        {
            this.Start();
        }

        void IOperation.Stop()
        {
            this.Stop();
        }

        void IOperation.AddCompletedHandler(OperationCompletedHandler handler) { this.Completed += handler; }
        void IOperation.RemoveCompletedHandler(OperationCompletedHandler handler) { this.Completed -= handler; }
        #endregion
    }
}

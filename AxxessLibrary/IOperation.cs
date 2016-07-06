using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    /// <summary>
    /// Not currently used.
    /// </summary>
    public class OperationException : Exception 
    { 
        public OperationType Type { get; set; }
        public OperationException(IOperation op, string message = "") : base(message) 
        { this.Type = op.Type; }
    }

    /// <summary>
    /// Not currently used.
    /// </summary>
    public class OperationTimeoutException : OperationException
    {
        public int Timeout { get; set; }
        public OperationTimeoutException(IOperation op) :
            base(op, "The operation exceeded maximum number of allowed cycles.")
        { this.Timeout = op.Timeout; }
    }

    /// <summary>
    /// Interface represents a valid operation in the Axxess library.
    /// Operations should be atomic (as much as possible) and involve communication 
    /// with a connected device.  Two operations should also be serial and not
    /// be designed to run simultaneously.
    /// </summary>
    public interface IOperation
    {
        /// <summary>
        /// The Status of the op.
        /// </summary>
        OperationStatus Status { get; }
        /// <summary>
        /// Any message being broadcast by the op.  Can be empty.
        /// </summary>
        string Message { get; }
        /// <summary>
        /// The type of the operation.
        /// </summary>
        OperationType Type { get; }
        /// <summary>
        /// The thread doing the main work.
        /// </summary>
        Thread WorkerThread { get; }
        /// <summary>
        /// The progress till completion.  
        /// Mostly used to update progress bars.
        /// </summary>
        int Progress { get; }
        /// <summary>
        /// The timeout duration of the operation in cycles.
        /// </summary>
        int Timeout { get; }
        /// <summary>
        /// Any excetion caught by the operation.
        /// </summary>
        Exception Error { get; }
        /// <summary>
        /// Starts the op.
        /// </summary>
        void Start();
        /// <summary>
        /// Stops the op, aborting all threads.
        /// </summary>
        void Stop();
        /// <summary>
        /// Adds an event handler to be called when the op completes.
        /// </summary>
        /// <param name="handler">The handler to add.</param>
        void AddCompletedHandler(OperationCompletedHandler handler);
        /// <summary>
        /// Removes an event handler previously added.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        void RemoveCompletedHandler(OperationCompletedHandler handler);
    }
}

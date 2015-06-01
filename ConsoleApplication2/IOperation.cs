﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Metra.Axxess
{
    public class OperationException : Exception 
    { 
        public OperationType Type { get; set; }
        public OperationException(IOperation op, string message = "") : base(message) 
        { this.Type = op.Type; }
    }
    public class OperationTimeoutException : OperationException
    {
        public int Timeout { get; set; }
        public OperationTimeoutException(IOperation op) :
            base(op, "The operation exceeded maximum number of allowed cycles.")
        { this.Timeout = op.Timeout; }
    }

    public interface IOperation
    {
        OperationStatus Status { get; }
        OperationType Type { get; }
        Thread WorkerThread { get; }
        int Progress { get; }
        int Timeout { get; }
        Exception Error { get; }
        void Start();
        void Stop();
    }
}
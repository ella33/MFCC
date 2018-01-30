using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectMFCC.Models.Transactions
{
    public class WaitFor
    {
        public WaitFor(string lockType, string lockTable, object resource, int lockId, int trHasLock, int trWaitsLock)
        {
            LockType = lockType;
            LockTable = lockTable;
            LockRecord = resource;
            LockId = lockId;
            TransHasLock = trHasLock;
            TransWaitsLock = trWaitsLock;
        }

        public string LockType { get; set; }
        public string LockTable { get; set; }
        public int LockId { get; set; }
        public object LockRecord { get; set; }
        public int TransHasLock { get; set; }
        public int TransWaitsLock { get; set; }
    }
}
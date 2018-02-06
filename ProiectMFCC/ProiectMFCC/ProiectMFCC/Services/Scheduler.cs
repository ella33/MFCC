using ProiectMFCC.Models.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace ProiectMFCC.Services
{
    public class Scheduler
    {
        public static string WRITE = "write";
        public static string READ = "read";
        private Dictionary<object, Lock> locks;
        private  List<WaitFor> waitFor;

        public List<Transaction> Transactions { get; set; }

        private Object thisLock = new Object();

        public Scheduler(Dictionary<object, Lock> locks, List<Transaction> transactions, List<WaitFor> waitFor)
        {
            this.locks = locks;
            this.waitFor = waitFor;
            Transactions = transactions;
        }

        //read lock
        public string Rl(Transaction transaction, object resource, string table)
        {
            bool wait = true;
            while (wait)
            {
                lock (thisLock)
                {
                    Lock myLock = null;           
                    if (locks.ContainsKey(resource))
                    {
                        myLock = locks[resource];
                    }
                    int transactionID = transaction.Id;
                    if (myLock == null)
                    {                         
                        //create a read lock on resource
                        myLock = new Lock(READ, resource, transactionID, table);
                        locks.Add(resource, myLock);
                        wait = false;
                    }
                    else if (myLock.Type == WRITE)
                    {   
                        //already lock==> must wait
                        if (isDeadlock(transactionID, myLock.TransactionId))
                        {
                            //releaseLocks(transaction);
                            transaction.Status = ("aborted");
                            return "aborted";
                        }
                        if (!waitFor.Any(x => x.TransWaitsLock == transaction.Id && x.TransHasLock == myLock.TransactionId && x.LockRecord == resource))
                        {
                            WaitFor waitForWrite = new WaitFor(READ, table, resource, myLock.Id, myLock.TransactionId, transaction.Id);
                            waitFor.Add(waitForWrite);
                            //transaction.Status = "blocked";
                        }
               
                    }
                    else
                    {   
                        //shared lock ==> update lock
                        myLock.TransactionId = transactionID;
                        wait = false;
                    }                  
                }
                if (!wait)
                {
                    try
                    {
                        Thread.Sleep(500);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            return "success";
        }

        //write lock
        public string Wl(Transaction transaction, object resource, string table)
        {
            bool wait = true;
            while (wait)
            {
                lock (thisLock)
                {
                    Lock myLock = null;
                    if (locks.ContainsKey(resource))
                    {
                        myLock = locks[resource];
                    }
                    int transactionID = transaction.Id;
                    if (myLock == null)
                    {         
                        myLock = new Lock(WRITE, resource, transactionID, table);
                        locks.Add(resource, myLock);
                        wait = false;
                    }
                    else if (myLock.Type == READ)
                    {
                        myLock.Type = WRITE;
                        locks[resource] = myLock;
                        wait = false;
                    }
                    else
                    {                  
                        if (isDeadlock(transactionID, myLock.TransactionId))
                        {
                            //releaseLocks(transaction);
                            transaction.Status = "aborted";
                            return "aborted";
                        }
                        if (!waitFor.Any(x => x.TransWaitsLock == transaction.Id && x.TransHasLock == myLock.TransactionId && x.LockRecord == resource))
                        {
                            WaitFor waitForResource = new WaitFor(WRITE, table, resource, myLock.Id, myLock.TransactionId, transaction.Id);
                            waitFor.Add(waitForResource);
                           // transaction.Status = "blocked";
                        }
                    }
                }
                if (!wait)
                {
                    try
                    {
                        Thread.Sleep(500);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            return "success";
        }

        public void ReleaseLocks(Transaction transaction)
        {
            lock (thisLock)
            {
                List<Lock> allLocks = new List<Lock>(locks.Values);
                foreach (var llock in allLocks)
                {
                    if (llock.TransactionId == transaction.Id)
                    {  
                        locks.Remove(llock.RecordId);
                    }
                }

                if (transaction.Status != "aborted")
                {
                    Transactions.Remove(transaction);
                    transaction.Status = "committed";
                    Transactions.Add(transaction);
                }
                
                var wf = waitFor.Where(w => w.TransHasLock == transaction.Id);
                foreach (var wait in wf)
                { 
                    if (wait.TransHasLock == transaction.Id)
                    {
                        var resource = wait.LockRecord;
                        Transaction transBlocked = Transactions.FirstOrDefault(x => x.Id == wait.TransWaitsLock);
                        if (transBlocked != null)
                        {
                            Transactions.Remove(transBlocked);
                            transBlocked.Status = "active";
                            Transactions.Add(transBlocked);
                        }
                    }
                }
                waitFor.RemoveAll(w => w.TransHasLock == transaction.Id);
            }         
        }

        private bool isDeadlock(int transactionId, int ownerId)
        {
            foreach (WaitFor wait in waitFor)
            {
                if (wait.TransHasLock == transactionId && ownerId == (wait.TransWaitsLock))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
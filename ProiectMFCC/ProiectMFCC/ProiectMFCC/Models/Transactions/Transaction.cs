using System;
using System.Collections.Generic;

namespace ProiectMFCC.Models.Transactions
{
    public class ReversibleAction
    {
        public ReversibleAction(string database)
        {
            Database = database;
        }

        public string Action { get; set; }
        public string Reverse { get; set; }
        public string Database { get; set; }
    }

    public class Transaction
    {
        public static int number = 0;

        public Transaction(string status)
        {
            Id = ++number;
            Timestamp = DateTime.Now;
            Actions = new List<ReversibleAction>();
            Status = status;
        }

        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Status { get; set; }
       public List<ReversibleAction> Actions { get; set; }
    }
}
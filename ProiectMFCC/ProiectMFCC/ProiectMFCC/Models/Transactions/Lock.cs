using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProiectMFCC.Models.Transactions
{
    public class Lock
    {
        public static int nr = 0;

        public Lock(string type, object recordId, int transID, string table)
        {
            Id = ++nr;
            Type = type;
            RecordId = recordId;
            TransactionId = transID;
            Table = table;
        }

        public int Id { get; set; }
        public string Type { get; set; }
        public object RecordId { get; set; }
        public int TransactionId { get; set; }
        public string Table { get; set; }
    }
}
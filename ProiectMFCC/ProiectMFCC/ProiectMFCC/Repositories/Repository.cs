using ProiectMFCC.Models.Main;
using ProiectMFCC.Models.Transactions;
using ProiectMFCC.Services;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace ProiectMFCC.Repositories
{
    public class Repository
    {
        private Scheduler _scheduler;
        private static int id = new Random().Next();
        private const string connectionStringClients = @"Data Source=.\SQLEXPRESS;Initial Catalog=ClientDetailsMFCC;Integrated Security=True";
        private const string connectionStringProducts = @"Data Source=.\SQLEXPRESS;Initial Catalog=ProductDetailsMFCC;Integrated Security=True";

        public Repository(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public Cart GetCartByUser(string clientName)
        {
            using (var clientConn = new SqlConnection(connectionStringClients))
            {
                clientConn.Open();

                using (var productsConn = new SqlConnection(connectionStringProducts))
                {
                    productsConn.Open();

                    Transaction transaction = new Transaction("active");
                    _scheduler.Transactions.Add(transaction);
                    List<Item> buyItems = new List<Item>();
                    Cart cartStore = null;

                    try
                    {
                        //SqlTransaction sqlTransactionProd = productsConn.BeginTransaction("ProductsTransaction");
                        //SqlTransaction sqlTransactionClient = clientConn.BeginTransaction("ClientTransaction");

                        if (_scheduler.Rl(transaction, clientName, "Client") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        Client clientStore = GetClient("SELECT * FROM Client where Name='" + clientName + "'", clientConn);

                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Client where Name='" + clientName + "'" });

                        if (_scheduler.Rl(transaction, clientStore.CartId, "Cart") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        cartStore = GetCart("SELECT * FROM Cart where Id='" + clientStore.CartId + "'", clientConn);
                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Cart where Id='" + clientStore.CartId + "'" });

                        if (_scheduler.Rl(transaction, "Item", "Item") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        List<Instrument> instruments = new List<Instrument>();
                        List<Accessory> accessories = new List<Accessory>();

                        using (SqlCommand command = new SqlCommand("SELECT * FROM Item where CartID='" + cartStore.Id + "'", clientConn))
                        {
                            using (SqlDataReader reader = command.ExecuteReader())
                            {
                                transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Item where CartID='" + cartStore.Id + "'" });

                                if (_scheduler.Rl(transaction, "instrument", "Instrument") == "aborted")
                                {
                                    Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                                    return null;
                                }

                                if (_scheduler.Rl(transaction, "accessory", "Accessory") == "aborted")
                                {
                                    Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                                    return null;
                                }

                                while (reader.Read())
                                {
                                    string type = reader[2].ToString();
                                    string buyItemName = reader[1].ToString();

                                    if (type == "instrument")
                                    {
                                        instruments.AddRange(GetInstrument("SELECT * FROM Instrument where Name ='" + buyItemName + "'", productsConn));
                                        transaction.Actions.Add(new ReversibleAction(connectionStringProducts) { Action = "SELECT * FROM Instrument where Name ='" + buyItemName + "'" });
                                    }
                                    else if (type == "accessory")
                                    {
                                        accessories.AddRange(GetAccessory("SELECT * FROM Accessory where Name ='" + buyItemName + "'", productsConn));
                                        transaction.Actions.Add(new ReversibleAction(connectionStringProducts) { Action = "SELECT * FROM Accessory where Name ='" + buyItemName + "'" });
                                    }
                                }
                            }
                        }

                        cartStore.Items.AddRange(instruments);
                        cartStore.Items.AddRange(accessories);

                        _scheduler.ReleaseLocks(transaction);
                        //sqlTransactionProd.Commit();
                        //sqlTransactionClient.Commit();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    return cartStore;
                }
            }
        }

        public string AddToCart(string username, string itemName, string type)
        {
            using (var clientConn = new SqlConnection(connectionStringClients))
            {
                clientConn.Open();

                using (var productsConn = new SqlConnection(connectionStringProducts))
                {
                    productsConn.Open();

                    Transaction transaction = new Transaction("active");
                    _scheduler.Transactions.Add(transaction);

                    try
                    {
                        //SqlTransaction sqlTransactionProd = productsConn.BeginTransaction("ProductsTransaction");
                        //SqlTransaction sqlTransactionClient = clientConn.BeginTransaction("ClientTransaction");

                        if (_scheduler.Rl(transaction, username, "Client") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return "aborted";
                        }

                        Client clientStore = GetClient("SELECT * FROM Client where Name='" + username + "'", clientConn);
                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Client where Name='" + username + "'" });

                        String status = _scheduler.Rl(transaction, clientStore.CartId, "Cart");
                        if (status == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return "aborted";
                        }

                        Cart cartStore = GetCart("SELECT * FROM Cart where Id='" + clientStore.CartId + "'", clientConn);
                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Cart where Id='" + clientStore.CartId + "'" });

                        double price = 0;

                        if (type == "instrument")
                        {
                            if (_scheduler.Rl(transaction, "instrument", "Instrument") == "aborted")
                            {
                                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                                return "aborted";
                            }
                            var itemToAdd = GetInstrument("SELECT * FROM Instrument where Name ='" + itemName + "'", productsConn);
                            transaction.Actions.Add(new ReversibleAction(connectionStringProducts) { Action = "SELECT * FROM Instrument where Name ='" + itemName + "'" });
                            price = itemToAdd.First().Price;
                        }
                        else if (type == "accessory")
                        {
                            if (_scheduler.Rl(transaction, "accessory", "Accessory") == "aborted")
                            {
                                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                                return "aborted";
                            }

                            var itemToAdd = GetAccessory("SELECT * FROM Accessory where Name ='" + itemName + "'", productsConn);
                            transaction.Actions.Add(new ReversibleAction(connectionStringProducts) { Action = "SELECT * FROM Accessory where Name ='" + itemName + "'" });
                            price = itemToAdd.First().Price;
                        }

                        if (_scheduler.Wl(transaction, "Item", "Item") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return "aborted";
                        }

                        var newId = ++id;
                        var query = "INSERT into Item values('" + newId + "','" + itemName + "','" + type + "','" + cartStore.Id + "')";
                        var reverseQuery = "Delete from Item where Id = " + newId + " and Name='" + itemName + "' and CartId = " + cartStore.Id;
                        using (SqlCommand command = new SqlCommand(query, clientConn))
                        {
                            var res = command.ExecuteNonQuery();
                            transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = query, Reverse =reverseQuery} );
                        }

                        if (_scheduler.Wl(transaction, cartStore.Id, "Cart") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            Rollback(transaction, clientConn, productsConn);
                            _scheduler.ReleaseLocks(transaction);
                            //sqlTransactionClient.Rollback();
                            //sqlTransactionProd.Rollback();
                            return "aborted";
                        }

                        using (SqlCommand command = new SqlCommand("UPDATE Cart set TotalPrice=TotalPrice+" + price + " where Id='" + cartStore.Id + "'", clientConn))
                        {
                            var res = command.ExecuteNonQuery();
                            var queryUpdate = "UPDATE Cart set TotalPrice=TotalPrice+" + price + " where Id='" + cartStore.Id + "'";
                            var queryUpdateReverse = "UPDATE Cart set TotalPrice=TotalPrice-" + price + " where Id='" + cartStore.Id + "'";
                            transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = queryUpdate, Reverse = queryUpdateReverse });
                        }

                        ///////// TEST DEAKLOCK
                        Thread.Sleep(10000);
                        if (_scheduler.Rl(transaction, "Client", "Client") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            Rollback(transaction, clientConn, productsConn);
                            _scheduler.ReleaseLocks(transaction);
                            //sqlTransactionClient.Rollback();
                            //sqlTransactionProd.Rollback();
                            return "aborted";
                        }
                        //////////////// TEST DEADLOCK

                        //sqlTransactionProd.Commit();
                        //sqlTransactionClient.Commit();
                        _scheduler.ReleaseLocks(transaction);
                        return transaction.Status;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        transaction.Status = "aborted";
                        Rollback(transaction, clientConn, productsConn);
                        _scheduler.ReleaseLocks(transaction);
                        return "aborted";
                    }
                }
            }
        }

        public List<Item> DeleteItem(string clientName, string itemName)
        {
            using (var clientConn = new SqlConnection(connectionStringClients))
            {
                clientConn.Open();

                using (var productsConn = new SqlConnection(connectionStringProducts))
                {
                    productsConn.Open();

                    Transaction transaction = new Transaction("active");
                    _scheduler.Transactions.Add(transaction);

                    try
                    {
                        //SqlTransaction sqlTransactionProd = productsConn.BeginTransaction("ProductsTransaction");
                        //SqlTransaction sqlTransactionClient = clientConn.BeginTransaction("ClientTransaction");

                        if (_scheduler.Rl(transaction, clientName, "Client") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }
                        Client clientStore = GetClient("SELECT * FROM Client where Name='" + clientName + "'", clientConn);
                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Client where Name='" + clientName + "'" });

                        string status = _scheduler.Rl(transaction, clientStore.CartId, "Cart");
                        if (status == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        Cart cartStore = GetCart("SELECT * FROM Cart where Id='" + clientStore.CartId + "'", clientConn);
                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Cart where Id='" + clientStore.CartId + "'" });

                        double price = 0;
                        string removedType;

                        if (_scheduler.Rl(transaction, "instrument", "Instrument") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        var itemToAdd = GetInstrument("SELECT * FROM Instrument where Name ='" + itemName + "'", productsConn);
                        transaction.Actions.Add(new ReversibleAction(connectionStringProducts) { Action = "SELECT * FROM Instrument where Name ='" + itemName + "'" });

                        if (itemToAdd.Any())
                        {
                            price = itemToAdd.First().Price;
                            removedType = "instrument";
                        }
                        else
                        {
                            if (_scheduler.Rl(transaction, "accessory", "Accessory") == "aborted")
                            {
                                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                                return null;
                            }

                            var itemToAdd2 = GetAccessory("SELECT * FROM Accessory where Name ='" + itemName + "'", productsConn);
                            transaction.Actions.Add(new ReversibleAction(connectionStringProducts) { Action = "SELECT * FROM Accessory where Name ='" + itemName + "'" });
                            price = itemToAdd2.First().Price;
                            removedType = "accessory";
                        }


                        if (_scheduler.Wl(transaction, "Item", "Item") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        using (SqlCommand command = new SqlCommand("Delete from Item where Name='" + itemName + "' and CartId=" + cartStore.Id, clientConn))
                        {
                            var res = command.ExecuteNonQuery();
                            var deleteQuery = "Delete from Item where Name='" + itemName + "' and CartId=" + cartStore.Id;
                            var deleteQueryReverse = "INSERT into Item values('" + itemToAdd.First().Id + "','" + itemName + "','" + removedType + "','" + cartStore.Id + "')";
                            transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = deleteQuery, Reverse = deleteQueryReverse });
                        }

                        if (_scheduler.Wl(transaction, cartStore.Id, "Cart") == "aborted")
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            Rollback(transaction, clientConn, productsConn);
                            _scheduler.ReleaseLocks(transaction);
                            //sqlTransactionClient.Rollback();
                            //sqlTransactionProd.Rollback();
                            return null;
                        }

                        using (SqlCommand command = new SqlCommand("UPDATE Cart set TotalPrice=TotalPrice-" + price + " where Id='" + cartStore.Id + "'", clientConn))
                        {
                            var res = command.ExecuteNonQuery();
                            var upQuery = "UPDATE Cart set TotalPrice=TotalPrice-" + price + " where Id='" + cartStore.Id + "'";
                            var upQueryReverse = "UPDATE Cart set TotalPrice=TotalPrice+" + price + " where Id='" + cartStore.Id + "'";
                            transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = upQuery, Reverse = upQueryReverse });

                        }

                        //sqlTransactionProd.Commit();
                        //sqlTransactionClient.Commit();
                        _scheduler.ReleaseLocks(transaction);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
            return null;
        }

        //public Client ChangeEmailSqlTr(String username, String email)
        //{
        //    using (var clientConn = new SqlConnection(connectionStringClients))
        //    {
        //        clientConn.Open();

        //        Transaction transaction = new Transaction("active");
        //        _scheduler.Transactions.Add(transaction);

        //        try
        //        {
        //            SqlTransaction sqlTransactionClient = clientConn.BeginTransaction("ClientTransaction");

        //            if (_scheduler.Rl(transaction, username, "Client") == "aborted")
        //            {
        //                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
        //                return null;
        //            }

        //            var clientt = GetClient("SELECT * from Client where Name='" + username + "'", clientConn, sqlTransactionClient);

        //            string status = _scheduler.Wl(transaction, "Client", "Client");
        //            if (status == "aborted")
        //            {
        //                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
        //                return null;
        //            }

        //            using (SqlCommand command = new SqlCommand("UPDATE Client set Email='" + email + "' where Name='" + username + "'", clientConn, sqlTransactionClient))
        //            {
        //                var res = command.ExecuteNonQuery();
        //                var upQuery = "UPDATE Client set Email='" + email + "' where Name='" + username + "'";
        //                var upQueryReverse = "UPDATE Client set Email='" + clientt.Email + "' where Name='" + username + "'";
        //                transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = upQuery, Reverse = upQueryReverse });
        //            }

        //            Thread.Sleep(10000);

        //            //this lock is requested to simulate deadlock
        //            if (_scheduler.Rl(transaction, "Item", "Item") == "aborted")
        //            {
        //                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
        //                sqlTransactionClient.Rollback();
        //                //Rollback(transaction, clientConn, null);
        //                _scheduler.ReleaseLocks(transaction);
        //                return null;
        //            }

        //            if (_scheduler.Rl(transaction, username, "Client") == "aborted")
        //            {
        //                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
        //                sqlTransactionClient.Rollback();
        //                //Rollback(transaction, clientConn, null);
        //                _scheduler.ReleaseLocks(transaction);
        //                return null;
        //            }

        //            var client = GetClient("SELECT * FROM Client where Name='" + username + "'", clientConn);
        //            transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Client where Name='" + username + "'" });
        //            sqlTransactionClient.Commit();
        //            _scheduler.ReleaseLocks(transaction);
        //            return client;
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //        return null;
        //    }
        //}

        public Client ChangeEmail(String username, String email)
        {
            using (var clientConn = new SqlConnection(connectionStringClients))
            {
                clientConn.Open();

                Transaction transaction = new Transaction("active");
                _scheduler.Transactions.Add(transaction);

                try
                {
                    //SqlTransaction sqlTransactionClient = clientConn.BeginTransaction("ClientTransaction");

                    if (_scheduler.Rl(transaction, username, "Client") == "aborted")
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        return null;
                    }

                    var clientt = GetClient("SELECT * from Client where Name='" + username + "'", clientConn);

                    string status = _scheduler.Wl(transaction, "Client", "Client");
                    if (status == "aborted")
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        return null;
                    }

                    using (SqlCommand command = new SqlCommand("UPDATE Client set Email='" + email + "' where Name='" + username + "'", clientConn))
                    {
                        var res = command.ExecuteNonQuery();
                        var upQuery = "UPDATE Client set Email='" + email + "' where Name='" + username + "'";
                        var upQueryReverse = "UPDATE Client set Email='" + clientt.Email + "' where Name='" + username + "'";
                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = upQuery, Reverse = upQueryReverse });
                    }


                    //this lock is requested to simulate deadlock
                    //Thread.Sleep(10000);
                    //if (_scheduler.Rl(transaction, "Item", "Item") == "aborted")
                    //{
                    //    Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                    //    //sqlTransactionClient.Rollback();
                    //    Rollback(transaction, clientConn, null);
                    //    _scheduler.ReleaseLocks(transaction);
                    //    return null;
                    //}

                    if (_scheduler.Rl(transaction, username, "Client") == "aborted")
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        //sqlTransactionClient.Rollback();
                        Rollback(transaction, clientConn, null);
                        _scheduler.ReleaseLocks(transaction);
                        return null;
                    }

                    var client = GetClient("SELECT * FROM Client where Name='" + username + "'", clientConn);
                    transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Client where Name='" + username + "'" });
                    //sqlTransactionClient.Commit();
                    _scheduler.ReleaseLocks(transaction);
                    return client;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                return null;
            }
        }

        //public Client SimulateDeadlockWithChangeEmail(string username, string item)
        //{
        //    Thread.Sleep(5000);

        //    using (var clientConn = new SqlConnection(connectionStringClients))
        //    {
        //        clientConn.Open();

        //        Transaction transaction = new Transaction("active");
        //        _scheduler.Transactions.Add(transaction);

        //        try
        //        {
        //            //SqlTransaction sqlTransactionClient = clientConn.BeginTransaction("ClientTransaction");
        //            String status = _scheduler.Wl(transaction, "Item", "Item");
        //            if (status == "aborted")
        //            {
        //                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
        //                return null;
        //            }

        //            using (SqlCommand command = new SqlCommand("UPDATE Item set Type='accessory'" + " where CartId=2", clientConn))
        //            {
        //                var res = command.ExecuteNonQuery();
        //                var upQuery = "UPDATE Item set Type='accessory'" + " where CartId=2";
        //                var upQueryReverse = "UPDATE Item set Type='instrument'" + " where CartId=2";
        //                transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action =  upQuery, Reverse = upQueryReverse});
        //            }

        //            if (_scheduler.Rl(transaction, "Client", "Client") == "aborted")
        //            {
        //                Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
        //                //sqlTransactionClient.Rollback();
        //                Rollback(transaction, clientConn, null);
        //                _scheduler.ReleaseLocks(transaction);
        //                return null;
        //            }

        //            var client = GetClient("SELECT * FROM Client where Name='" + username + "'", clientConn);
        //            transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Client where Name='" + username + "'" });
        //            //sqlTransactionClient.Commit();
        //            _scheduler.ReleaseLocks(transaction);
        //            return client;
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e);
        //        }
        //        return null;

        //    }
        //}

        public Client SimulateDeadlockWithInsert(string username, string item)
        {
            using (var clientConn = new SqlConnection(connectionStringClients))
            {
                clientConn.Open();

                Transaction transaction = new Transaction("active");
                _scheduler.Transactions.Add(transaction);

                try
                {
                    if (_scheduler.Wl(transaction, "Client", "Client") == "aborted")
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        //sqlTransactionClient.Rollback();
                        Rollback(transaction, clientConn, null);
                        _scheduler.ReleaseLocks(transaction);
                        return null;
                    }

                    Thread.Sleep(5000);

                    //SqlTransaction sqlTransactionClient = clientConn.BeginTransaction("ClientTransaction");
                    String status = _scheduler.Wl(transaction, "Item", "Item");
                    if (status == "aborted")
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        Rollback(transaction, clientConn, null);
                        _scheduler.ReleaseLocks(transaction);
                        return null;
                    }

                    using (SqlCommand command = new SqlCommand("UPDATE Item set Type='accessory'" + " where CartId=2", clientConn))
                    {
                        var res = command.ExecuteNonQuery();
                        var upQuery = "UPDATE Item set Type='accessory'" + " where CartId=2";
                        var upQueryReverse = "UPDATE Item set Type='instrument'" + " where CartId=2";
                        transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = upQuery, Reverse = upQueryReverse });
                    }

                    

                    var client = GetClient("SELECT * FROM Client where Name='" + username + "'", clientConn);
                    transaction.Actions.Add(new ReversibleAction(connectionStringClients) { Action = "SELECT * FROM Client where Name='" + username + "'" });
                    //sqlTransactionClient.Commit();
                    _scheduler.ReleaseLocks(transaction);
                    return client;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                return null;

            }
        }

        private void Rollback(Transaction transaction, SqlConnection clientConn, SqlConnection prodConn)
        {
            transaction.Actions.Reverse();

            foreach (var action in transaction.Actions)
            {
                if (!string.IsNullOrEmpty(action.Reverse))
                {
                    SqlConnection conn = null;
                    if (action.Database == connectionStringClients)
                        conn = clientConn;
                    else if (action.Database == connectionStringProducts)
                        conn = prodConn;

                        using (SqlCommand command = new SqlCommand(action.Reverse, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private Cart GetCart(string query, SqlConnection conn)
        {
            using (SqlCommand command = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Int32.Parse(reader[0].ToString());
                        double price = Double.Parse(reader[1].ToString());
                        var cartStore = new Cart(id, price);
                        return cartStore;
                    }
                }
            }
            return null;
        }

        private List<Instrument> GetInstrument(string query, SqlConnection conn)
        {
            var list = new List<Instrument>();
            using (SqlCommand command = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var n = reader[0].ToString();
                        var c = reader[1].ToString();
                        var p = Double.Parse(reader[2].ToString());
                        var m = reader[3].ToString();
                        var ins = new Instrument(n, c, p, m);
                        list.Add(ins);
                    }
                }
            }
            return list;
        }

        private List<Accessory> GetAccessory(string query, SqlConnection conn)
        {
            var list = new List<Accessory>();
            using (SqlCommand command2 = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader2 = command2.ExecuteReader())
                {
                    while (reader2.Read())
                    {
                        var n = reader2[0].ToString();
                        var p = Double.Parse(reader2[1].ToString());
                        var m = reader2[2].ToString();
                        var i = reader2[3].ToString();
                        var acc = new Accessory(n, p, m, i);
                        list.Add(acc);
                    }
                }
            }
            return list;
        }

        //private Client GetClientSqlTr(string query, SqlConnection conn, SqlTransaction transaction)
        //{
        //    using (SqlCommand command = new SqlCommand(query, conn, transaction))
        //    {
        //        using (SqlDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                if (reader.HasRows)
        //                {
        //                    string name = reader[0].ToString();
        //                    string email = reader[1].ToString();
        //                    int cartID = Int32.Parse(reader[2].ToString());
        //                    return new Client(name, email, cartID);
        //                }
        //            }
        //        }
        //    }
        //    return null;
        //}

        private Client GetClient(string query, SqlConnection conn)
        {
            //using (SqlCommand command = new SqlCommand(query, conn, transaction))
            using (SqlCommand command = new SqlCommand(query, conn))
            {
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            string name = reader[0].ToString();
                            string email = reader[1].ToString();
                            int cartID = Int32.Parse(reader[2].ToString());
                            return new Client(name, email, cartID);
                        }
                    }
                }
            }
            return null;
        }
    }
}
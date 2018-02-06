using System;
using ProiectMFCC.Services;
using ProiectMFCC.Models.Entities;
using System.Collections.Generic;
using System.Data.SqlClient;
using ProiectMFCC.Models.Transactions;
using ProiectMFCC.Constants;
using System.Data;
using System.Diagnostics;

namespace ProiectMFCC.Repositories
{
    public class DonationsRepo
    {
        private Scheduler _scheduler;
        private static int id = new Random().Next();

        public DonationsRepo(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public List<Donation> GetDonationsByDonorName(string donorName)
        {
            using (var donationsConn = new SqlConnection(ConnectionStrings.donations))
            {
                donationsConn.Open();

                using (var projectsConn = new SqlConnection(ConnectionStrings.projects))
                {
                    projectsConn.Open();

                    Transaction t = new Transaction(TransactionStatus.ACTIVE);
                    _scheduler.Transactions.Add(t);
                    List<Donation> donations = new List<Donation>();
                    Donor donor = new Donor();

                    try
                    {
                        //TODO fix here
                        t.Actions.Add(new ReversibleAction(ConnectionStrings.donations) { Action = "SELECT * FROM Client where Name='" + "" + "'" });
                        if (_scheduler.Rl(t, donorName, Tables.donor) == TransactionStatus.ABORTED)
                        {
                            Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        int donorCountryId = 0, donorId = 0;
                        SqlCommand sqlcmd = new SqlCommand();
                        sqlcmd.Connection = donationsConn;
                        sqlcmd.CommandType = CommandType.Text;
                        sqlcmd.Parameters.Add("DonorName", SqlDbType.VarChar).Value = donorName;
                        sqlcmd.CommandText = "SELECT * FROM donor WHERE Name = @DonorName";
                        SqlDataReader dr = sqlcmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                donorId = int.Parse(dr["ID"].ToString());
                                donorCountryId = int.Parse(dr["countryId"].ToString());
                            }
                        }
                        dr.Close();
                        sqlcmd.Parameters.Clear();
                        //TODO fix here
                        t.Actions.Add(new ReversibleAction(ConnectionStrings.donations) { Action = "SELECT * FROM Client where Name='" + "" + "'" });

                        if (_scheduler.Rl(t, "country", Tables.country).Equals(TransactionStatus.ABORTED))
                        {
                            Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        sqlcmd = new SqlCommand();
                        sqlcmd.Connection = donationsConn;
                        sqlcmd.CommandType = CommandType.Text;
                        sqlcmd.Parameters.Add("CountryId", SqlDbType.Int).Value = donorCountryId;
                        sqlcmd.CommandText = "SELECT * FROM country WHERE ID = @CountryId";
                        dr = sqlcmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                donor = new Donor(donorId, donorName, dr["Name"].ToString());
                            }
                        }
                        dr.Close();
                        sqlcmd.Parameters.Clear();

                        //TODO fix here
                        t.Actions.Add(new ReversibleAction(ConnectionStrings.donations) { Action = "SELECT * FROM Cart where Id='" + "" + "'" });

                        if (_scheduler.Rl(t, "donation", Tables.project).Equals(TransactionStatus.ABORTED))
                        {
                            Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        sqlcmd = new SqlCommand();
                        sqlcmd.Connection = donationsConn;
                        sqlcmd.CommandType = CommandType.Text;
                        sqlcmd.Parameters.Add("DonorId", SqlDbType.Int).Value = donor.Id;
                        sqlcmd.CommandText = "SELECT * FROM donation WHERE donorId = @DonorId";
                        dr = sqlcmd.ExecuteReader();

                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                donations.Add(new Donation(
                                    int.Parse(dr["ID"].ToString()),
                                    int.Parse(dr["ProjectId"].ToString()),
                                    donor.Id,
                                    DateTime.Parse(dr["SubmissionDate"].ToString()).ToString("MMM dd, yyyy"),
                                    int.Parse(dr["Amount"].ToString()))
                                );
                            }
                        }
                        dr.Close();
                        sqlcmd.Parameters.Clear();
                        _scheduler.ReleaseLocks(t);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    return donations;
                }
            }
        }

        public void AddDonor(string donorName, int countryId)
        {
            using (var donationsConn = new SqlConnection(ConnectionStrings.donations))
            {
                Transaction t = new Transaction(TransactionStatus.ACTIVE);
                _scheduler.Transactions.Add(t);
                t.Actions.Add(new ReversibleAction(ConnectionStrings.donations) { Action = "INSERT INTO" });
                if (_scheduler.Wl(t, donorName, Tables.donor) == TransactionStatus.ABORTED)
                {
                    Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                    return;
                }

                donationsConn.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = donationsConn;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "INSERT INTO donor(name, countryId) VALUES(@name, @cid)";
                sqlcmd.Parameters.Add("name", SqlDbType.VarChar).Value = donorName;
                sqlcmd.Parameters.Add("cid", SqlDbType.Int).Value = countryId;
                sqlcmd.ExecuteNonQuery();
                sqlcmd.Parameters.Clear();
            }
        }

        public void DeleteDonation(int id)
        {
            using (var donationsConn = new SqlConnection(ConnectionStrings.donations))
            {
                donationsConn.Open();

                using (var projectsConn = new SqlConnection(ConnectionStrings.projects))
                {
                    projectsConn.Open();
                    Transaction t = new Transaction(TransactionStatus.ACTIVE);
                    _scheduler.Transactions.Add(t);
                    t.Actions.Add(new ReversibleAction(ConnectionStrings.donations) { Action = "SELECT projectId FROM donation WHERE ID=" + id.ToString() });
                    if (_scheduler.Rl(t, id, Tables.donation) == TransactionStatus.ABORTED)
                    {
                        Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        return;
                    }
                    SqlCommand sqlcmd = new SqlCommand();
                    sqlcmd.Connection = donationsConn;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "SELECT * FROM donation WHERE ID=@id";
                    sqlcmd.Parameters.Add("id", SqlDbType.Int).Value = id;
                    SqlDataReader dr = sqlcmd.ExecuteReader();
                    string projectId = "", donorId = "", amount = "";
                    string submissionDate = "";
                    if (dr.HasRows)
                    {
                        while (dr.Read())
                        {
                            projectId = dr["ProjectId"].ToString();
                            donorId = dr["DonorId"].ToString();
                            amount = dr["Amount"].ToString();
                            submissionDate = dr["SubmissionDate"].ToString();
                        }
                    }
                    sqlcmd.Parameters.Clear();
                    dr.Close();

                    SqlCommand delcmd = new SqlCommand();
                    delcmd.Connection = donationsConn;
                    delcmd.CommandType = CommandType.Text;
                    delcmd.CommandText = "DELETE from donation WHERE ID=@id";
                    delcmd.Parameters.Add("id", SqlDbType.VarChar).Value = id;
                    delcmd.ExecuteNonQuery();
                    delcmd.Parameters.Clear();

                    t.Actions.Add(new ReversibleAction(ConnectionStrings.donations)
                    {
                        Action = "DELECT from donation WHERE ID=" + id.ToString(),
                        Reverse = "INSERT INTO donation(ProjectId, DonorId, Amount, SubmissionDate)" +
                            "VALUES(" + projectId + "," + donorId + "," + amount + "," + submissionDate + ")"
                    });

                    if (_scheduler.Wl(t, id, Tables.donation) == TransactionStatus.ABORTED)
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        Rollback(t, donationsConn, projectsConn);
                        _scheduler.ReleaseLocks(t);
                        return;
                    }

                    SqlCommand upcmd = new SqlCommand();
                    upcmd.Connection = projectsConn;
                    upcmd.CommandType = CommandType.Text;
                    upcmd.CommandText = "UPDATE project SET Funds=Funds-@amount WHERE ID=@pid";
                    upcmd.Parameters.Add("amount", SqlDbType.Int).Value = int.Parse(amount);
                    upcmd.Parameters.Add("pid", SqlDbType.Int).Value = int.Parse(projectId);
                    upcmd.ExecuteNonQuery();
                    upcmd.Parameters.Clear();

                    t.Actions.Add(new ReversibleAction(ConnectionStrings.projects)
                    {
                        Action = "UPDATE project SET Funds=Funds-" + amount + " WHERE ID=" + id,
                        Reverse = "UPDATE project SET Funds=Funds+" + amount + " WHERE ID=" + id
                    });

                    if (_scheduler.Wl(t, "Funds", Tables.project).Equals(TransactionStatus.ABORTED))
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        Rollback(t, donationsConn, projectsConn);
                        _scheduler.ReleaseLocks(t);
                        return;
                    }

                    _scheduler.ReleaseLocks(t);
                }
            }
        }

        public void AddDonation(string donorName, string projectName, int amount)
        {
            using (var donationsConn = new SqlConnection(ConnectionStrings.donations))
            {
                donationsConn.Open();

                using (var projectsConn = new SqlConnection(ConnectionStrings.projects))
                {
                    projectsConn.Open();

                    Transaction t = new Transaction(TransactionStatus.ACTIVE);
                    _scheduler.Transactions.Add(t);

                    t.Actions.Add(new ReversibleAction(ConnectionStrings.donations)
                    {
                        Action = "SELECT projectId from project WHERE Name=" + projectName
                    });
                    if (_scheduler.Rl(t, "projectId", Tables.project).Equals(TransactionStatus.ABORTED))
                    {
                        Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        return;
                    }

                    SqlCommand sqlcmd = new SqlCommand();
                    sqlcmd.Connection = projectsConn;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "SELECT ID FROM project WHERE Name=@pname";
                    sqlcmd.Parameters.Add("pname", SqlDbType.VarChar).Value = projectName;
                    int projectId = Convert.ToInt32(sqlcmd.ExecuteScalar());
                    sqlcmd.Parameters.Clear();

                    t.Actions.Add(new ReversibleAction(ConnectionStrings.donations)
                    {
                        Action = "SELECT ID from donor WHERE name=" + donorName
                    });
                    if (_scheduler.Rl(t, "donorId", Tables.donor).Equals(TransactionStatus.ABORTED))
                    {
                        Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        return;
                    }

                    sqlcmd.Connection = donationsConn;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "SELECT ID FROM donor WHERE name=@dname";
                    sqlcmd.Parameters.Add("dname", SqlDbType.VarChar).Value = donorName;
                    int donorId = Convert.ToInt32(sqlcmd.ExecuteScalar());
                    sqlcmd.Parameters.Clear();

                    sqlcmd.Connection = donationsConn;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "INSERT INTO donation(ProjectId, DonorId, Amount) VALUES(@pid, @did, @a)";
                    sqlcmd.Parameters.Add("pid", SqlDbType.Int).Value = projectId;
                    sqlcmd.Parameters.Add("did", SqlDbType.Int).Value = donorId;
                    sqlcmd.Parameters.Add("a", SqlDbType.Int).Value = amount;
                    sqlcmd.ExecuteNonQuery();
                    sqlcmd.Parameters.Clear();

                    t.Actions.Add(new ReversibleAction(ConnectionStrings.donations)
                    {
                        Action = "INSERT INTO donation(ProjectId, DonorId, Amount) VALUES(" + projectId.ToString() + "," + donorId.ToString() + "," + amount.ToString() + ")",
                        Reverse = "DELETE from donation WHERE ProjectId=" + projectId.ToString() + " AND DonorId=" + donorId.ToString() + " AND Amount=" + amount.ToString()
                    });
                    if (_scheduler.Wl(t, "donorId", Tables.donation).Equals(TransactionStatus.ABORTED))
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        Rollback(t, donationsConn, projectsConn);
                        _scheduler.ReleaseLocks(t);
                        return;
                    }

                    sqlcmd.Connection = projectsConn;
                    sqlcmd.CommandType = CommandType.Text;
                    sqlcmd.CommandText = "UPDATE project SET Funds=Funds+@amount WHERE ID=@pid";
                    sqlcmd.Parameters.Add("pid", SqlDbType.Int).Value = projectId;
                    sqlcmd.Parameters.Add("amount", SqlDbType.Int).Value = amount;
                    sqlcmd.ExecuteNonQuery();
                    sqlcmd.Parameters.Clear();

                    t.Actions.Add(new ReversibleAction(ConnectionStrings.projects)
                    {
                        Action = "UPDATE project SET Funds=Funds+" + amount.ToString() + " WHERE ID=" + projectId.ToString(),
                        Reverse = "UPDATE project SET Funds=Funds-" + amount.ToString() + " WHERE ID=" + projectId.ToString()
                    });
                    if (_scheduler.Wl(t, "Funds", Tables.project).Equals(TransactionStatus.ABORTED))
                    {
                        Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                        Rollback(t, donationsConn, projectsConn);
                        _scheduler.ReleaseLocks(t);
                        return;
                    }

                    _scheduler.ReleaseLocks(t);
                }
            }
        }

        private void Rollback(Transaction transaction, SqlConnection donationConn, SqlConnection projectsConn)
        {
            transaction.Actions.Reverse();

            foreach (var action in transaction.Actions)
            {
                if (!string.IsNullOrEmpty(action.Reverse))
                {
                    SqlConnection conn = null;
                    if (action.Database == ConnectionStrings.donations)
                        conn = donationConn;
                    else if (action.Database == ConnectionStrings.projects)
                        conn = projectsConn;

                    using (SqlCommand command = new SqlCommand(action.Reverse, conn))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

    }
}

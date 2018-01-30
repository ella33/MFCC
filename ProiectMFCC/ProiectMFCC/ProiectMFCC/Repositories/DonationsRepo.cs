using System;
using ProiectMFCC.Services;
using ProiectMFCC.Models.Entities;
using System.Collections.Generic;
using System.Data.SqlClient;
using ProiectMFCC.Models.Transactions;
using ProiectMFCC.Constants;
using System.Data;

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
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
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

                        if (_scheduler.Rl(t, donorCountryId, Tables.country).Equals(TransactionStatus.ABORTED))
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
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

                        if (_scheduler.Rl(t, "Project", Tables.project).Equals(TransactionStatus.ABORTED))
                        {
                            Console.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                            return null;
                        }

                        sqlcmd = new SqlCommand();
                        sqlcmd.Connection = donationsConn;
                        sqlcmd.CommandType = CommandType.Text;
                        sqlcmd.Parameters.Add("DonorId", SqlDbType.Int).Value = donor.Id;
                        sqlcmd.CommandText = "SELECT * FROM donation WHERE ID = @DonorId";
                        dr = sqlcmd.ExecuteReader();

                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                donations.Add(new Donation(int.Parse(dr["ProjectId"].ToString()),
                                    donor.Id,
                                    DateTime.Parse(dr["SubmissionDate"].ToString()),
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
    }
}
using ProiectMFCC.Constants;
using ProiectMFCC.Models.Entities;
using ProiectMFCC.Models.Transactions;
using ProiectMFCC.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace ProiectMFCC.Repositories
{
    public class CountriesRepo
    {
        private Scheduler _scheduler;

        public CountriesRepo(Scheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public List<Country> GetCountries()
        {
            List<Country> countries = new List<Country>();
            using (var donationsConn = new SqlConnection(ConnectionStrings.donations))
            {
                donationsConn.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = donationsConn;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "SELECT * FROM country";
                SqlDataReader dr = sqlcmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        countries.Add(new Country(int.Parse(dr["ID"].ToString()), dr["Name"].ToString()));
                    }
                }
                return countries;
            }
        }

        public int AddCountry(string countryName)
        {
            using (var donationsConn = new SqlConnection(ConnectionStrings.donations))
            {
                Transaction t = new Transaction(TransactionStatus.ACTIVE);
                _scheduler.Transactions.Add(t);
                t.Actions.Add(new ReversibleAction(ConnectionStrings.donations) { Action = "INSERT INTO" });
                if (_scheduler.Wl(t, countryName, Tables.country) == TransactionStatus.ABORTED)
                {
                    Debug.WriteLine("[ABORT]: Deadlock occured. The transaction was aborted");
                    return -1;
                }
                donationsConn.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = donationsConn;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "INSERT INTO country(Name) VALUES(@cn);SELECT SCOPE_IDENTITY();";
                sqlcmd.Parameters.Add("cn", SqlDbType.VarChar).Value = countryName;

                int res = Convert.ToInt32(sqlcmd.ExecuteScalar());
                _scheduler.ReleaseLocks(t);
                return res;
            }
        }
    }
}
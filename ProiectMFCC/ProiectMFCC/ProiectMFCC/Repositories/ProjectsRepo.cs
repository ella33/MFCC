using ProiectMFCC.Models.Entities;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProiectMFCC.Repositories
{
    public class ProjectsRepo
    {
        public List<Project> GetProjects()
        {
            List<Project> projects = new List<Project>();
            using (var projectsConn = new SqlConnection(ConnectionStrings.projects))
            {
                projectsConn.Open();
                SqlCommand sqlcmd = new SqlCommand();
                sqlcmd.Connection = projectsConn;
                sqlcmd.CommandType = CommandType.Text;
                sqlcmd.CommandText = "SELECT * FROM project";
                SqlDataReader dr = sqlcmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        projects.Add(new Project(
                            int.Parse(dr["ID"].ToString()),
                            dr["Name"].ToString(), 
                            dr["Summary"].ToString(),
                            int.Parse(dr["Funds"].ToString())
                            )
                        );
                    }
                }
                dr.Close();
                return projects;
            }
        }
    }
}
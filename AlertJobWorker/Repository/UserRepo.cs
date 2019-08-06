using Dapper;
using System.Configuration;
using System.Data.SqlClient;

namespace AlertJobWorker
{
    public static class UserRepo
    {
        private static string connectionStr = ConfigurationManager.ConnectionStrings["WebCrawler3Security"].ConnectionString;

        public static string GetHashPasswordByUserId(int userId)
        {
            string hashPassword = "";

            using (var conn = new SqlConnection(connectionStr))
            {
                string sql = "SELECT [PasswordHash] FROM [Security].[Users] WHERE [UserID] = '" + userId + "'";
                hashPassword = conn.ExecuteScalar<string>(sql);
            }

            return hashPassword;
        }

        public static string GetUserNameByUserId(int userId)
        {
            string userName = "";

            using (var conn = new SqlConnection(connectionStr))
            {
                string sql = "SELECT [LoginName] FROM [Security].[Users] WHERE [UserID] = '" + userId + "'";
                userName = conn.ExecuteScalar<string>(sql);
            }

            return userName;
        }
    }
}

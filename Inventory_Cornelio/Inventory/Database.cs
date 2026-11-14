using System;
using System.Data.OleDb;
using System.IO;

namespace Inventory
{
    public static class Database
    {
        public static readonly String DBpATH =
            Path.GetFullPath(
                Path.Combine (
                        AppDomain.CurrentDomain.BaseDirectory,
                            @"..\..\sampledb.accdb"
                    )
                );
        private static readonly string connectionString = @"Provider = Microsoft.ACE.OLEDB.12.0; Data Source=" + DBpATH + ";Persist Security Info=False;";
        
        public static OleDbConnection GetConnection ()
        {
            return new OleDbConnection(connectionString);
        }
    }

    public static class CurrentUser
    {
        public static int UserID { get; private set; }
        public static string Username { get; private set; }
        public static string FullName { get; private set; }
        public static string Role { get; private set; }

        public static string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FullName)) return FullName.Trim();
                if (!string.IsNullOrWhiteSpace(Username)) return Username.Trim();
                return "User";
            }
        }

        public static void Set(int userId, string username, string fullName, string role)
        {
            UserID = userId;
            Username = username;
            FullName = fullName;
            Role = role;
        }
    }
}

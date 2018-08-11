using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.Sqlite;

namespace Messaging
{
    public class DataSource : IDisposable
    {
        private readonly SqliteConnection _conn;

        public DataSource()
        {
            const string connectionString = @"Data Source=..\..\..\..\db.db;";
            _conn = new SqliteConnection(connectionString);
            _conn.Open();
        }

        public long GetUserAge()
        {
            const string @select = "SELECT age FROM Users WHERE Id = 1";
            using (var comm = new SqliteCommand(select, _conn))
            {
                return (long) comm.ExecuteScalar();
            }
        }

        public void UpdateAge(long age)
        {
            var update = $"UPDATE Users SET age = {age} WHERE Id = 1";
            using (var comm = new SqliteCommand(update, _conn))
            {
                comm.ExecuteReader();
            }
        }

        public void Dispose()
        {
            _conn?.Dispose();
        }
    }
}

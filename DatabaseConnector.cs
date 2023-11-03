using Microsoft.Data.Sqlite;

public class DatabaseConnector
{
    private SqliteConnection connection;
    private string connectionString;

    public DatabaseConnector(string databasePath)
    {
        connectionString = $"Data Source={databasePath}";
        connection = new SqliteConnection(connectionString);
    }

    public SqliteConnection GetConnection()
    {
        return connection;
    }

    public void OpenConnection()
    {
        if (connection.State == System.Data.ConnectionState.Closed)
        {
            connection.Open();
        }
    }

    public void CloseConnection()
    {
        if (connection.State == System.Data.ConnectionState.Open)
        {
            connection.Close();
        }
    }
}


using MySql.Data.MySqlClient;

public class DatabaseConnector
{
    private MySqlConnection connection;
    private string connectionString;

    public DatabaseConnector()
    {
        connectionString = "server=localhost;user=root;password=;database=govapp;";
        connection = new MySqlConnection(connectionString);
    }

    public MySqlConnection GetConnection()
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

using Microsoft.Data.Sqlite;

namespace DrawersScratch.Utils;

public class SqlUtils
{
    // https://stackoverflow.com/a/67144114/16458003
    public static bool TableAlreadyExists(SqliteConnection openConnection, string tableName)
    {
        var sql = "SELECT name FROM sqlite_master WHERE type='table' AND name='" + tableName + "';";
        if (openConnection.State == System.Data.ConnectionState.Open)
        {
            SqliteCommand command = new SqliteCommand(sql, openConnection);
            SqliteDataReader reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Close();
                return true;
            }
            reader.Close();
            return false;
        }
        else
        {
            throw new System.ArgumentException("Data.ConnectionState must be open");
        }
    }
}

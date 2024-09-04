using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DrawersScratch.Utils;
using Microsoft.Data.Sqlite;

namespace DrawersScratch.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private const string DbPath = "hello.db";
    private const string DbTableName = "user";
    private long _dbLastRowId;

    [ObservableProperty]
    private string _headingColumn0 = "Add To Database";

    [ObservableProperty]
    private string _headingColumn1 = "Read From Database";

    [ObservableProperty]
    private string _headingColumn2 = "Log";

    [ObservableProperty]
    private string _textColumn1 = "Database Contents:";

    [ObservableProperty]
    private string _textColumn2 = "Log:";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(HandleAddClickCommand))]
    private string? _username;

    public MainViewModel()
    {
        SQLitePCL.Batteries.Init();
        using var connection = new SqliteConnection("Data Source=" + DbPath);
        connection.Open();
        if (SqlUtils.TableAlreadyExists(connection, DbTableName))
            return;

        var command = connection.CreateCommand();
        command.CommandText =
            @"
            CREATE TABLE user (
                id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL
            );

            INSERT INTO user
            VALUES (1, 'Brice'),
                   (2, 'Alexander'),
                   (3, 'Nate');
            ";
        command.ExecuteNonQuery();
        connection.Close();
    }

    private bool CanAddToDb() => !string.IsNullOrEmpty(Username);

    [RelayCommand(CanExecute = nameof(CanAddToDb))]
    private async Task HandleAddClick()
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        AddToDb();
    }

    [RelayCommand]
    private void HandleReadClick()
    {
        ReadFromDb();
    }

    private void AppendToTextColumn2(string text)
    {
        TextColumn2 += $"\n{text}";
    }

    private void AppendToTextColumn1(string text)
    {
        TextColumn1 += $"\n{text}";
    }

    private void AddToDb()
    {
        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
            @"
            INSERT INTO user (name)
            VALUES ($name)
            ";
        command.Parameters.AddWithValue("$name", Username);
        command.ExecuteNonQuery();
        AppendToTextColumn2($"user with name {Username} inserted into {DbTableName}.");

        command.CommandText =
            @"
            SELECT last_insert_rowid()
            ";
        _dbLastRowId = (long)(command.ExecuteScalar() ?? 0);
        AppendToTextColumn2($"`_dbLastRowId` set to {_dbLastRowId}.");
        connection.Close();
    }

    private void ReadFromDb()
    {
        using var connection = new SqliteConnection($"Data Source={DbPath}");
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText =
            @"
            SELECT name
            FROM user
            WHERE id = $id
            ";
        command.Parameters.AddWithValue("$id", _dbLastRowId);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var name = reader.GetString(0);

            AppendToTextColumn1($"Hi, {name}!");
            AppendToTextColumn2($"Read {name} from db.");
        }
        connection.Close();
    }
}

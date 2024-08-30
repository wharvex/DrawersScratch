using System;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;

namespace DrawersScratch.ViewModels
{
    public partial class MainViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string _headingColumn1 = "Add To Database";

        [ObservableProperty]
        private string _headingColumn2 = "Read From Database";

        [ObservableProperty]
        private string _headingColumn3 = "Log";

        [ObservableProperty]
        private string _textColumn2 = "Database Contents:";

        [ObservableProperty]
        private string _textColumn3 = "Log:";

        [RelayCommand]
        private void HandleAddClick()
        {
            TestSqlite();
        }

        private void AppendToTextColumn2(string text)
        {
            TextColumn2 += "\n" + text;
        }

        private void TestSqlite()
        {
            SQLitePCL.Batteries.Init();
            long id;
            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

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

                var name = "Tim";

                command.CommandText =
                    @"
                    INSERT INTO user (name)
                    VALUES ($name)
                    ";
                command.Parameters.AddWithValue("$name", name);
                command.ExecuteNonQuery();

                command.CommandText =
                    @"
                    SELECT last_insert_rowid()
                    ";
                id = (long)(command.ExecuteScalar() ?? 0);

                AppendToTextColumn2($"Your new user ID is {id}.");
            }

            using (var connection = new SqliteConnection("Data Source=hello.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    @"
                    SELECT name
                    FROM user
                    WHERE id = $id
                    ";
                command.Parameters.AddWithValue("$id", id);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.GetString(0);

                        AppendToTextColumn2($"Hello, {name}!");
                    }
                }
            }
        }
    }
}

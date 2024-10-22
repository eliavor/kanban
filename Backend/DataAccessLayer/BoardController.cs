using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

internal class BoardController
{
    private readonly string _connectionString;

    internal BoardController()
    {
        // Get the base directory of the tests project
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Adjust the relative path to move from the tests bin directory to the actual database location
        string path = Path.GetFullPath(Path.Combine(
         Directory.GetCurrentDirectory(), "kanban.db"));


        // Set the connection string
        _connectionString = $"Data Source={path};Version=3;";
    }

    // Method to select boards based on given criteria
    internal List<BoardDAO> Select(Dictionary<string, object> criteria)
    {
        string query;
        List<BoardDAO> boards = new List<BoardDAO>();
        if (criteria.Count == 0)
        {
            query = "SELECT * FROM Board";
        }
        else
        {
            query = "SELECT * FROM Board WHERE ";
        }
        List<string> conditions = new List<string>();
        List<SQLiteParameter> parameters = new List<SQLiteParameter>();
        BoardDAO board = null;

        foreach (var criterion in criteria)
        {
            conditions.Add($"{criterion.Key} = @{criterion.Key}");
            parameters.Add(new SQLiteParameter($"@{criterion.Key}", criterion.Value));
        }

        query += string.Join(" AND ", conditions);

        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddRange(parameters.ToArray());
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string name = reader.GetString(1);
                        int boardID = (int) reader.GetInt64(0);
                        string owner = reader.GetString(5);
                        List<string> userForBoard = GetUsersForBoard((int)reader.GetInt64(0));
                        List<ColumnDAO> columns = GetColumnsForBoard((int)reader.GetInt64(0),
                            (int)reader.GetInt64(2), (int)reader.GetInt64(3),
                            (int)reader.GetInt64(4));
                        

                        board = new BoardDAO(
                            (int)boardID, // BoardId
                            name, // BoardName
                            owner, // Owner
                            userForBoard, // Users for the board (List<String>)
                            columns, // Columns for the board (List<ColumnDAO>)
                            (int)reader.GetInt64(0) // LastTaskID
                        );
                        boards.Add(board);
                    }

                }
            }
        }
        

        return boards;
    }

    // Method to delete a board by ID and return the deleted board
    internal BoardDAO Delete(int boardId)
    {
        DeleteBoardTasks(boardId);
        BoardDAO board = GetBoardById(boardId);
        if (board == null)
        {
            throw new Exception("Board not found.");
        }

        string query = "DELETE FROM Board WHERE BoardId = @BoardId";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@BoardId", boardId);
                command.ExecuteNonQuery();
            }
        }
        string query2 = "DELETE FROM UserBoard WHERE BoardId = @BoardId";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query2, connection))
            {
                command.Parameters.AddWithValue("@BoardId", boardId);
                command.ExecuteNonQuery();
            }
        }

        return board;
    }

    // Method to insert a new board and return the inserted board
    internal BoardDAO Insert(BoardDAO board)
    {
        string query = @"INSERT INTO Board (BoardId, BoardName, BackLogLimit, InProgressLimit, DoneLimit, OwnerEmail, LastTaskId) 
                         VALUES (@BoardId, @BoardName, @BackLogLimit, @InProgressLimit, @DoneLimit, @OwnerEmail, @LastTaskId)";

        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using var command = new SQLiteCommand(query, connection);
            command.Parameters.AddWithValue("@BoardId", board.BoardId);
            command.Parameters.AddWithValue("@BoardName", board.BoardName);
            command.Parameters.AddWithValue("@BackLogLimit", board.Columns[0].Limit);
            command.Parameters.AddWithValue("@InProgressLimit", board.Columns[1].Limit);
            command.Parameters.AddWithValue("@DoneLimit", board.Columns[2].Limit);
            command.Parameters.AddWithValue("@OwnerEmail", board.Owner);
            command.Parameters.AddWithValue("@LastTaskId", board.lastTaskID);
            command.ExecuteNonQuery();
        }

        string query2 = @"INSERT INTO UserBoard (BoardId, UserEmail) Values (@BoardId, @UserEmail)";
        foreach (string user in board.Users)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using var command = new SQLiteCommand(query2, connection);
                command.Parameters.AddWithValue("@BoardId", board.BoardId);
                command.Parameters.AddWithValue("@UserEmail", user);
                command.ExecuteNonQuery ();
            }
        }
        return board;
    }

    // Method to update a board based on given criteria and return the updated board
    internal BoardDAO Update(int boardId, Dictionary<string, object> updates)
    {
        string query = "UPDATE Board SET ";
        List<string> setClauses = new List<string>();
        List<SQLiteParameter> parameters = new List<SQLiteParameter>();

        // Check if "Users" update is present
        if (updates.ContainsKey("UserEmail"))
        {
            // Retrieve list of users from updates dictionary
            List<string> users = (List<string>)updates["UserEmail"];

            // Delete existing entries for the boardId in Board-User table
            string deleteQuery = "DELETE FROM UserBoard WHERE BoardId = @BoardId";
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using var deleteCommand = new SQLiteCommand(deleteQuery, connection);
                deleteCommand.Parameters.AddWithValue("@BoardId", boardId);
                deleteCommand.ExecuteNonQuery();
            }

            // Insert new entries for each user
            string insertQuery = @"INSERT INTO UserBoard (BoardId, UserEmail) VALUES (@BoardId, @UserEmail)";
            foreach (string user in users)
            {
                using (var connection = new SQLiteConnection(_connectionString))
                {
                    connection.Open();
                    using var insertCommand = new SQLiteCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@BoardId", boardId);
                    insertCommand.Parameters.AddWithValue("@UserEmail", user);
                    insertCommand.ExecuteNonQuery();
                }
            }

            // Remove "Users" from updates since it's processed separately
            updates.Remove("UserEmail");
        }

        // Construct set clauses and parameters for Board table update
        bool flag=true;
        foreach (var update in updates)
        {
            setClauses.Add($"{update.Key} = @{update.Key}");
            parameters.Add(new SQLiteParameter($"@{update.Key}", update.Value));
            flag = false;
        }
        if (!flag)
        {
            // Complete the main update query for Board table
            query += string.Join(", ", setClauses);
            query += " WHERE BoardId = @BoardId";
            parameters.Add(new SQLiteParameter("@BoardId", boardId));

            // Execute the update query for Board table
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    command.ExecuteNonQuery();
                }
            }
        }
        // Return the updated board object
        BoardDAO a= GetBoardById(boardId);
        return a;
    }

    // Helper method to get users for a specific board
    private List<string> GetUsersForBoard(int boardId)
    {
        List<string> users = new List<string>();
        string query = "SELECT UserEmail FROM  UserBoard WHERE BoardId = @BoardId";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@BoardId", boardId);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        users.Add(reader.GetString(0));
                    }
                }
            }
        }
        return users;
    }

    // Helper method to get columns for a specific board
    private List<ColumnDAO> GetColumnsForBoard(int boardId, int backlogLimit, int inProgressLimit, int doneLimit)
    {
        List<ColumnDAO> columns = new List<ColumnDAO>();

        columns.Add(new ColumnDAO(0, boardId, GetTasksForColumn(boardId, 0), backlogLimit));
        columns.Add(new ColumnDAO(1, boardId, GetTasksForColumn(boardId, 1), inProgressLimit));
        columns.Add(new ColumnDAO(2, boardId, GetTasksForColumn(boardId, 2), doneLimit));

        return columns;
    }

    // Helper method to get tasks for a specific column
    private List<TaskDAO> GetTasksForColumn(int boardId, int columnOrdinal)
    {
        List<TaskDAO> tasks = new List<TaskDAO>();
        string query = "SELECT * FROM Task WHERE BoardId = @BoardId AND ColumnOrdinal = @ColumnOrdinal";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@BoardId", boardId);
                command.Parameters.AddWithValue("@ColumnOrdinal", columnOrdinal);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var task = new TaskDAO(
                            reader.GetInt16(0),
                            reader.GetString(1),
                            reader.GetString(5),
                            DateTime.Parse(reader.GetString(2)),
                            DateTime.Parse(reader.GetString(3)),
                            reader.GetString(4),
                            reader.GetInt16(6),
                            reader.GetInt16(7),
                            true
                        );
                        tasks.Add(task);
                    }
                }
            }
        }
        return tasks;
    }

    // Helper method to get a board by ID
    private BoardDAO GetBoardById(int boardId)
    {
        return Select(new Dictionary<string, object> { { "BoardId", boardId } })[0];
    }
    private void DeleteBoardTasks(int boardId)
    {
        string query = "DELETE FROM Task WHERE BoardId = @BoardId";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                command.Parameters.AddWithValue("@BoardId", boardId);
                command.ExecuteNonQuery();
            }
        }
    }
    internal void DeleteAll()
    {
        string query = "DELETE FROM Board";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                try { command.ExecuteNonQuery(); }
                catch (Exception ex) { }
            }
        }
        string query2 = "DELETE FROM UserBoard";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query2, connection))
            {
                try { command.ExecuteNonQuery(); }
                catch (Exception ex) { }
            }
        }
    }
}

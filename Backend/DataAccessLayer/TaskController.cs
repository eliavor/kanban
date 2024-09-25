using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

internal class TaskController
{
	private readonly string _connectionString;

	internal TaskController()
	{
        // Get the base directory of the tests project
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        // Adjust the relative path to move from the tests bin directory to the actual database location
        string path = Path.GetFullPath(Path.Combine(
         Directory.GetCurrentDirectory(), "kanban.db"));

        // Set the connection string
        _connectionString = $"Data Source={path};Version=3;";
    }

	// Method to select tasks based on given criteria
	internal List<TaskDAO> Select(Dictionary<string, object> criteria)
	{
		string query;
		List<TaskDAO> tasks = new List<TaskDAO>();
        if (criteria.Count == 0)
        {
            query = "SELECT * FROM Task";
        }
        else
        {
            query = "SELECT * FROM Task WHERE ";
        }
        List<string> conditions = new List<string>();
		List<SQLiteParameter> parameters = new List<SQLiteParameter>();

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
						var task = new TaskDAO(
							reader.GetInt32(0),
							reader.GetString(1),
							reader.GetString(5),
							DateTime.Parse(reader.GetString(2)),
							DateTime.Parse(reader.GetString(3)),
							reader.GetString(4),
							reader.GetInt32(6),
							reader.GetInt32(7)
						);
						tasks.Add(task);
					}
				}
			}
		}

		return tasks;
	}

	// Method to delete a task by ID and return the deleted task
	internal TaskDAO Delete(int taskId,int boardId)
	{
		TaskDAO task = GetTaskById(taskId, boardId);
		if (task == null)
		{
			throw new Exception("Task not found.");
		}

		string query = "DELETE FROM Task WHERE TaskId = @TaskId";
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();
			using (var command = new SQLiteCommand(query, connection))
			{
				command.Parameters.AddWithValue("@TaskId", taskId);
				command.ExecuteNonQuery();
			}
		}

		return task;
	}

	// Method to insert a new task and return the inserted task
	internal TaskDAO Insert(TaskDAO task)
	{
		string query = @"INSERT INTO Task (Title, CreationDate, DueDate, Description, AssigneeEmail, BoardId, ColumnOrdinal, TaskId) 
                         VALUES (@Title, @CreationDate, @DueDate, @Description, @AssigneeEmail, @BoardId, @ColumnOrdinal, @TaskId)";

		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();
			using (var command = new SQLiteCommand(query, connection))
			{
				command.Parameters.AddWithValue("@Title", task.Title);
				command.Parameters.AddWithValue("@CreationDate", task.CreationDate.ToString("yyyy-MM-dd HH:mm:ss"));
				command.Parameters.AddWithValue("@DueDate", task.DueDate.ToString("yyyy-MM-dd HH:mm:ss"));
				command.Parameters.AddWithValue("@Description", task.Description);
				command.Parameters.AddWithValue("@AssigneeEmail", task.Assignee);
				command.Parameters.AddWithValue("@BoardId", task.BoardId);
				command.Parameters.AddWithValue("@ColumnOrdinal", task.ColumnOrdinal);
                command.Parameters.AddWithValue("@TaskId", task.TaskId);
                command.ExecuteNonQuery();

			}
		}

		return task;
	}

	// Method to update a task based on given criteria and return the updated task
	internal TaskDAO Update(int taskId,int boardId,Dictionary<string, object> updates)
	{
		string query = "UPDATE Task SET ";
		List<string> setClauses = new List<string>();
		List<SQLiteParameter> parameters = new List<SQLiteParameter>();

		foreach (var update in updates)
		{
			setClauses.Add($"{update.Key} = @{update.Key}");
			parameters.Add(new SQLiteParameter($"@{update.Key}", update.Value));
		}

		query += string.Join(", ", setClauses);
		query += " WHERE TaskId = @TaskId";
		parameters.Add(new SQLiteParameter("@TaskId", taskId));
		query+= " AND BoardId = @BoardId";
		parameters.Add(new SQLiteParameter("@BoardId", boardId));

		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();
			using (var command = new SQLiteCommand(query, connection))
			{
				command.Parameters.AddRange(parameters.ToArray());
				command.ExecuteNonQuery();
			}
		}

		return GetTaskById(taskId,boardId);
	}

	// Helper method to get UserDAO by email
	private UserDAO GetUserDAO(string email)
	{
		UserDAO user = null;
		string query = "SELECT * FROM User WHERE Email = @Email";
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();
			using (var command = new SQLiteCommand(query, connection))
			{
				command.Parameters.AddWithValue("@Email", email);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						user = new UserDAO(reader.GetString(0), reader.GetString(1));
					}
				}
			}
		}
		return user;
	}

	// Helper method to get a task by ID
	private TaskDAO GetTaskById(int taskId,int boardId)
	{
		TaskDAO task = null;
		string query = "SELECT * FROM Task WHERE TaskId = @TaskId AND BoardId=@BoardId";
		using (var connection = new SQLiteConnection(_connectionString))
		{
			connection.Open();
			using (var command = new SQLiteCommand(query, connection))
			{
				command.Parameters.AddWithValue("@TaskId", taskId);
				command.Parameters.AddWithValue("@BoardId", boardId);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						task = new TaskDAO(
							reader.GetInt32(0),
							reader.GetString(1),
							reader.GetString(5),
							DateTime.Parse(reader.GetString(2)),
							DateTime.Parse(reader.GetString(3)),
							reader.GetString(4),
							reader.GetInt32(6),
							reader.GetInt32(7)
						);
					}
				}
			}
		}
		return task;
	}
    internal void DeleteAll()
    {
        string query = "DELETE FROM Task";
        using (var connection = new SQLiteConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SQLiteCommand(query, connection))
            {
                try { command.ExecuteNonQuery(); }
                catch (Exception ex) { }
            }
        }
    }
}

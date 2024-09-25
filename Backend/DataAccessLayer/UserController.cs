using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using MySql.Data;
using MySql.Data.MySqlClient;

internal class UserController
{
    private readonly string _connectionString;

    internal UserController()
    {
        // Get the base directory of the tests project
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;



        // Set the connection string
        _connectionString = "Server=kanban.c3wqw4y2yjiu.eu-north-1.rds.amazonaws.com;Database=Kanban;User ID=admin;Password=Oreliav2005;"; 
    }

    // Method to select users based on given criteria
    internal List<UserDAO> Select(Dictionary<string, object> criteria)
    {
        bool flag = false;
        List<UserDAO> users2 = new List<UserDAO>();
        if (criteria.ContainsKey("BoardId"))
        {
            int boardId = (int)criteria["BoardId"];
            string query1 = "SELECT * FROM UserBoard WHERE BoardId=@BoardId";
            using (MySqlConnection connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                using (MySqlCommand command = new MySqlCommand(query1, connection))
                {
                    command.Parameters.AddWithValue("@BoardId", boardId);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var user = new UserDAO(
                                reader.GetString(0),
                                reader.GetString(1)
                            );
                            users2.Add(user);
                        }
                    }
                }
            }
            criteria.Remove("BoardId");
            flag= true;
        }
        string query;
        List<UserDAO> users = new List<UserDAO>();
        if (criteria.Count == 0)
        {
            query = "SELECT * FROM User";
        }
        else
        {
            query = "SELECT * FROM User WHERE ";
        }
            List<string> conditions = new List<string>();
        List<MySqlParameter> parameters = new List<MySqlParameter>();

        foreach (var criterion in criteria)
        {
            conditions.Add($"{criterion.Key} = @{criterion.Key}");
            parameters.Add(new MySqlParameter($"@{criterion.Key}", criterion.Value));
        }

        query += string.Join(" AND ", conditions);

        using (var connection1 = new MySqlConnection(_connectionString))
        {
            connection1.Open();
            using (var command = new MySqlCommand(query, connection1))
            {
                command.Parameters.AddRange(parameters.ToArray());
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var user = new UserDAO(
                            reader.GetString(0),
                            reader.GetString(1)
                        );
                        users.Add(user);
                    }
                }
            }
        }
        List<UserDAO> users3 = users;
        if (flag)
        {
            users3=users.Intersect(users2).ToList();
        }
        return users3;
    }

    // Method to delete a user by email and return the deleted user
    internal UserDAO Delete(string email)
    {
        UserDAO user = GetUserByEmail(email);
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        string query = "DELETE FROM User WHERE Email = @Email";
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                command.ExecuteNonQuery();
            }
        }
        string query2 = "DELETE FROM UserBoard WHERE Email = @Email";
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", email);
                command.ExecuteNonQuery();
            }
        }

        return user;
    }

    // Method to insert a new user and return the inserted user
    internal UserDAO Insert(UserDAO user)
    {
        Console.WriteLine("the connection " + _connectionString);

        string query = @"INSERT INTO User (Email, Password) 
                         VALUES (@Email, @Password)";

        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@Password", user.Password);
                command.ExecuteNonQuery();
            }
        }
        return user;
    }

    // Method to update a user's password and return the updated user
    internal UserDAO Update(string email, string newPassword)
    {
        string query = "UPDATE User SET Password = @Password WHERE Email = @Email";
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Password", newPassword);
                command.Parameters.AddWithValue("@Email", email);
                command.ExecuteNonQuery();
            }
        }

        return GetUserByEmail(email);
    }
    private UserDAO GetUserByEmail(string email)
    {
        UserDAO user = null;
        string query = "SELECT * FROM User WHERE Email = @Email";
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(query, connection))
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
    internal void DeleteAll()
    {
        string query = "DELETE FROM User";
        using (var connection = new MySqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new MySqlCommand(query, connection))
            {
                try { command.ExecuteNonQuery(); }
                catch (Exception ex) { }
            }
        }
    }

    internal List<UserDAO> LoadData()
    {
        return Select(new Dictionary<string, object>());
    }
}
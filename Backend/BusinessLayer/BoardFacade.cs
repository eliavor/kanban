using KanBan_2024.ServiceLayer;
using log4net;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class BoardFacade
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(BoardFacade));
        private Dictionary<string, List<int>> boardsPerUser;
        private Dictionary<int, BoardBL> boardsByID;
        private Authenticator a;
        private int lastBoardID;
        private BoardController BC;
        private TaskController TC;
        private static string secret = "my super secret key that is very secret and nobody knows it!";

        public BoardFacade(Authenticator a)
        {
            this.a = a;
            boardsPerUser = new Dictionary<string, List<int>>();
            boardsByID = new Dictionary<int, BoardBL>();
            log.Info("BoardFacade initialized.");
            lastBoardID = -2;
            BC = new BoardController();
            TC = new TaskController();
        }

        internal int generateUniqueBoardID()
        {
            lastBoardID++;
            return lastBoardID;
        }

        /// <summary>
        /// Creates a new board for the specified user.
        /// </summary>
        /// <param name="BoardName">The name of the board to create.</param>
        /// <param name="email">The email address of the user creating the board.</param>
        /// <returns>Returns the created BoardBL object.</returns>
        /// <exception cref="Exception">Thrown when the user does not exist, board name is null or empty, or board name already exists.</exception>
        internal BoardBL CreateBoard(string BoardName, string email,string JWT)
        {
            log.Info($"Attempting to create board with name: {BoardName} for user: {email}");

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"CreateBoard failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"CreateBoard failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Check if the board name is valid
            if (BoardName == null || BoardName.Length == 0)
            {
                log.Error($"CreateBoard failed for user: {email}. Reason: Illegal board name.");
                throw new Exception("Illegal board name.");
            }

            // Check if the board name already exists for the user
            if (boardsPerUser.ContainsKey(email) && FindBoard(BoardName,boardsPerUser[email]) != null)
            {
                log.Error($"CreateBoard failed for user: {email}. Reason: Board name already exists.");
                throw new Exception("Board name already exists.");
            }
            List<String> boardUsers = new List<String>();
            boardUsers.Add(email);
            BoardBL b = new BoardBL(BoardName, boardUsers, generateUniqueBoardID());

            // Add the board to the user's list of boards
            if (boardsPerUser.ContainsKey(email))
            {
                boardsPerUser[email].Add(b.boardID);
            }
            else
            {
                List<int> list = new List<int>();
                list.Add(b.boardID);
                boardsPerUser.Add(email, list);
            }
            boardsByID.Add(b.boardID, b);

            log.Info($"Board created successfully with name: {BoardName} for user: {email}");

            // Return the created board object
            return b;
        }

        public BoardBL DeleteBoard(string BoardName, string email, string JWT)
        {
            log.Info($"Attempting to delete board with name: {BoardName} for user: {email}");

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"DeleteBoard failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"DeleteBoard failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Find the board to delete
            BoardBL b = FindBoards(BoardName, email);

            // If board is not found, throw an exception
            if (b == null)
            {
                log.Error($"DeleteBoard failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }
            if (b.ownerEmail != null && !b.ownerEmail.Equals(email))
            {
                log.Error($"DeleteBoard failed for user: {email}. Reason: User is not the board owner.");
                throw new Exception("User is not the board owner.");
            }
            foreach (String user in b.users)
            {
                boardsPerUser[user].Remove(b.boardID);
            }
            boardsByID.Remove(b.boardID);
            b.boardID = -2;
            log.Info($"Board deleted successfully with name: {BoardName} for user: {email}");

            // Return the deleted board object
            return b;
        }

        public BoardBL FindBoards(string BoardName, string email)
        {
            log.Info($"Attempting to find board with name: {BoardName} for user: {email}");

            // Check if the user has any boards
            if (!boardsPerUser.ContainsKey(email))
            {
                log.Error($"FindBoards failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Search for the board by name
            BoardBL b = null;
            foreach (int boardID in boardsPerUser[email])
            {
                BoardBL temp = boardsByID[boardID];
                if (temp.BoardName.Equals(BoardName)) return temp;
            }
            return b;
        }

        public List<taskSL> InProgressTasks(string email, string JWT)
        {
            log.Info($"Attempting to get in-progress tasks for user: {email}");

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"InProgressTasks failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"InProgressTasks failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Prepare a list to store in-progress tasks
            List<taskSL> tasks = new List<taskSL>();
            if (!boardsPerUser.ContainsKey(email)) return tasks;
            // Iterate through each board of the user
            foreach (int boardID in boardsPerUser[email])
            {
                BoardBL b = boardsByID[boardID];
                // Retrieve tasks from the second column (assuming index 1 represents in-progress column)
                List<taskSL> ColumnTasks = new List<taskSL>();
                foreach (TaskBL taskBL in b.Columns[1].TaskList)
                {
                    ColumnTasks.Add(new taskSL(taskBL)); // Convert TaskBL to taskSL and add to list
                }

                // Add tasks of the column to the main list
                tasks.AddRange(ColumnTasks);
            }

            // Return the list of in-progress tasks
            return tasks;
        }

        public bool DoesUserExist(string email)
        {
            if(email == "") return true;
            return a.signedIn.ContainsKey(email);
        }
        /// <summary>
        /// Finds a board by its name within a list of BoardBL objects.
        /// </summary>
        /// <param name="BoardName">The name of the board to find.</param>
        /// <param name="boardBLs">The list of BoardBL objects to search within.</param>
        /// <returns>Returns the BoardBL object if found; otherwise, returns null.</returns>
        internal BoardBL FindBoard(string BoardName, List<int> boardBLs)
        {
            BoardBL boardBL = null;
            foreach (int id in boardBLs)
            {
                BoardBL b = boardsByID[id];
                if (b.BoardName.Equals(BoardName))
                {
                    boardBL = b;
                    break;
                }
            }
            return boardBL;
        }

        public TaskBL FindTask(int id, List<TaskBL> taskBLs)
        {
            TaskBL taskbl = null;
            foreach (TaskBL b in taskBLs)
            {
                if (b.ID == id)
                {
                    taskbl = b;
                    break;
                }
            }
            return taskbl;
        }

        public int GetColumnLimit(string email, string BoardName, int columnOrdinal, string JWT)
        {
            log.Info($"Attempting to get column limit for column: {columnOrdinal} on board: {BoardName} for user: {email}");

            // Check if the column ordinal is within valid range (0 to 2)
            if (columnOrdinal > 2 || columnOrdinal < 0)
            {
                log.Error($"GetColumnLimit failed for user: {email}. Reason: Invalid column ordinal.");
                throw new Exception("Invalid column ordinal.");
            }

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"GetColumnLimit failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"GetColumnLimit failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"GetColumnLimit failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Retrieve the column limit
            int lim = b.Columns[columnOrdinal].Limit;
            return lim;
        }

        public string GetColumnName(string email, string BoardName, int columnOrdinal, string JWT)
        {
            log.Info($"Attempting to get column name for column: {columnOrdinal} on board: {BoardName} for user: {email}");

            // Check if the column ordinal is within valid range (0 to 2)
            if (columnOrdinal > 2 || columnOrdinal < 0)
            {
                log.Error($"GetColumnName failed for user: {email}. Reason: Invalid column ordinal.");
                throw new Exception("Invalid column ordinal.");
            }

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"GetColumnName failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"GetColumnName failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"GetColumnName failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Retrieve the column name
            string name = b.Columns[columnOrdinal].Name;
            return name;
        }

        public TaskBL UpdateTaskDueDate(string email, int taskid, DateTime dueDate, string BoardName, int columnOrdinal, string JWT)
        {
            log.Info($"Attempting to update task due date for task: {taskid} on board: {BoardName} for user: {email}");

            // Check if the due date is in the future
            if (dueDate.CompareTo(DateTime.Now) <= 0)
            {
                log.Error($"UpdateTaskDueDate failed for user: {email}. Reason: Due date must be in the future.");
                throw new Exception("Due date must be in the future.");
            }

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"UpdateTaskDueDate failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"UpdateTaskDueDate failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"UpdateTaskDueDate failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Check if the column ordinal is valid (0 or 1)
            if (columnOrdinal > 2 || columnOrdinal < 0)
            {
                log.Error($"UpdateTaskDueDate failed for user: {email}. Reason: Invalid column ordinal.");
                throw new Exception("Invalid column ordinal.");
            }
            if (columnOrdinal == 2)
            {
                log.Error($"UpdateTaskDueDate failed for user: {email}. Reason: Can't update finished tasks.");
                throw new Exception("Can't update finished tasks.");
            }

            // Retrieve the task list from the specified column
            List<TaskBL> TL = b.Columns[columnOrdinal].TaskList;

            // Find the task in the list
            TaskBL ans = FindTask(taskid, TL);
            if (ans == null) throw new Exception("Due date must be in the future.");
            if (!ans.Assignee.Equals(email))
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Not the correct Assignee.");
                throw new Exception("Not the correct Assignee.");
            }
            ans.DueDate = dueDate;

            log.Info($"Task due date updated successfully for task: {taskid} on board: {BoardName} for user: {email}");
            return ans;
        }

        public TaskBL UpdateTaskDescription(string email, int taskid, string description, string BoardName, int columnOrdinal, string JWT)
        {
            log.Info($"Attempting to update task description for task: {taskid} on board: {BoardName} for user: {email}");

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"UpdateTaskDescription failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"UpdateTaskDescription failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"UpdateTaskDescription failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Check if description length is within limits
            if (description.Length > 300)
            {
                log.Error($"UpdateTaskDescription failed for user: {email}. Reason: Description too long.");
                throw new Exception("Description too long.");
            }

            // Check if column ordinal is valid (0 to 1)
            if (columnOrdinal > 2 || columnOrdinal < 0)
            {
                log.Error($"UpdateTaskDescription failed for user: {email}. Reason: Invalid column ordinal.");
                throw new Exception("Invalid column ordinal.");
            }
            if (columnOrdinal ==2)
            {
                log.Error($"UpdateTaskDescription failed for user: {email}. Reason: Can't update finished tasks.");
                throw new Exception("Can't update finished tasks.");
            }
            // Check if the task exists in the specified column
            List<TaskBL> TL = b.Columns[columnOrdinal].TaskList;
            TaskBL ans = FindTask(taskid, TL);
            if (ans == null)
            {
                log.Error($"UpdateTaskDescription failed for user: {email}. Reason: Task does not exist.");
                throw new Exception("Task does not exist");
            }
            if (!ans.Assignee.Equals(email))
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Not the correct Assignee.");
                throw new Exception("Not the correct Assignee.");
            }
            ans.Description = description;

            log.Info($"Task description updated successfully for task: {taskid} on board: {BoardName} for user: {email}");
            return ans;
        }

        public TaskBL UpdateTaskTitle(string email, int taskid, string title, string BoardName, int columnOrdinal, string JWT)
        {
            log.Info($"Attempting to update task title for task: {taskid} on board: {BoardName} for user: {email}");

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Check if column ordinal is valid (0 to 1)
            if (columnOrdinal > 2 || columnOrdinal < 0)
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Invalid column ordinal.");
                throw new Exception("Invalid column ordinal.");
            }
            if (columnOrdinal ==2)
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Can't update finished tasks.");
                throw new Exception("Can't update finished tasks.");
            }

            // Check if title is empty or too long
            if (string.IsNullOrEmpty(title) || title.Length > 50)
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Invalid title.");
                throw new Exception("Invalid title.");
            }

            // Check if column is finished (ordinal 2)
            if (columnOrdinal == 2)
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Can't update finished tasks.");
                throw new Exception("Can't update finished tasks.");
            }

            // Get the task list from the specified column
            List<TaskBL> TL = b.Columns[columnOrdinal].TaskList;

            // Find the task in the column
            TaskBL ans = FindTask(taskid, TL);
            if (ans == null)
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Task does not exist.");
                throw new Exception("Task does not exist");
            }
            if (title.Length == 0)
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Invalid title.");
                throw new Exception("Invalid title.");
            }
            if(!ans.Assignee.Equals(email))
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Not the correct Assignee.");
                throw new Exception("Not the correct Assignee.");
            }
            ans.Title = title;

            log.Info($"Task title updated successfully for task: {taskid} on board: {BoardName} for user: {email}");
            return ans;
        }

        public List<TaskBL> GetColumn(string email, string BoardName, int columnOrdinal, string JWT)
        {
            log.Info($"Attempting to get column: {columnOrdinal} on board: {BoardName} for user: {email}");

            // Check if column ordinal is valid (0 to 2)
            if (columnOrdinal > 2 || columnOrdinal < 0)
            {
                log.Error($"GetColumn failed for user: {email}. Reason: Invalid column ordinal.");
                throw new Exception("Invalid column ordinal.");
            }

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"GetColumn failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"GetColumn failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"GetColumn failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Get the task list from the specified column
            List<TaskBL> taskBLs = b.Columns[columnOrdinal].TaskList;

            return taskBLs;
        }

        public BoardBL AdvanceTask(string email, int taskid, string BoardName, int columnOrdinal, string JWT)
        {
            log.Info($"Attempting to advance task: {taskid} on board: {BoardName} for user: {email}");

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"AdvanceTask failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"AdvanceTask failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Check if column ordinal is valid (0 to 1)
            if (columnOrdinal >= 2 || columnOrdinal < 0)
            {
                log.Error($"AdvanceTask failed for user: {email}. Reason: Invalid column ordinal.");
                throw new Exception("Invalid column ordinal.");
            }

            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"AdvanceTask failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            // Get the task list from the current column
            List<TaskBL> TL = b.Columns[columnOrdinal].TaskList;

            // Find the task in the current column
            TaskBL ans = FindTask(taskid, TL);
            if (ans == null)
            {
                log.Error($"AdvanceTask failed for user: {email}. Reason: Task not found.");
                throw new Exception("Task not found.");
            }
            if (!ans.Assignee.Equals(email))
            {
                log.Error($"UpdateTaskTitle failed for user: {email}. Reason: Not the correct Assignee.");
                throw new Exception("Not the task's assignee");
            }
            ans.ColumnPosition=ans.ColumnPosition+1;
            TL.Remove(ans);
            b.Columns[columnOrdinal].TaskList = TL;
            b.Columns[columnOrdinal + 1].TaskList.Add(ans);

            log.Info($"Task advanced successfully for task: {taskid} on board: {BoardName} for user: {email}");
            return b;
        }

        public TaskBL AddTask(string email, string BoardName, string title, string description, DateTime DueDate, string JWT)
        {
            log.Info($"Attempting to add task to board: {BoardName} for user: {email}");

            // Check if the user exists
            if (!DoesUserExist(email))
            {
                log.Error($"AddTask failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"AddTask failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Check if any of the input parameters are null or empty
            if (BoardName == null || title == null || title.Length > 50 || (description != null && description.Length > 300) || BoardName == "" || title == "" || DueDate==null)
            {
                log.Error($"AddTask failed for user: {email}. Reason: Illegal input.");
                throw new Exception("Illegal input.");
            }

            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"AddTask failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }

            if (!boardsByID[b.boardID].users.Contains(email))
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: User not a board member.");
                throw new Exception("User not a board member.");
            }

            // Check if the due date is in the future
            if (DueDate.CompareTo(DateTime.Now) <= 0)
            {
                log.Error($"AddTask failed for user: {email}. Reason: Due date must be in the future.");
                throw new Exception("Due date must be in the future.");
            }

            // Check if the first column has reached its task limit
            if (b.Columns[0].TaskList.Count >= b.Columns[0].Limit && b.Columns[0].Limit != -1)
            {
                log.Error($"AddTask failed for user: {email}. Reason: Too many tasks in column.");
                throw new Exception("Too many tasks in column.");
            }
            if (description == null) description = "";
            TaskBL t = new TaskBL(b.GetNextUniqueTaskID(), DateTime.Now, DueDate, title, description,b.boardID ,0);

            // Add the task to the first column
            b.Columns[0].addTask(t, b.boardID);
                 
            log.Info($"Task added successfully to board: {BoardName} for user: {email}");
            return t;
        }
        /// <summary>
        /// Sets a limit on the number of tasks allowed in a specified column on a board for a user.
        /// </summary>
        /// <param name="email">The email address of the user.</param>
        /// <param name="BoardName">The name of the board containing the column.</param>
        /// <param name="ColumnOrdinal">The ordinal number of the column to limit (0-based index).</param>
        /// <param name="Limit">The limit to set for the number of tasks in the column (must be greater than or equal to 0).</param>
        /// <returns>Returns the ColumnBL object representing the column with the updated limit.</returns>
        /// <exception cref="Exception">Thrown when user does not exist, board does not exist, column ordinal is invalid, limit is negative, or there are already more tasks in the column than the new limit.</exception>
        internal ColumnBL LimitColumn(string email, string BoardName, int ColumnOrdinal, int Limit, string JWT)
        {
            log.Info($"Attempting to limit column: {ColumnOrdinal} on board: {BoardName} for user: {email}");
            if (!DoesUserExist(email))
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            // Check if any of the input parameters are null or empty
            if (BoardName == null ||  BoardName == "" || ColumnOrdinal <0 || ColumnOrdinal > 2 || Limit < 0)
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: Illegal input.");
                throw new Exception("Illegal input.");
            }

            // Find the board
            BoardBL b = FindBoard(BoardName, boardsPerUser[email]);
            if (b == null)
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: No such board.");
                throw new Exception("No such board.");
            }
            if (b.Columns[ColumnOrdinal].Limit == Limit)
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: Limit already set.");
                throw new Exception("Limit already set.");
            }
            if (!b.users.Contains(email))
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: User not a board member.");
                throw new Exception("User not a board member.");
            }
            if (b.Columns[ColumnOrdinal].TaskList.Count > Limit)
            {
                log.Error($"LimitColumn failed for user: {email}. Reason: Too many Tasks in Column.");
                throw new Exception("Too many Tasks in Column.");
            }

            b.Columns[ColumnOrdinal].Limit = Limit;
            log.Info($"Column limit set successfully for column: {ColumnOrdinal} on board: {BoardName} for user: {email}");
            return b.Columns[ColumnOrdinal];
        }

        internal void DeleteData()
        {
            BC.DeleteAll();
            TC.DeleteAll();
            boardsByID = new Dictionary<int, BoardBL>();
            boardsPerUser = new Dictionary<string, List<int>>();
        }

        internal void LoadData()
        {
            List<BoardBL> boardsList = BoardDaoToBoardBL(BC.Select(new Dictionary<string, object>()));
            if (boardsList == null)
            {
                lastBoardID = -2;
                return;
                
            }
            int lastID = -2;
            foreach (BoardBL toAns in boardsList)
            {
                if(toAns.boardID > lastID)
                {
                    lastID = toAns.boardID;
                }
                boardsByID.Add(toAns.boardID, toAns);
                foreach (String userEmail in toAns.users)
                {
                    if (!boardsPerUser.ContainsKey(userEmail))
                    {
                        boardsPerUser.Add(userEmail, new List<int>());
                        
                    }
                    boardsPerUser[userEmail].Add(toAns.boardID);

                }
            }
            lastBoardID = lastID;
        }

        internal List<BoardBL> BoardDaoToBoardBL(List<BoardDAO> boardsDAO)
        {
            if (boardsDAO.Count == 1 && boardsDAO.First() == null) return null;
            List<BoardBL> ans = new List<BoardBL> ();
            foreach(BoardDAO b in boardsDAO)
            {
                
                BoardBL toAns = new BoardBL(b, b.Users);
                ans.Add(toAns);
            }
            return ans;
        }

        internal BoardBL JoinBoard(String Email, int boardID, string JWT)
        {
            if (!DoesUserExist(Email))
            {
                log.Error($"CreateBoard failed for user: {Email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"JoinBoard failed for user: {Email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            if (!boardsByID.ContainsKey(boardID))
            {
                log.Error($"JoinBoard failed for user: {Email}. Reason: No such board.");
                throw new Exception("No such board.");
            }
            BoardBL target = boardsByID[boardID];
            if (boardsPerUser.ContainsKey(Email))
            {
                foreach (int tempID in boardsPerUser[Email])
                {
                    BoardBL b = boardsByID[tempID];
                    if (b.BoardName.Equals(target.BoardName)){
                        log.Error($"JoinBoard failed for user: {Email}. Reason: Already joined to the board.");
                        throw new Exception("Already joined to the board.");
                    }
                }
            }

            target.users.Add(Email);
            boardsPerUser.Add(Email, new List<int>());
            boardsPerUser[Email].Add(target.boardID);
            target.users = target.users;
            return target;
        }
        internal BoardBL LeaveBoard(String Email, int boardID, string JWT)
        {
            if (!DoesUserExist(Email))
            {
                log.Error($"CreateBoard failed for user: {Email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"LeaveBoard failed for user: {Email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            if (!boardsByID.ContainsKey(boardID))
            {
                log.Error($"JoinBoard failed for user: {Email}. Reason: No such board.");
                throw new Exception("No such board.");
            }
            BoardBL toLeave = boardsByID[boardID];
            if (!toLeave.users.Contains(Email))
            {
                log.Error($"JoinBoard failed for user: {Email}. Reason: Not a member of the board.");
                throw new Exception("Not a member of the board.");
            }
            if (toLeave.ownerEmail.Equals(Email))
            {
                log.Error($"JoinBoard failed for user: {Email}. Reason: Owner can't leave.");
                throw new Exception("Owner can't leave.");
            }
            toLeave.users.Remove(Email);
            toLeave.users = toLeave.users;
            List<int> list = boardsPerUser[Email];
            foreach(int tempID in list)
            {
                BoardBL b = boardsByID[tempID];
                if (b.BoardName.Equals(toLeave.BoardName)){
                    list.Remove(b.boardID);
                    return b;
                }
            }
            return null;
        }

        internal List<int> GetUserBoards(String Email, string JWT) 
        {
            if (!DoesUserExist(Email))
            {
                log.Error($"getUserBoards failed for user: {Email}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"GetUserBoards failed for user: {Email}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            if (boardsPerUser.ContainsKey(Email))
            {
                List<int> ids = boardsPerUser[Email];
                return ids;
            }
            return new List<int>();
        }

        internal BoardBL TransferOwnership(String currentOwnerEmail, String NewOwnerEmail, String boardName, string JWT)
        {
            if (!DoesUserExist(currentOwnerEmail))
            {
                log.Error($"TransferOwnership failed for user: {currentOwnerEmail}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"TransferOwnership failed for user: {currentOwnerEmail}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            if (!DoesUserExist(NewOwnerEmail))
            {
                log.Error($"TransferOwnership failed for user: {NewOwnerEmail}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            BoardBL b = FindBoard(boardName, boardsPerUser[currentOwnerEmail]);
            if(b == null)
            {
                log.Error($"TransferOwnership failed for user: {currentOwnerEmail}. Reason: No such board.");
                throw new Exception("No such board.");
            }
            if (!b.ownerEmail.Equals(currentOwnerEmail))
            {
                log.Error($"TransferOwnership failed for user: {currentOwnerEmail}. Reason: User is not the board owner.");
                throw new Exception("User is not the board owner.");
            }
            if (!b.users.Contains(NewOwnerEmail))
            {
                log.Error($"TransferOwnership failed for user: {NewOwnerEmail}. Reason: User is not a board member.");
                throw new Exception("User is not a board member.");
            }
            if(b.ownerEmail == NewOwnerEmail)
            {
                log.Error($"TransferOwnership failed for user: {NewOwnerEmail}. Reason: Already the owner.");
                throw new Exception("Already the owner.");
            }
            b.ownerEmail = NewOwnerEmail;
            return b;
        }

        internal TaskBL AssignTask(String oldAssignee, String boardName, int columnOrdinal, int taskID, String NewAssignee, string JWT)
        {
            if (oldAssignee != "" &&!DoesUserExist(oldAssignee))
            {
                log.Error($"AssignTask failed for user: {oldAssignee}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"AssignTask failed for user: {oldAssignee}. Reason: Invalid token.");
                throw new Exception("Invalid token.");

            }
            if (!DoesUserExist(NewAssignee)) {
                log.Error($"AssignTask failed for user: {NewAssignee}. Reason: No such user.");
                throw new Exception("No such user.");
            }
            BoardBL b;
            if (boardsPerUser.ContainsKey(oldAssignee)) 
                {
                b = FindBoard(boardName, boardsPerUser[oldAssignee]);
            }
            else
            {
                b = FindBoard(boardName, boardsPerUser[NewAssignee]);
            }
            if(b == null)
            {
                log.Error($"AssignTask failed for user: {oldAssignee}. Reason: No such board.");
                throw new Exception("No such board.");
            }
            if (!b.users.Contains(NewAssignee))
            {
                log.Error($"AssignTask failed for user: {NewAssignee}. Reason: No such board.");
                throw new Exception("No such board.");
            }
            if(columnOrdinal < 0 || columnOrdinal > 1)
            {
                log.Error($"AssignTask failed for user: {oldAssignee}. Reason: Invalid column ordinal");
                throw new Exception("Invalid column ordinal.");
            }
            TaskBL t = FindTask(taskID, b.Columns[columnOrdinal].TaskList);
            if (t  == null)
            {
                log.Error($"AssignTask failed for user: {oldAssignee}. Reason: No such task.");
                throw new Exception("No such task.");
            }
            //if (NewAssignee.Equals(oldAssignee))
            //{
            //    log.Error($"AssignTask failed for user: {oldAssignee}. Reason: Already the assignee.");
            //    throw new Exception("Already the assignee.");
            //}
            if(t.Assignee != "" && !t.Assignee.Equals(oldAssignee))
            {
                log.Error($"AssignTask failed for user: {oldAssignee}. Reason: User is not the asignee.");
                throw new Exception("Not the task's assignee");
            }
            t.Assignee = NewAssignee;
            return t;
            
        }
        internal String GetBoardName(int boardID)
        {
            if(!boardsByID.ContainsKey(boardID))
            {
                log.Error($"GetBoardName failed. Reason: No such board.");
                throw new Exception("No such board.");
            }
            return boardsByID[boardID].BoardName;
        }
        public bool ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            try
            {
                // Validate the token and its claims
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = "kanbanserver.com",
                    ValidateAudience = true,
                    ValidAudience = "kanbanapi.com",
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);


                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}

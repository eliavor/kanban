using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;


namespace KanBan_2024.ServiceLayer
{
    public class BoardService
    {
        internal BoardFacade BF { get; }
        private Authenticator a;

        internal BoardService(Authenticator a)
        {
            this.a = a;
            BF = new BoardFacade(a);
        }
        /// <summary>
        /// This method creates a board for the given user.
        /// </summary>
        /// <param name="email">Email of the user, must be logged in</param>
        /// <param name="name">The name of the new board</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string CreateBoard(string email, string name)
        {
            try
            {
                BoardBL b = BF.CreateBoard(name, email);
                Response response = new Response(new BoardSL(b.BoardName,b.boardID, b.ownerEmail),null);
                string toReturn = JsonSerializer.Serialize(response);
                return toReturn;
            }
            catch (Exception ex)
            {
                Response r = new Response(null, ex.Message);
                return JsonSerializer.Serialize(r);
            }
        }
        /// <summary>
        /// This method deletes a board.
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in and an owner of the board.</param>
        /// <param name="name">The name of the board</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string DeleteBoard(string email, string name)
        {
            try
            {
                BoardBL b = BF.DeleteBoard(name, email);
                Response response = new Response(null,null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null,ex.Message));
            }
        }
        
        /// <summary>
        /// This method limits the number of tasks in a specific column.
        /// </summary>
        /// <param name="email">The email address of the user, must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="limit">The new limit value. A value of -1 indicates no limit.</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string LimitColumn(string email, string boardName, int columnOrdinal, int limit)
        {
            try
            {
                BF.LimitColumn(email, boardName, columnOrdinal, limit);
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }

        /// <summary>
        /// This method adds a new task.
        /// </summary>
        /// <param name="email">Email of the user. The user must be logged in.</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="title">Title of the new task</param>
        /// <param name="description">Description of the new task</param>
        /// <param name="dueDate">The due date if the new task</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string AddTask(string email, string boardName, string title, string description, DateTime dueDate)
        {
            try
            {
                TaskBL t = BF.AddTask(email, boardName, title, description, dueDate);
                Response response = new Response(new taskSL(t.ID, t.CreationTime, t.DueDate, t.Title, t.Description, t.Assignee), null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method advances a task to the next column
        /// </summary>
        /// <param name="email">Email of user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string AdvanceTask(string email, string boardName, int columnOrdinal, int taskId)
        {
            try
            {
                BoardBL b = BF.AdvanceTask(email, taskId, boardName, columnOrdinal);
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
           
        /// <summary>
        /// This method gets the limit of a specific column.
        /// </summary>
        /// <param name="email">The email address of the user, must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>A response with the column's limit, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string GetColumnLimit(string email, string boardName, int columnOrdinal)
        {
            try
            {
                int lim = BF.GetColumnLimit(email,boardName,columnOrdinal);
                Response response = new Response(lim, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method gets the name of a specific column
        /// </summary>
        /// <param name="email">The email address of the user, must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>A response with the column's name, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string GetColumnName(string email, string boardName, int columnOrdinal)
        {
            try
            {
                string name = BF.GetColumnName(email, boardName, columnOrdinal);
                Response response = new Response(name, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method returns a column given it's name
        /// </summary>
        /// <param name="email">Email of the user, must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <returns>A response with a list of the column's tasks, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string GetColumn(string email, string boardName, int columnOrdinal)
        {
            try
            {
                List<TaskBL> col = BF.GetColumn(email, boardName, columnOrdinal);
                List<taskSL> ans = new List<taskSL>();
                foreach (TaskBL taskBL in col)
                {
                    ans.Add(new taskSL(taskBL));
                }
                Response response = new Response(ans, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        ///<summary>This method loads all persisted data.
        ///<para>
        ///<b>IMPORTANT:</b> When starting the system via the GradingService - do not load the data automatically, only through this method. 
        ///In some cases we will call LoadData when the program starts and in other cases we will call DeleteData. Make sure you support both options.
        ///</para>
        /// </summary>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string LoadData()
        {
            try
            {
                BF.LoadData();
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }

        ///<summary>This method deletes all persisted data.
        ///<para>
        ///<b>IMPORTANT:</b> 
        ///In some cases we will call LoadData when the program starts and in other cases we will call DeleteData. Make sure you support both options.
        ///</para>
        /// </summary>
        ///<returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string DeleteData()
        {
            try
            {
                BF.DeleteData();
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }

        }
        /// <summary>
        /// This method returns a board's name
        /// </summary>
        /// <param name="boardId">The board's ID</param>
        /// <returns>A response with the board's name, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string GetBoardName(int boardId)
        {
            try
            {
                string name = BF.GetBoardName(boardId);
                Response response = new Response(name, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method transfers a board ownership.
        /// </summary>
        /// <param name="currentOwnerEmail">Email of the current owner. Must be logged in</param>
        /// <param name="newOwnerEmail">Email of the new owner</param>
        /// <param name="boardName">The name of the board</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string TransferOwnership(string currentOwnerEmail, string newOwnerEmail, string boardName)
        {
            try
            {
                BoardBL b = BF.TransferOwnership(currentOwnerEmail, newOwnerEmail, boardName);
                Response response = new Response(new BoardSL(b.BoardName , b.boardID, b.ownerEmail), null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method adds a user as member to an existing board.
        /// </summary>
        /// <param name="email">The email of the user that joins the board. Must be logged in</param>
        /// <param name="boardID">The board's ID</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string JoinBoard(string email, int boardID)
        {
            try
            {
                BF.JoinBoard(email, boardID);
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }

        /// <summary>
        /// This method removes a user from the members list of a board.
        /// </summary>
        /// <param name="email">The email of the user. Must be logged in</param>
        /// <param name="boardID">The board's ID</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string LeaveBoard(string email, int boardID)
        {
            try
            {
                BF.LeaveBoard(email, boardID);
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method returns a list of IDs of all user's boards.
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <returns>A response with a list of IDs of all user's boards, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string GetUserBoards(string email)
        {
            try
            {
                List<int> boards = BF.GetUserBoards(email);
                Response response = new Response(boards, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
    }
}

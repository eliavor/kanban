using IntroSE.Kanban.Backend.BusinessLayer;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace KanBan_2024.ServiceLayer
{
    public class TaskService
    {
        private BoardFacade BF;

        internal TaskService(BoardFacade b)
        {
            this.BF = b;
        }


        /// <summary>
        /// This method updates the due date of a task
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="dueDate">The new due date of the column</param>
        /// <returns>A response with the updated taskSL, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string UpdateTaskDueDate(string email, string boardName, int columnOrdinal, int taskId, DateTime dueDate)
        {
            try
            {
                TaskBL t = BF.UpdateTaskDueDate(email, taskId, dueDate, boardName, columnOrdinal);
                Response response = new Response(new taskSL(t.ID, t.CreationTime, t.DueDate, t.Title, t.Description, t.Assignee), null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null,ex.Message));
            }
        }

        /// <summary>
        /// This method updates the description of a task.
        /// </summary>
        /// <param name="email">Email of user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="description">New description for the task</param>
        /// <returns>A response with the updated taskSL, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string UpdateTaskDescription(string email, string boardName, int columnOrdinal, int taskId, string description)
        {
            try
            {
                TaskBL t = BF.UpdateTaskDescription(email, taskId, description, boardName, columnOrdinal);
                Response response = new Response(new taskSL(t.ID, t.CreationTime, t.DueDate, t.Title, t.Description, t.Assignee),null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }

        /// <summary>
        /// This method updates task title.
        /// </summary>
        /// <param name="email">Email of user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column ID. The first column is identified by 0, the ID increases by 1 for each column</param>
        /// <param name="taskId">The task to be updated identified task ID</param>
        /// <param name="title">New title for the task</param>
        /// <returns>A response with the updated taskSL, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string UpdateTaskTitle(string email, string boardName, int columnOrdinal, int taskId, string title)
        {
            try
            {
                TaskBL t = BF.UpdateTaskTitle(email, taskId, title, boardName, columnOrdinal);
                Response response = new Response(new taskSL(t.ID, t.CreationTime, t.DueDate, t.Title, t.Description, t.Assignee), null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method returns all in-progress tasks of a user.
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <returns>A response with a list of the in-progress tasks of the user, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string InProgressTasks(string email)
        {
            try
            {
                List<taskSL> l = BF.InProgressTasks(email);
                Response response = new Response(l, null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method assigns a task to a user
        /// </summary>
        /// <param name="email">Email of the user. Must be logged in</param>
        /// <param name="boardName">The name of the board</param>
        /// <param name="columnOrdinal">The column number. The first column is 0, the number increases by 1 for each column</param>
        /// <param name="taskID">The task to be updated identified a task ID</param>        
        /// <param name="emailAssignee">Email of the asignee user</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string AssignTask(string email, string boardName, int columnOrdinal, int taskID, string emailAssignee)
        {
            try
            {
                TaskBL task = BF.AssignTask(email, boardName, columnOrdinal, taskID, emailAssignee);
                Response response = new Response(new taskSL(task), null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }

    }
}

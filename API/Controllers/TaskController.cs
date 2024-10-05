using KanBan_2024.ServiceLayer;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ServiceFactory _serviceFactory;

        public TaskController(ServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        [HttpPut("update-due-date")]
        public IActionResult UpdateTaskDueDate(string email, string boardName, int columnOrdinal, int taskId, DateTime dueDate, string JWT)
        {
            var response = _serviceFactory.TS.UpdateTaskDueDate(email, boardName, columnOrdinal, taskId, dueDate, JWT);
            return Content(response);
        }

        [HttpPut("update-description")]
        public IActionResult UpdateTaskDescription(string email, string boardName, int columnOrdinal, int taskId, string description, string JWT)
        {
            var response = _serviceFactory.TS.UpdateTaskDescription(email, boardName, columnOrdinal, taskId, description, JWT);
            return Content(response);
        }

        [HttpPut("update-title")]
        public IActionResult UpdateTaskTitle(string email, string boardName, int columnOrdinal, int taskId, string title, string JWT)
        {
            var response = _serviceFactory.TS.UpdateTaskTitle(email, boardName, columnOrdinal, taskId, title, JWT);
            return Content(response);
        }

        [HttpGet("in-progress-tasks")]
        public IActionResult InProgressTasks(string email, string JWT)
        {
            var response = _serviceFactory.TS.InProgressTasks(email, JWT);
            return Content(response);
        }

        [HttpPut("assign-task")]
        public IActionResult AssignTask(string email, string boardName, int columnOrdinal, int taskID, string emailAssignee, string JWT)
        {

            var response = _serviceFactory.TS.AssignTask(email, boardName, columnOrdinal, taskID, emailAssignee, JWT);
            return Content(response);
        }
    }
}

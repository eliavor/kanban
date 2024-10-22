using KanBan_2024.ServiceLayer;
using Microsoft.AspNetCore.Mvc;
using System;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BoardController : Controller
    {
        private readonly ServiceFactory _serviceFactory;

        public BoardController(ServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        // POST: BoardController/create
        [HttpPost("create")]
        public ActionResult Create([FromBody] BoardCreate boardCreate)
        {
            var response = _serviceFactory.BS.CreateBoard(boardCreate.Email, boardCreate.BoardName, boardCreate.JWT);
            return Content(response, "application/json");
        }

        // DELETE: BoardController/delete
        [HttpDelete("delete")]
        public ActionResult Delete([FromBody] BoardDelete boardDelete)
        {
            var response = _serviceFactory.BS.DeleteBoard(boardDelete.Email, boardDelete.BoardName, boardDelete.JWT);
            return Content(response, "application/json");
        }

        // POST: BoardController/limitColumn
        [HttpPost("limitColumn")]
        public ActionResult LimitColumn([FromBody] LimitColumnRequest request)
        {
            var response = _serviceFactory.BS.LimitColumn(request.Email, request.BoardName, request.ColumnOrdinal, request.Limit, request.JWT);
            return Content(response, "application/json");
        }

        // POST: BoardController/addTask
        [HttpPost("addTask")]
        public ActionResult AddTask([FromBody] AddTaskRequest request)
        {
            var response = _serviceFactory.BS.AddTask(request.Email, request.BoardName, request.Title, request.Description, request.DueDate, request.JWT);
            return Content(response, "application/json");
        }

        // POST: BoardController/advanceTask
        [HttpPost("advanceTask")]
        public ActionResult AdvanceTask([FromBody] AdvanceTaskRequest request)
        {
            var response = _serviceFactory.BS.AdvanceTask(request.Email, request.BoardName, request.ColumnOrdinal, request.TaskId, request.JWT);
            return Content(response, "application/json");
        }

        // GET: BoardController/getColumnLimit
        [HttpGet("getColumnLimit")]
        public ActionResult GetColumnLimit([FromQuery] string email, [FromQuery] string boardName, [FromQuery] int columnOrdinal, [FromQuery] string JWT)
        {
            var response = _serviceFactory.BS.GetColumnLimit(email, boardName, columnOrdinal, JWT);
            return Content(response, "application/json");
        }

        // GET: BoardController/getColumnName
        [HttpGet("getColumnName")]
        public ActionResult GetColumnName([FromQuery] string email, [FromQuery] string boardName, [FromQuery] int columnOrdinal, [FromQuery] string JWT)
        {
            var response = _serviceFactory.BS.GetColumnName(email, boardName, columnOrdinal, JWT);
            return Content(response, "application/json");
        }

        // GET: BoardController/getColumn
        [HttpGet("getColumn")]
        public ActionResult GetColumn([FromQuery] string email, [FromQuery] string boardName, [FromQuery] int columnOrdinal, [FromQuery] string JWT)
        {
            var response = _serviceFactory.BS.GetColumn(email, boardName, columnOrdinal, JWT);
            return Content(response, "application/json");
        }


        // GET: BoardController/getUserBoards
        [HttpGet("getUserBoards")]
        public ActionResult GetUserBoards([FromQuery] string email, [FromQuery] string JWT)
        {
            var response = _serviceFactory.BS.GetUserBoards(email, JWT);
            return Content(response, "application/json");
        }


        // GET: BoardController/getBoardTasks
        [HttpGet("getBoardTasks")]
        public ActionResult GetBoardTasks([FromQuery] int boardID, [FromQuery] string JWT)
        {
            var response = _serviceFactory.BS.GetBoardTasks(boardID, JWT);
            return Content(response, "application/json");   
        }

        // GET: BoardController/getBoardName
        [HttpGet("getBoardName")]
        public ActionResult GetBoardName([FromQuery] int boardId)
        {
            var response = _serviceFactory.BS.GetBoardName(boardId);
            return Content(response, "application/json");
        }

        // POST: BoardController/transferOwnership
        [HttpPost("transferOwnership")]
        public ActionResult TransferOwnership([FromBody] TransferOwnershipRequest request)
        {
            var response = _serviceFactory.BS.TransferOwnership(request.CurrentOwnerEmail, request.NewOwnerEmail, request.BoardName, request.JWT);
            return Content(response, "application/json");
        }

        // POST: BoardController/joinBoard
        [HttpPost("joinBoard")]
        public ActionResult JoinBoard([FromBody] JoinBoardRequest request)
        {
            var response = _serviceFactory.BS.JoinBoard(request.Email, request.BoardID, request.JWT);
            return Content(response, "application/json");
        }

        // POST: BoardController/leaveBoard
        [HttpPost("leaveBoard")]
        public ActionResult LeaveBoard([FromBody] LeaveBoardRequest request)
        {
            var response = _serviceFactory.BS.LeaveBoard(request.Email, request.BoardID, request.JWT);
            return Content(response, "application/json");
        }
    }

    // DTO classes for requests
    public class BoardCreate
    {
        public string Email { get; set; }
        public string BoardName { get; set; }
        public string JWT {  get; set; }
    }

    public class BoardDelete
    {
        public string Email { get; set; }
        public string BoardName { get; set; }
        public string JWT { get; set; }
    }

    public class LimitColumnRequest
    {
        public string Email { get; set; }
        public string BoardName { get; set; }
        public int ColumnOrdinal { get; set; }
        public int Limit { get; set; }
        public string JWT { get; set; }
    }

    public class AddTaskRequest
    {
        public string Email { get; set; }
        public string BoardName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public string JWT { get; set; }
    }

    public class AdvanceTaskRequest
    {
        public string Email { get; set; }
        public string BoardName { get; set; }
        public int ColumnOrdinal { get; set; }
        public int TaskId { get; set; }
        public string JWT { get; set; }
    }

    public class TransferOwnershipRequest
    {
        public string CurrentOwnerEmail { get; set; }
        public string NewOwnerEmail { get; set; }
        public string BoardName { get; set; }
        public string JWT { get; set; }
    }

    public class JoinBoardRequest
    {
        public string Email { get; set; }
        public int BoardID { get; set; }
        public string JWT { get; set; }
    }

    public class LeaveBoardRequest
    {
        public string Email { get; set; }
        public int BoardID { get; set; }
        public string JWT { get; set; }
    }
}

using KanBan_2024.ServiceLayer;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BoardController : Controller
    {
        private ServiceFactory _serviceFactory;

        public BoardController(ServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        //POST: BoardController/create
        [HttpPost("create")]
        public ActionResult Create([FromBody] BoardCreate boardCreate)
        {
            var response = _serviceFactory.BS.CreateBoard(boardCreate.Email, boardCreate.boardName);
            return Content(response, "application/json");
        }
    }
    public class BoardCreate
    {
        public string Email { get; set; }
        public string boardName { get; set; }
    }

}

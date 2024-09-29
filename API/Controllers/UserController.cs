using KanBan_2024.ServiceLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private ServiceFactory _serviceFactory;

        public UserController(ServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
        }

        // POST: UserController/Register
        [HttpPost("register")]
        public ActionResult Register([FromBody] UserRegistrationDto registrationDto)
        {
            var response = _serviceFactory.US.Register(registrationDto.Email, registrationDto.Password);
            return Content(response, "application/json");
        }

        // POST: UserController/Login
        [HttpPost("login")]
        public ActionResult Login([FromBody] UserLoginDto loginDto)
        {
            var response = _serviceFactory.US.Login(loginDto.Email, loginDto.Password);
            return Content(response, "application/json");
        }

        // POST: UserController/Logout
        [HttpPost("logout")]
        public ActionResult Logout([FromBody] UserLogoutDto logoutDto)
        {
            var response = _serviceFactory.US.Logout(logoutDto.Email);
            return Content(response, "application/json");
        }
    }

    public class UserRegistrationDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class UserLogoutDto
    {
        public string Email { get; set; }
    }
}

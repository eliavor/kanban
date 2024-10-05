using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class UserBL
    {
        internal string password;
        internal string UserEmail { get; set; }
        internal bool LoggedIn {  get; set; }
        internal UserDAO userDAO {  get; set; }
        public string JWT { get; set; } = "";
        internal UserBL(string password, string userEmail)
        {
            userDAO = new UserDAO(userEmail, password);
            this.password = password;
            UserEmail = userEmail;
            LoggedIn = true;
            userDAO.persist();
        }
        internal UserBL(UserDAO user)
        {
            UserEmail = user.Email;
            password = user.Password;
            LoggedIn = true;
            userDAO = new UserDAO(UserEmail, password);
        }
    }
}

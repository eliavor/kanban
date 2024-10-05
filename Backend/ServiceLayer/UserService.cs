using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.BusinessLayer;
using log4net;
using System.Reflection;
using log4net.Config;
using log4net.Repository;
using System.IO;

namespace KanBan_2024.ServiceLayer
{
    public class UserService
    {
    
        private UserFacade UF;
        internal UserService(Authenticator a)
        {
            UF = new UserFacade(a);

        }
        /// <summary>
        /// This method registers a new user to the system.
        /// </summary>
        /// <param name="email">The user email address, used as the username for logging the system.</param>
        /// <param name="password">The user password.</param>
        /// <returns>A respone with userSL, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string Register(string email, string password)
        {
            try 
            {
                UserBL user = UF.Register(email, password);
                Response response = new Response(new UserSL(user.UserEmail), null);
                return JsonSerializer.Serialize(response);
            }
            catch (Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null,ex.Message));
            }
           
        }
        /// <summary>
        ///  This method logs in an existing user.
        /// </summary>
        /// <param name="email">The email address of the user to login</param>
        /// <param name="password">The password of the user to login</param>
        /// <returns>A response with the userSL, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string Login(string email, string password)
        {
            try
            {
                UserBL user = UF.Login(email, password);
                Response response = new Response(new UserSL(user.UserEmail, user.JWT), null);
                return JsonSerializer.Serialize(response);
            }
            catch(Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }
        }
        /// <summary>
        /// This method logs out a logged in user. 
        /// </summary>
        /// <param name="email">The email of the user to log out</param>
        /// <returns>An empty response, unless an error occurs (see <see cref="GradingService"/>)</returns>
        public string Logout(string email,string JWT)
        {
            try
            {
                UF.Logout(email, JWT);
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch(Exception ex)
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
                UF.LoadData();
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch(Exception ex)
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
                UF.DeleteData();
                Response response = new Response(null, null);
                return JsonSerializer.Serialize(response);
            }
            catch(Exception ex)
            {
                return JsonSerializer.Serialize(new Response(null, ex.Message));
            }

        }

    }

}

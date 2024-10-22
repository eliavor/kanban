using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class Authenticator
    {
        internal Dictionary<string, UserBL> signedIn {  get; set; }
        internal Authenticator() 
        {
            signedIn = new Dictionary<string, UserBL>();
        }
        /// <summary>
        /// Signs in a user with the specified email address and associates them with the provided user object.
        /// </summary>
        /// <param name="email">The email address of the user to sign in.</param>
        /// <param name="user">The UserBL object representing the user to sign in.</param>
        internal void SignIn(string email, UserBL user)
        {
            //signedIn.Add(email, user);
        }

        /// <summary>
        /// Signs out a user with the specified email address.
        /// </summary>
        /// <param name="email">The email address of the user to sign out.</param>
        internal void SignOut(string email)
        {
            signedIn.Remove(email);
        }

    }
}

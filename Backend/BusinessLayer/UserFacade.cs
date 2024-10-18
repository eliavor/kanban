using KanBan_2024.ServiceLayer;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;


namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class UserFacade
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UserFacade));
        private static Dictionary<string, UserBL> users;
        private UserController UC;
        private Authenticator a;
        private static string secret = "my super secret key that is very secret and nobody knows it!";

        internal UserFacade(Authenticator a)
        {
            this.a = a;
            users = new Dictionary<string, UserBL>();
            log.Info("UserFacade initialized.");
            UC = new UserController();
        }

        /// <summary>
        /// Registers a new user with the provided email and password.
        /// </summary>
        /// <param name="Email">The email address of the user to register.</param>
        /// <param name="Password">The password for the user account.</param>
        /// <returns>The UserBL object representing the registered user.</returns>
        /// <exception cref="Exception">Thrown when the user already exists, password is invalid, or email format is invalid.</exception>
        internal UserBL Register(string Email, string Password)
        {
            
            log.Info($"Attempting to register user with email: {Email}");
            Email = Email.ToLower();
            // Check if user already exists or password/email is invalid
            if (users.ContainsKey(Email) || !CheckPasswordValidity(Password) || !CheckEmailValidity(Email))
            {
                log.Error($"Registration failed for user with email: {Email}. Reason: Illegal password or user already exists.");
                throw new Exception("Illegal password or user already exists.");
            }


            // Create a new UserBL object and add it to users dictionary
            UserBL User = new UserBL(Password, Email);
            User.JWT = GenerateToken(Email);
            users.Add(Email, User);
            a.SignIn(Email, User);
            log.Info($"User registered successfully with email: {Email}");

            return User;
        }


        /// <summary>
        /// Checks the validity of a password based on the following criteria:
        /// - Length between 6 and 20 characters
        /// - Contains at least one uppercase letter, one lowercase letter, and one digit
        /// </summary>
        /// <param name="Password">The password to validate.</param>
        /// <returns>True if the password is valid according to the criteria, otherwise false.</returns>
        internal bool CheckPasswordValidity(string Password)
        {
            // Check for password length
            if (Password.Length < 6 || Password.Length > 20)
                return false;

            bool upper = false;
            bool lower = false;
            bool num = false;

            // Check for presence of uppercase letters, lowercase letters, and digits
            for (int i = 0; i < Password.Length; i++)
            {
                if (Password[i] >= 'a' && Password[i] <= 'z')
                    lower = true;
                if (Password[i] >= 'A' && Password[i] <= 'Z')
                    upper = true;
                if (Password[i] >= '0' && Password[i] <= '9')
                    num = true;
            }

            // Password must contain at least one uppercase letter, one lowercase letter, and one digit
            return num && upper && lower;
        }


        /// <summary>
        /// Checks the validity of an email address based on the following criteria:
        /// - Length between 5 and 254 characters
        /// - Format must match standard email address pattern (e.g., user@example.com)
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email address is valid according to the criteria, otherwise false.</returns>
        internal bool CheckEmailValidity(string email)
        {
            // Check if the email is null or empty
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            // Check for email length
            if (email.Length < 5 || email.Length > 254)
            {
                return false;
            }

            // Regular expression pattern for validating email format
            string emailPattern = @"^(?:[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*|""(?:[\x01-\x08\x0b\x0c\x0e-\x1f\x21\x23-\x5b\x5d-\x7f]|\\[\x01-\x09\x0b\x0c\x0e-\x7f])*
            "")@(?:(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?|\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[
            0-9][0-9]?|IPv6:[a-fA-F0-9]{1,4}:[a-fA-F0-9]{1,4}:[a-fA-F0-9]{1,4}:[a-fA-F0-9]{1,4}:[a-fA-F0-9]{1,4}:[a-fA-F0-9]{1,4}:[a-fA-F0-9]{1,4}:[a-fA-F0-9]{1,4})\])$"; 

            // Check if the email matches the pattern
            if (!Regex.IsMatch(email, emailPattern))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Logs in a user with the provided email and password.
        /// </summary>
        /// <param name="Email">The email address of the user to log in.</param>
        /// <param name="Password">The password for the user account.</param>
        /// <returns>The UserBL object representing the logged-in user.</returns>
        /// <exception cref="Exception">Thrown when no such user exists, password is incorrect, or the user is already logged in.</exception>
        internal UserBL Login(string Email, string Password)
        {
            log.Info($"Attempting to login user with email: {Email}");

            // Check if user exists and password is valid
            if (!DoesUserExist(Email) || !CheckPasswordValidity(Password))
            {
                log.Error($"Login failed for user with email: {Email}. Reason: No such user or password not compliant with the rules.");
                throw new Exception("No such user or password not compliant with the rules.");
            }

            // Check if the user is already logged in
            //if (CheckLoggedIn(Email))
            //{
            //    log.Error($"Login failed for user with email: {Email}. Reason: User already logged in.");
            //    throw new Exception("User already logged in.");
            //}

            // Validate the passwor d
            UserBL user = users[Email];
            if (!user.password.Equals(Password))
            {
                log.Error($"Login failed for user with email: {Email}. Reason: Wrong password.");
                throw new Exception("Wrong password.");
            }

            // Mark the user as logged in
            user.JWT = GenerateToken(Email);
            users[Email].LoggedIn = true;
            a.SignIn(Email, user);
            log.Info($"User logged in successfully with email: {Email}");

            return user;
        }

        /// <summary>
        /// Checks if a user with the specified email address is currently logged in.
        /// </summary>
        /// <param name="Email">The email address of the user to check.</param>
        /// <returns>True if the user is logged in, otherwise false.</returns>
        internal bool CheckLoggedIn(string Email)
        {
            return a.signedIn.ContainsKey(Email);
        }


        /// <summary>
        /// Logs out a user with the specified email address.
        /// </summary>
        /// <param name="Email">The email address of the user to log out.</param>
        /// <exception cref="Exception">Thrown when no such user exists or the user is not logged in.</exception>
        public void Logout(string Email, string JWT)
        {

            log.Info($"Attempting to logout user with email: {Email}");

            // Check if the user exists
            if (!DoesUserExist(Email))
            {
                log.Error($"Logout failed for user with email: {Email}. Reason: No such user.");
                throw new Exception("No such user.");
            }

            // Check if the user is logged in
            if (!a.signedIn.ContainsKey(Email))
            {
                log.Error($"Logout failed for user with email: {Email}. Reason: Not logged in.");
                throw new Exception("Not logged in.");
            }
            if (!ValidateToken(JWT))
            {
                log.Error($"Logout failed because token is invalid for: {Email}");
                throw new Exception("Invalid token.");
            }

            // Remove the user from the signedIn dictionary to log them out
            a.signedIn.Remove(Email);
            log.Info($"User logged out successfully with email: {Email}");
        }

        public bool DoesUserExist(string Email)
        {
            Email=Email.ToLower();
            return users.ContainsKey(Email);
        }

        public void LoadData()
        {
            List<UserBL> UsersList = UserDaoToUserBL(UC.Select(new Dictionary<string, object>()));
            foreach (UserBL toAns in UsersList)
            {
                users.Add(toAns.UserEmail, toAns);
            }
        }
        public List<UserBL> UserDaoToUserBL(List<UserDAO> list)
        {
            List<UserBL> ans = new List<UserBL>();
            foreach (UserDAO u in list)
            {
                UserBL toAns = new UserBL(u);
                ans.Add(toAns);
            }
            return ans;
        }
        public void DeleteData()
        {
            UC.DeleteAll();
            users = new Dictionary<string, UserBL>();
        }


        public string GenerateToken(string username)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "kanbanserver.com",
                audience: "kanbanapi.com",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30), 
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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

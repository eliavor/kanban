using IntroSE.Kanban.Backend.BusinessLayer;
using System.IO;
using System;

internal class UserDAO
{
    // Internal fields
    internal string Email { get; }
    internal string Password { get; }
    internal bool isPersist=false;
    private UserController Controller;

    // Constructor with insert function
    internal UserDAO(string email, string password)
    {
        Email = email;
        Password = password;
        Controller = new UserController();
    }
    internal void persist()
    {
        Controller.Insert(this);
    }

}

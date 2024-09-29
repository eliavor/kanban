using KanBan_2024.ServiceLayer;

namespace CLI
{
    class CLI
    {
        public static void Main(string[] args)
        {
            ServiceFactory SF = new ServiceFactory();

            while (true)
            {

                Console.WriteLine("1. Register \n2. Login \n3. Create Board \n4. Add task ");
                string input = Console.ReadLine();
                if (input == null || input.Length > 1)
                {
                    throw new Exception("Illegal Argument Exception");
                }
                switch (input)
                {
                    case "1":
                        Console.WriteLine("Enter Email: ");
                        string email = Console.ReadLine();
                        Console.WriteLine("Enter Password: ");
                        string password = Console.ReadLine();

                        Console.WriteLine(SF.US.Register(email, password));

                        break;

                    case "2":
                        Console.WriteLine("Enter Email: ");
                        string loginEmail = Console.ReadLine();
                        Console.WriteLine("Enter Password: ");
                        string loginPassword = Console.ReadLine();

                        Console.WriteLine(SF.US.Login(loginEmail, loginPassword));
                        break;
                }
            }
        }
    }
}
using IntroSE.Kanban.Backend.BusinessLayer;
using log4net.Config;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KanBan_2024.ServiceLayer
{
    public class ServiceFactory
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public UserService US;
        public BoardService BS;
        public TaskService TS;
        public ServiceFactory() {
            Authenticator a = new Authenticator();
            US = new UserService(a);
            BS = new BoardService(a);
            TS = new TaskService(BS.BF);
            startLog();
            LoadData();
        }
        public static void startLog()
        {
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.Config"));
        }
        public void LoadData()
        {
            US.LoadData();
            BS.LoadData();
        }
        public void DeleteData()
        {
            US.DeleteData();
            BS.DeleteData();
        }
    }
}

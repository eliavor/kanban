using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class ColumnBL
    {
        internal int ColumnOrdinal;

        internal List<TaskBL> TaskList { get; set; }
        
        internal int boardID { get; set; }
        
        internal ColumnDAO columnDAO { get; set; }
        internal int _Limit;
        internal int Limit
        {
            get => _Limit;
            set {
                _Limit = value;
                columnDAO.Limit = value;
            }
        }
        internal string Name;

        internal void addTask(TaskBL task, int boardID)
        {
            TaskList.Add(task);
            columnDAO.TaskList.Add(task.taskDAO);
        }
        
        public ColumnBL(int columnOrdinal, string name, int BoardID)
        {
            columnDAO = new ColumnDAO(columnOrdinal, BoardID, new List<TaskDAO>(), -1);
            ColumnOrdinal = columnOrdinal;
            TaskList = new List<TaskBL>();
            Limit = -1;
            Name = name;
            boardID = BoardID;

        }

        internal ColumnBL(ColumnDAO columnDAO)
        {
            this.columnDAO = columnDAO;
            ColumnOrdinal = columnDAO.ColumnOrdinal;
            TaskList = new List<TaskBL>();
            Limit = columnDAO.Limit;
            Name = columnDAO.Name;
            boardID = columnDAO.BoardId;
            foreach(TaskDAO taskDAO in columnDAO.TaskList)
            {
                TaskBL toAdd = new TaskBL(taskDAO);
                TaskList.Add(toAdd);
            }
            this.columnDAO = columnDAO;

        }
    }
}

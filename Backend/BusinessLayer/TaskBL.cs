using KanBan_2024.ServiceLayer;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class TaskBL
    {
        internal int ID { get; set; }
        internal DateTime CreationTime { get; set; }
        internal DateTime _DueDate;
        internal DateTime DueDate {
            get => _DueDate;
            set {
                _DueDate = value;
                taskDAO.DueDate = value;
            } 
        }
        internal string _Title;
        internal string Title
        {
            get => _Title;
            set
            {
                _Title = value;
                taskDAO.Title = value;
            }
        }
        internal string _Description;
        internal string Description
        {
            get => _Description;
            set
            {
                _Description = value;
                taskDAO.Description = value;
            }
        }
        internal int _ColumnPosition;
        internal int ColumnPosition
        {
            get => _ColumnPosition;
            set
            {
                _ColumnPosition = value;
                taskDAO.ColumnOrdinal = value;
            }
        }
        internal string _Assignee;
        internal String Assignee
        {
            get => _Assignee;
            set
            {
                _Assignee = value;
                taskDAO.Assignee = value;
            }
        }
        internal  TaskDAO taskDAO{  get; set; }
        internal TaskBL(int iD, DateTime creationTime, DateTime dueDate, string title, string description,int boardID, int columnPosition)
        {
            taskDAO = new TaskDAO(iD, title, "", creationTime, dueDate, description, boardID, columnPosition);
            ID = iD;
            CreationTime = creationTime;
            DueDate = dueDate;
            Title = title;
            Description = description;
            ColumnPosition = columnPosition;

            Assignee = "";
            taskDAO.persist();
        }
        internal TaskBL(TaskDAO taskDAo)
        {
            taskDAO = taskDAo;
            ID = taskDAO.TaskId;
            CreationTime = taskDAO.CreationDate;
            DueDate = taskDAO.DueDate;
            Title = taskDAO.Title;
            Description = taskDAO.Description;
            ColumnPosition = taskDAO.ColumnOrdinal;
            Assignee = taskDAO.Assignee;
            taskDAO.persist();
        }
    }
}

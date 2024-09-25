using IntroSE.Kanban.Backend.BusinessLayer;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanBan_2024.ServiceLayer
{
    public class taskSL
    {
        public int Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime DueDate { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Assignee { get; set; }

        internal taskSL(int id,DateTime creationDate, DateTime dueDate, string title, string description, string assignee) {
            this.Id = id;
            this.CreationDate = creationDate;
            this.DueDate = dueDate;
            this.Title = title;
            this.Description = description;
            this.Assignee = assignee;
       
            
        }
        internal taskSL(TaskBL taskBL)
        {
            this.Id = taskBL.ID;
            this.CreationDate = taskBL.CreationTime;
            this.DueDate = taskBL.DueDate;
            this.Title = taskBL.Title;
            this.Description = taskBL.Description;
            this.Assignee = taskBL.Assignee;
        }
        public taskSL() {}
    }
}

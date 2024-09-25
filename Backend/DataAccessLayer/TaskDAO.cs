using log4net;
using System;
using System.Collections.Generic;
using System.IO;

internal class TaskDAO
{
    // Internal fields
    internal int TaskId { get; set; }
    private string _Title;
    internal string Title
    {
        get { return _Title;}
        set
        {
            if(isPersist)
                Controller.Update(TaskId,BoardId, new Dictionary<string, object> { { "Title", value } });
            _Title = value;
        } }
    private string _Assignee;
    internal String Assignee
    {
        get { return _Assignee; }
        set
        {
            if(isPersist)
                Controller.Update(TaskId,BoardId, new Dictionary<string, object> { { "AssigneeEmail", value } });
            _Assignee = value;
        }
    }
    private DateTime _CreationDate;
    internal DateTime CreationDate
    {
        get { return _CreationDate; }
        set
        {
            if(isPersist)
                Controller.Update(TaskId,BoardId, new Dictionary<string, object> { { "CreationDate", value } });
            _CreationDate = value;
        }
    }
    private DateTime _DueDate;
    internal DateTime DueDate
    {
        get { return _DueDate; }
        set
        {
            if(isPersist)
                Controller.Update(TaskId,BoardId, new Dictionary<string, object> { { "DueDate", value } });
            _DueDate = value;
        }
    }
    private string _Description;
    internal string Description
    {
        get { return _Description; }
        set
        {
            if(isPersist)
                Controller.Update(TaskId,BoardId, new Dictionary<string, object> { { "Description", value } });
            _Description = value;
        }
    }
    internal int BoardId { get;}
    private int _ColumnOrdinal;
    internal int ColumnOrdinal
    {
        get { return _ColumnOrdinal; }
        set
        {
            if(isPersist)
                Controller.Update(TaskId, BoardId, new Dictionary<string, object> { { "ColumnOrdinal", value } });
            _ColumnOrdinal = value;
        }
    }
    internal bool isPersist = false;
    private TaskController Controller; 

    // Constructor with insert function
    internal TaskDAO(int taskId, string title, String assignee, DateTime creationDate, DateTime dueDate, string description, int boardId, int columnOrdinal)
    {
        TaskId = taskId;
        Title = title;
        Assignee = assignee;
        CreationDate = creationDate;
        DueDate = dueDate;
        Description = description;
        BoardId = boardId;
        ColumnOrdinal = columnOrdinal;
        Controller = new TaskController();
        
    }
    internal void persist()
    {
        if(!isPersist)
        {
            Controller.Insert(this);
            isPersist = true;
        }
    }

}

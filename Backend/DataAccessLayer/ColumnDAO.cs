using System;
using System.Collections.Generic;
using System.Security.Permissions;

internal class ColumnDAO
{
	// Internal fields
	internal int ColumnOrdinal { get; }
	internal int BoardId { get; }
	internal string Name { get; }
    private List<TaskDAO> _TaskList;
	internal List<TaskDAO> TaskList 
    {
        get => _TaskList;
        set
        {
            
            if (isPersist)
            {
                foreach(TaskDAO task in value)
                {
                    try
                    {
                        TaskController.Delete(task.TaskId, BoardId);
                    }
                    catch { }
                    TaskController.Insert(task);
                }
            }
            _TaskList = value;
        }
    }
    internal int _limit;
	internal int Limit
    {
        get =>_limit;
        set
        {

            _limit = value;
            if (isPersist) { 
                switch (ColumnOrdinal)
                {
                    case 0:
                        Controller.Update(BoardId, new Dictionary<string, object> { { "BackLogLimit", value } });
                    break;
                    case 1:
                        Controller.Update(BoardId, new Dictionary<string, object> { { "InProgressLimit", value } });
                    break;
                    case 2:
                        Controller.Update(BoardId, new Dictionary<string, object> { { "DoneLimit", value } });
                    break;

                }
            }

        }  }
	private BoardController Controller;
    private TaskController TaskController;
    internal bool isPersist=false;

	// Constructor with insert function
	internal ColumnDAO(int columnOrdinal, int boardId, List<TaskDAO> taskList, int limit)
	{
        Controller = new BoardController();
        TaskController = new TaskController();
        ColumnOrdinal = columnOrdinal;
		BoardId = boardId;
        ColumnOrdinal = columnOrdinal;
        switch (columnOrdinal)
        {
            case 0:
                Name = "Backlog";
                break;
            case 1:
                Name = "In Progress";
                break;
            case 2:
                Name = "Done";
                break;
            
        }
        TaskList = taskList;
		Limit = limit;
        Controller = new BoardController();

	}
    internal void persist()
    {
        if (!isPersist)
        {


            isPersist = true;
            switch (ColumnOrdinal)
            {
                case 0:
                    Controller.Update(BoardId, new Dictionary<string, object> { { "BackLogLimit", Limit } });
                    break;
                case 1:
                    Controller.Update(BoardId, new Dictionary<string, object> { { "InProgressLimit", Limit } });
                    break;
                case 2:
                    Controller.Update(BoardId, new Dictionary<string, object> { { "DoneLimit", Limit } });
                    break;

            }
            foreach (TaskDAO task in _TaskList)
            {
                try
                {
                    TaskController.Delete(task.TaskId, BoardId);
                }
                catch (Exception ex) { }
                TaskController.Insert(task);
            }
        }
    }

}


using System;
using System.Collections.Generic;

internal class BoardDAO
{
    // Internal fields
    private int _BoardId;
    internal int BoardId
    {
        get => _BoardId;
        set
        {
            
            if (value == -3)
            {
                Controller.Delete(_BoardId);

            }
            _BoardId = value;

        }
    }

    internal string BoardName { get; set; }
    private int _lastTaskID;
    internal int lastTaskID
    {
		get=> _lastTaskID;
        set
        {
			_lastTaskID= value;
            if(isPersist)
                Controller.Update(BoardId, new Dictionary<string, object> { { "LastTaskID", value } });
        
        }
    }
    private string _Owner;
    internal String Owner
    {
		get=> _Owner;
        set
        {
			_Owner= value;
            if(isPersist)
                Controller.Update(BoardId, new Dictionary<string, object> { { "OwnerEmail", value } });
        }
    }
	internal List<ColumnDAO> Columns { get; set; }
    bool isPersist = false;
    private List<string> _Users;
	internal List<String> Users { 
        get => _Users;
        set 
        {
            _Users= value;
            if (isPersist)
                Controller.Update(BoardId, new Dictionary<string, object> { {"UserEmail", value } });
        }
    }
    internal bool isPresist = false;
	private BoardController Controller { get; set; }

	// Constructor with insert function
	internal BoardDAO(int boardId, string boardName, String owner, List<String> users, List<ColumnDAO> columns,int lastTaskID)
	{
		BoardId = boardId;
		BoardName = boardName;
		Owner = owner;
		Users = users;
		Columns = columns;
		Controller = new BoardController();
		this.lastTaskID = lastTaskID;

	}
    internal void persist()
    {
        if (!isPersist)
        {
            Controller.Insert(this);
            Columns[0].persist();
            Columns[1].persist();
            Columns[2].persist();
            isPersist = true;
        }
    }
}
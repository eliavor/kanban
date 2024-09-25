using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace IntroSE.Kanban.Backend.BusinessLayer
{
    internal class BoardBL
    {
        internal int _lastTaskID;
        internal int lastTaskID
        {
            get => _lastTaskID;
            set
            {
                _lastTaskID = value;
                boardDAO.lastTaskID = value;
            }
        }
            

        internal string _ownerEmail;
        internal string ownerEmail
        {
            get => _ownerEmail;
            set
            {
                _ownerEmail = value;
                boardDAO.Owner = value;
            }
        }
        internal int _boardID;
        internal int boardID
        {
            get => _boardID;
            set
            {
                _boardID = value;
                if(value==-2)
                    boardDAO.BoardId = -3;
            }
        } 
        internal int GetNextUniqueTaskID()
        {
            lastTaskID = lastTaskID + 1;
            return lastTaskID;
        }
        internal ColumnBL[] Columns {  get; set; }
        internal string BoardName { get; set; }
        internal List<string> _users;
        internal List<String> users
        {
            get => _users;
            set
            {
                _users = value;
                boardDAO.Users = value;
            }
        }
        internal BoardDAO boardDAO { get; set; }
        internal BoardBL( string boardName1, List<String> users1, int boardID1)
        {
            
            Columns = new ColumnBL[3];
            string[] names = { "backlog", "in progress", "done" };
            for (int i = 0; i < 3; i++)
            {
                Columns[i] = new ColumnBL(i, names[i], boardID1);
            }
            List<ColumnDAO> _column = new List<ColumnDAO>();
            _column.Add(Columns[0].columnDAO);
            _column.Add(Columns[1].columnDAO);
            _column.Add(Columns[2].columnDAO);
            string owner2 = users1.First();
            boardDAO = new BoardDAO(boardID1, boardName1, owner2, users1, _column, -2);
            this.users = users1;
            this.boardID = boardID1;
            BoardName = boardName1;
            lastTaskID = -2;
            ownerEmail = owner2;
            boardDAO.persist();
        }
        internal BoardBL(BoardDAO boardDao)
        {
            this.boardDAO = boardDao;
            boardID = boardDao.BoardId;
            BoardName = boardDao.BoardName;
            ownerEmail = boardDao.Owner;
            users = new List<string>();
            List<string> users1 = boardDao.Users;
            foreach(String user in users1)
            {
                users.Add(user);
            }
            Columns = new ColumnBL[3];
            Columns[0] = new ColumnBL(boardDao.Columns[0]);
            Columns[1] = new ColumnBL(boardDao.Columns[1]);
            Columns[2] = new ColumnBL(boardDao.Columns[2]);

            boardDAO = boardDao;
            lastTaskID = boardDao.lastTaskID;
        }

        internal BoardBL(BoardDAO boardDao, List<string> users1)
        {
            this.boardDAO = boardDao;
            boardID = boardDao.BoardId;
            BoardName = boardDao.BoardName;
            ownerEmail = boardDao.Owner;
            users = new List<string>();
            foreach (String user in users1)
            {
                users.Add(user);
            }
            Columns = new ColumnBL[3];
            Columns[0] = new ColumnBL(boardDao.Columns[0]);
            Columns[1] = new ColumnBL(boardDao.Columns[1]);
            Columns[2] = new ColumnBL(boardDao.Columns[2]);

            boardDAO = boardDao;
            lastTaskID = boardDao.lastTaskID;
        }
    }
}

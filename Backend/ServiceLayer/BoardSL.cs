using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KanBan_2024.ServiceLayer
{
    public class BoardSL
    {
        public string boardName {  get; set; }
        public int boardId {  get; set; }
        public string boardOwner {  get; set; }

        public BoardSL(string boardName, int boardId, string boardOwner) {
            this.boardName = boardName;
            this.boardId = boardId;
            this.boardOwner = boardOwner;
        }
        
    }
}

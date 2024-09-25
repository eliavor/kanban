using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;
using IntroSE.Kanban.Backend.ServiceLayer;
using KanBan_2024.ServiceLayer;
using log4net;
using Microsoft.VisualBasic;

namespace BoardServiceTests
{
    public class main
    {
        private ServiceFactory serviceFactory;

        public void Setup()
        {
            serviceFactory = new ServiceFactory();
        }

        public void CreateBoardTest()
        {


            Setup();
            serviceFactory.DeleteData();
            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string boardName2 = "Test Board 2";
            string duplicateBoardName = "Duplicate Board";

            serviceFactory.US.Register(validEmail, "Valid1Password!");

            //Test 1: successful creation

            string result1 = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            BoardSL b = ConvertReturnValue<BoardSL>(r1?.ReturnValue);
            int boardID = b.boardId;
            BoardSL boardSL = ConvertReturnValue<BoardSL>(r1?.ReturnValue);
            Console.WriteLine(r1?.ErrorMessage == null && boardSL.boardName == boardName1 && boardSL.boardOwner == validEmail ? "CreateBoardTest 1 Passed" : "CreateBoardTest 1 Failed");
            //DataBase Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, "Valid1Password!");
            string DataResult1 = serviceFactory.BS.GetUserBoards(validEmail);
            Response rd1 = JsonSerializer.Deserialize<Response>(DataResult1);
            List<int> list = ConvertReturnValue<List<int>>(rd1?.ReturnValue);
            if (list != null)
            {
                foreach (int i in list)
                {
                    if (i == boardID)
                        Console.WriteLine("CreateBoardTest 2 Passed");
                    else
                        Console.WriteLine("CreateBoardTest 2 Failed");
                }
            }

            // Test 3: Invalid User
            string result2 = serviceFactory.BS.CreateBoard(invalidEmail, boardName2);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "CreateBoardTest 3 Passed" : "CreateBoardTest 3 Failed");


            // Test 4: Duplicate Board Name for the same user
            string result3 = serviceFactory.BS.CreateBoard(validEmail, duplicateBoardName);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            if (r3?.ErrorMessage == null)
            {
                // Try creating the same board again
                string result4 = serviceFactory.BS.CreateBoard(validEmail, duplicateBoardName);
                Response? r4 = JsonSerializer.Deserialize<Response>(result4);
                Console.WriteLine(r4?.ErrorMessage == "Board name already exists." ? "CreateBoardTest 4 Passed" : "CreateBoardTest 4 Failed1");
            }
            else
            {
                Console.WriteLine("CreateBoardTest 4 Failed");
            }

            //Delete this scope's data
            serviceFactory.DeleteData();

        }

        public void DeleteBoardTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string nonExistentBoardName = "Non-Existent Board";

            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response response = JsonSerializer.Deserialize<Response>(res);
            BoardSL b = ConvertReturnValue<BoardSL>(response?.ReturnValue);

            // Test 5: Successfully delete an existing board
            string result1 = serviceFactory.BS.DeleteBoard(validEmail, boardName1);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "DeleteBoardTest 5 Passed" : "DeleteBoardTest 5 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, "Valid1Password!");
            string DataResult1 = serviceFactory.BS.GetUserBoards(validEmail);
            Response rd2 = JsonSerializer.Deserialize<Response>(DataResult1);
            List<int> list = ConvertReturnValue<List<int>>(rd2?.ReturnValue);
            if (list != null)
            {
                if (list.Count != 0)
                    Console.WriteLine("DeleteBoardTest 6 Failed");
                else
                    Console.WriteLine("DeleteBoardTest 6 Passed");
            }

            // Test 7: Invalid User
            string result2 = serviceFactory.BS.DeleteBoard(invalidEmail, boardName1);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "DeleteBoardTest 7 Passed" : "DeleteBoardTest 7 Failed");

            // Test 8: Non-Existent Board for valid user
            string result3 = serviceFactory.BS.DeleteBoard(validEmail, nonExistentBoardName);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "DeleteBoardTest 8 Passed" : "DeleteBoardTest 8 Failed");

            // Test 9: Deletion not by the board owner
            string validEmail2 = "valid2@example.com";
            string res2 = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response response2 = JsonSerializer.Deserialize<Response>(res2);
            BoardSL b2 = ConvertReturnValue<BoardSL>(response2?.ReturnValue);
            serviceFactory.US.Register(validEmail2, "validPassword1!");
            serviceFactory.BS.JoinBoard(validEmail2, b2.boardId);
            string result4 = serviceFactory.BS.DeleteBoard(validEmail2, b.boardName);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "User is not the board owner." ? "DeleteBoardTest 9 Passed" : "DeleteBoardTest 9 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        public void LimitColumnTest()
        {
            Setup();
            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string existingBoardName = "Test Board 2";
            int columnOrdinal = 0;
            int newLimit = 5;

            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            serviceFactory.BS.CreateBoard(validEmail, boardName1);

            // Test 10: Successfully limit a column
            string result1 = serviceFactory.BS.LimitColumn(validEmail, boardName1, columnOrdinal, newLimit);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "LimitColumnTest 10 Passed" : "LimitColumnTest 10 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            string check = serviceFactory.US.Login(validEmail, "Valid1Password!");
            string res = serviceFactory.BS.GetColumnLimit(validEmail, boardName1, columnOrdinal);
            Response response = JsonSerializer.Deserialize<Response>(res);
            Console.WriteLine(ConvertReturnValue<int>(response?.ReturnValue) != newLimit ? "LimitColumnTest 11 Failed" : "LimitColumnTest 11 Passed");

            // Test 12: Invalid User 
            string result2 = serviceFactory.BS.LimitColumn(invalidEmail, boardName1, columnOrdinal, newLimit);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "LimitColumnTest 12 Passed" : "LimitColumnTest 12 Failed");

            // Test 13: Non-Existent Board
            string result3 = serviceFactory.BS.LimitColumn(validEmail, "Non-Existent Board", columnOrdinal, newLimit);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "LimitColumnTest 13 Passed" : "LimitColumnTest 13 Failed");

            // Test 14: Invalid Column Ordinal
            int invalidColumnOrdinal = 5;
            string result4 = serviceFactory.BS.LimitColumn(validEmail, boardName1, invalidColumnOrdinal, newLimit);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Illegal input." ? "LimitColumnTest 14 Passed" : "LimitColumnTest 14 Failed");

            // Test 15: Invalid Limit (less than 0)
            int invalidLimit = -1;
            string result5 = serviceFactory.BS.LimitColumn(validEmail, boardName1, columnOrdinal, invalidLimit);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "Illegal input." ? "LimitColumnTest 15 Passed" : "LimitColumnTest 15 Failed");

            // Test 16: Existing Board but not owned by the user
            serviceFactory.US.Register("another@example.com", "Valid2Password!");
            serviceFactory.BS.CreateBoard("another@example.com", existingBoardName);
            string result6 = serviceFactory.BS.LimitColumn(validEmail, existingBoardName, columnOrdinal, newLimit);
            Response? r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "No such board." ? "LimitColumnTest 16 Passed" : "LimitColumnTest 16 Failed");

            //Test 17: Trying to change a column's limit when there are too many tasks in the column
            serviceFactory.BS.AddTask(validEmail, boardName1, "test Task 1", "1", DateTime.Now.AddDays(7));
            serviceFactory.BS.AddTask(validEmail, boardName1, "test Task 2", "2", DateTime.Now.AddDays(7));
            string result7 = serviceFactory.BS.LimitColumn(validEmail, boardName1, 0, 1);
            Response? r7 = JsonSerializer.Deserialize<Response>(result7);
            Console.WriteLine(r7?.ErrorMessage == "Too many Tasks in Column." ? "LimitColumnTest 17 Passed" : "LimitColumnTest 17 Failed");

            //Test bonus: Trying to change column limit to the same limit
            string result8= serviceFactory.BS.LimitColumn(validEmail, boardName1, columnOrdinal, newLimit);
            Response? r8 = JsonSerializer.Deserialize<Response>(result8);
            Console.WriteLine(r8?.ErrorMessage == "Limit already set." ? "LimitColumnTest bonus Passed" : "LimitColumnTest bonus Failed");
            

            //Delete this scope's data
            serviceFactory.DeleteData();
        }
        
        public void AddTaskTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string nonExistentBoardName = "Non-Existent Board";
            string title = "Sample Task";
            string description = "Sample Task Description";
            DateTime dueDate = DateTime.Now.AddDays(7);

            serviceFactory.DeleteData();

            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            serviceFactory.BS.CreateBoard(validEmail, boardName1);

            // Test 18: Successfully add a task
            string result1 = serviceFactory.BS.AddTask(validEmail, boardName1, title, description, dueDate);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "AddTaskTest 18 Passed" : "AddTaskTest 18 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, "Valid1Password!");
            string res = serviceFactory.BS.GetColumn(validEmail, boardName1, 0);
            Response response = JsonSerializer.Deserialize<Response>(res);
            List<taskSL> list = ConvertReturnValue<List<taskSL>>(response?.ReturnValue);
            if (list != null)
            {
                bool b = true;
                foreach (taskSL item in list)
                {
                    if ((!item.Assignee.Equals("")) || (!item.Title.Equals(title)) || (!item.Description.Equals(description)))
                        b = false;
                }
                Console.WriteLine(b ? "AddTaskTest 19 Passed" : "AddTaskTest 19 Failed");
            }
            else
                Console.WriteLine("AddTaskTest 19 Failed");

            // Test 20: Invalid User
            string result2 = serviceFactory.BS.AddTask(invalidEmail, boardName1, title, description, dueDate);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "AddTaskTest 20 Passed" : "AddTaskTest 20 Failed");

            // Test 21: Non-Existent Board
            string result3 = serviceFactory.BS.AddTask(validEmail, nonExistentBoardName, title, description, dueDate);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "AddTaskTest 21 Passed" : "AddTaskTest 21 Failed");

            // Test 22: Invalid Due Date (Past Due Date)
            DateTime pastDueDate = DateTime.Now.AddDays(-1);
            string result4 = serviceFactory.BS.AddTask(validEmail, boardName1, title, description, pastDueDate);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Due date must be in the future." ? "AddTaskTest 22 Passed" : "AddTaskTest 22 Failed");

            //Test 23: Long Title 
            string longTitle = "123456789012345678901234567890123123456789012345678901234567890";
            string result5 = serviceFactory.BS.AddTask(validEmail, boardName1, longTitle, description, dueDate);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "Illegal input." ? "AddTaskTest 23 Passed" : "AddTaskTest 23 Failed");

            //Test 24: Empty Title 
            string EmptyTitle = "";
            string result6 = serviceFactory.BS.AddTask(validEmail, boardName1, EmptyTitle, description, dueDate);
            Response? r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Illegal input." ? "AddTaskTest 24 Passed" : "AddTaskTest 24 Failed");

            //Test 25: Long description 
            string LongDescription = "";
            for (int i = 0; i < 10; i++)
            {
                LongDescription += "1234567890123456789012345678901234567890";
            }
            string result7 = serviceFactory.BS.AddTask(validEmail, boardName1, title, LongDescription, dueDate);
            Response? r7 = JsonSerializer.Deserialize<Response>(result7);
            Console.WriteLine(r7?.ErrorMessage == "Illegal input." ? "AddTaskTest 25 Passed" : "AddTaskTest 25 Failed");

            //Test 26: Empty Description
            string EmptyDescription = "";
            string result8 = serviceFactory.BS.AddTask(validEmail, boardName1, title, EmptyDescription, dueDate);
            Response? r8 = JsonSerializer.Deserialize<Response>(result8);
            Console.WriteLine(r8?.ErrorMessage == null ? "AddTaskTest 26 Passed" : "AddTaskTest 26 Failed");

            //Test 27: Too many Tasks
            serviceFactory.BS.LimitColumn(validEmail, boardName1, 0, 4);
            serviceFactory.BS.AddTask(validEmail, boardName1, title, "last task", dueDate);
            serviceFactory.BS.AddTask(validEmail, boardName1, title, "last task promise", dueDate);
            string result9 = serviceFactory.BS.AddTask(validEmail, boardName1, title, "too much tasks", dueDate);
            Response? r9 = JsonSerializer.Deserialize<Response>(result9);
            Console.WriteLine(r9?.ErrorMessage == "Too many tasks in column." ? "AddTaskTest 27 Passed" : "AddTaskTest 27 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();

        }

        public void AdvanceTaskTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string nonExistentBoardName = "Non-Existent Board";
            string title1 = "Sample Task 1";
            string description = "Sample Task Description";
            DateTime dueDate = DateTime.Now.AddDays(7);

            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response response1 = JsonSerializer.Deserialize<Response>(res);
            BoardSL b = ConvertReturnValue<BoardSL>(response1?.ReturnValue);
            int boardId = b.boardId;

            // Add a task to the first column
            string resultAddTask = serviceFactory.BS.AddTask(validEmail, boardName1, title1, description, dueDate);
            Response taskResponse = JsonSerializer.Deserialize<Response>(resultAddTask);
            int taskId = GetTaskIdFromResponse(taskResponse);

            // Test 28: Successfully advance a task
            serviceFactory.TS.AssignTask("", boardName1, 0, taskId, validEmail);
            string result1 = serviceFactory.BS.AdvanceTask(validEmail, boardName1, 0, taskId);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "AdvanceTaskTest 28 Passed" : "AdvanceTaskTest 28 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, "Valid1Password!");
            string s = serviceFactory.BS.GetColumn(validEmail, boardName1, 1);
            Response response2 = JsonSerializer.Deserialize<Response>(s);
            List<taskSL> list = ConvertReturnValue<List<taskSL>>(response2?.ReturnValue);
            if (list != null)
            {
                bool a = true;
                foreach (taskSL item in list)
                {
                    if (item.Assignee != validEmail || item.Title != title1 || item.Description != description || item.DueDate.Equals(dueDate))
                        a = false;
                }
                Console.WriteLine(a ? "AdvanceTaskTest 29 Passed" : "AdvanceTaskTest 29 Failed");
            }
            else
                Console.WriteLine("AdvanceTaskTest 29 Failed");

            // Test 30: Invalid User
            string result2 = serviceFactory.BS.AdvanceTask(invalidEmail, boardName1, 0, taskId);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "AdvanceTaskTest 30 Passed" : "AdvanceTaskTest 30 Failed");

            // Test 31: Non-Existent Board
            string result3 = serviceFactory.BS.AdvanceTask(validEmail, nonExistentBoardName, 0, taskId);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "AdvanceTaskTest 31 Passed" : "AdvanceTaskTest 31 Failed");

            // Test 32: Invalid Column Ordinal
            string result4 = serviceFactory.BS.AdvanceTask(validEmail, boardName1, 5, taskId);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Invalid column ordinal." ? "AdvanceTaskTest 32 Passed" : "AdvanceTaskTest 32 Failed");

            // Test 33: Invalid Task ID
            string result5 = serviceFactory.BS.AdvanceTask(validEmail, boardName1, 0, -2);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "Task not found." ? "AdvanceTaskTest 33 Passed" : "AdvanceTaskTest 33 Failed");

            //Test 34: Advence a task by a non assignee member
            string validEmail2 = "valid2@example.com";
            serviceFactory.US.Register(validEmail2, "Valid1Password!");
            serviceFactory.BS.JoinBoard(validEmail2, boardId);
            string result6 = serviceFactory.BS.AdvanceTask(validEmail2, boardName1, 1, taskId);
            Response r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Not the task's assignee" ? "AdvanceTaskTest 34 Passed" : "AdvanceTaskTest 34 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        public void InProgressTasksTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string title1 = "Sample Task 1";
            string title2 = "Sample Task 2";
            string description = "Sample Task Description";
            DateTime dueDate = DateTime.Now.AddDays(7);

            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            serviceFactory.BS.CreateBoard(validEmail, boardName1);

            // Add tasks to the board
            string resultAddTask1 = serviceFactory.BS.AddTask(validEmail, boardName1, title1, description, dueDate);
            Response taskResponse1 = JsonSerializer.Deserialize<Response>(resultAddTask1);
            int taskId1 = GetTaskIdFromResponse(taskResponse1);

            string resultAddTask2 = serviceFactory.BS.AddTask(validEmail, boardName1, title2, description, dueDate);
            Response taskResponse2 = JsonSerializer.Deserialize<Response>(resultAddTask2);
            int taskId2 = GetTaskIdFromResponse(taskResponse2);

            // Advance the first task to the next column
            serviceFactory.TS.AssignTask("", boardName1, 0, taskId1, validEmail);
            serviceFactory.BS.AdvanceTask(validEmail, boardName1, 0, taskId1);


            // Test 35: Successfully get in-progress tasks for a valid user
            string result1 = serviceFactory.TS.InProgressTasks(validEmail);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            List<taskSL> list = ConvertReturnValue<List<taskSL>>(r1?.ReturnValue);
            bool b = false;
            foreach (taskSL task in list)
            {
                if (task.Id == taskId1)
                    b = true;
            }
            Console.WriteLine(b ? "InProgressTasksTest 35 Passed" : "InProgressTasksTest 35 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, "Valid1Password!");
            string resultD6 = serviceFactory.TS.InProgressTasks(validEmail);
            Response rD6 = JsonSerializer.Deserialize<Response>(resultD6);
            List<taskSL> list6 = ConvertReturnValue<List<taskSL>>(rD6?.ReturnValue);
            if (list6.Count == 0)
            {
                Console.WriteLine("InProgressTasksTest 36 Failed");
            }
            else
            {
                bool d = false;
                foreach (taskSL task in list6)
                {
                    if (task.Id == taskId1)
                        d = true;
                }
                Console.WriteLine(d ? "InProgressTasksTest 36 Passed" : "InProgressTasksTest 36 Failed");
            }

            // Test 37: Invalid User
            string result2 = serviceFactory.TS.InProgressTasks(invalidEmail);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "InProgressTasksTest 37 Passed" : "InProgressTasksTest 37 Failed");

            //Test 38:  Empty In-Progress Tasks List
            // Remove all tasks from the in-progress column on board 1
            serviceFactory.BS.AdvanceTask(validEmail, boardName1, 1, taskId1);
            string result3 = serviceFactory.TS.InProgressTasks(validEmail);
            Response r3 = JsonSerializer.Deserialize<Response>(result3);
            List<taskSL> list2 = ConvertReturnValue<List<taskSL>>(r3?.ReturnValue);
            Console.WriteLine(list2.Count == 0 ? "InProgressTasksTest 38 Passed" : "InProgressTasksTest 38 Failed");

            // Test 39: Multiple Boards with Tasks in In-Progress Column
            string boardName2 = "Test Board 2";
            serviceFactory.BS.CreateBoard(validEmail, boardName2);
            serviceFactory.TS.AssignTask("", boardName1, 0, taskId2, validEmail);
            serviceFactory.BS.AdvanceTask(validEmail, boardName1, 0, taskId2);

            string resultAddTask3 = serviceFactory.BS.AddTask(validEmail, boardName2, title2, description, dueDate);
            Response taskResponse3 = JsonSerializer.Deserialize<Response>(resultAddTask3);
            int taskId3 = GetTaskIdFromResponse(taskResponse3);
            serviceFactory.TS.AssignTask("", boardName2, 0, taskId3, validEmail);
            serviceFactory.BS.AdvanceTask(validEmail, boardName2, 0, taskId3);
            string result4 = serviceFactory.TS.InProgressTasks(validEmail);
            Response r4 = JsonSerializer.Deserialize<Response>(result4);
            List<taskSL> list3 = ConvertReturnValue<List<taskSL>>(r4?.ReturnValue);
            Console.WriteLine(list3.Count == 2 ? "InProgressTasksTest 39 Passed" : "InProgressTasksTest 39 Failed");

            //Test 40: Unassigned task
            string resultAddTask4 = serviceFactory.BS.AddTask(validEmail, boardName1, title2, description, dueDate);
            Response taskResponse4 = JsonSerializer.Deserialize<Response>(resultAddTask4);
            int taskId4 = GetTaskIdFromResponse(taskResponse4);
            serviceFactory.BS.AdvanceTask(validEmail, boardName1, 0, taskId4);
            string result5 = serviceFactory.TS.InProgressTasks(validEmail);
            Response r5 = JsonSerializer.Deserialize<Response>(result5);
            List<taskSL> list4 = ConvertReturnValue<List<taskSL>>(r5?.ReturnValue);
            Console.WriteLine(list4.Count == 2 ? "InProgressTasksTest 40 Passed" : "InProgressTasksTest 40 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        public void GetColumnLimitTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string nonExistentBoardName = "Non-Existent Board";
            string title = "Sample Task";
            string description = "Sample Task Description";
            DateTime dueDate = DateTime.Now.AddDays(7);


            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            serviceFactory.BS.CreateBoard(validEmail, boardName1);

            // Add a task to the first column
            string resultAddTask = serviceFactory.BS.AddTask(validEmail, boardName1, title, description, dueDate);
            Response taskResponse = JsonSerializer.Deserialize<Response>(resultAddTask);
            int taskId = GetTaskIdFromResponse(taskResponse);

            // Test 41: Successfully get column limit
            string result1 = serviceFactory.BS.GetColumnLimit(validEmail, boardName1, 0);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "GetColumnLimitTest 41 Passed" : "GetColumnLimitTest 41 Failed");


            // Test 32: Invalid User
            string result2 = serviceFactory.BS.GetColumnLimit(invalidEmail, boardName1, 0);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "GetColumnLimitTest 42 Passed" : "GetColumnLimitTest 42 Failed");

            // Test 43: Non-Existent Board
            string result3 = serviceFactory.BS.GetColumnLimit(validEmail, nonExistentBoardName, 0);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "GetColumnLimitTest 43 Passed" : "GetColumnLimitTest 43 Failed");

            // Test 44: Invalid Column Ordinal
            string result4 = serviceFactory.BS.GetColumnLimit(validEmail, boardName1, 5);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Invalid column ordinal." ? "GetColumnLimitTest 44 Passed" : "GetColumnLimitTest 44 Failed");

            //Test 45: Non-deafult limit
            serviceFactory.BS.LimitColumn(validEmail, boardName1, 0, 40);
            string result5 = serviceFactory.BS.GetColumnLimit(validEmail, boardName1, 0);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            int returnValue = ConvertReturnValue<int>(r5?.ReturnValue);
            Console.WriteLine(returnValue == 40 ? "GetColumnLimitTest 45 Passed" : "GetColumnLimitTest 45 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, "Valid1Password!");
            string resultd7 = serviceFactory.BS.GetColumnLimit(validEmail, boardName1, 0);
            Response rd7 = JsonSerializer.Deserialize<Response>(resultd7);
            int returnValue2 = ConvertReturnValue<int>(rd7?.ReturnValue);
            Console.WriteLine(rd7?.ErrorMessage == null && returnValue2 == 40 ? "GetColumnLimitTest 46 Passed" : "GetColumnLimitTest 46 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }
        public T ConvertReturnValue<T>(object returnValue)
        {
            if (returnValue is JsonElement jsonElement)
            {
                return JsonSerializer.Deserialize<T>(jsonElement);
            }
            return (T)returnValue;
        }

        public void GetColumnNameTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string nonExistentBoardName = "Non-Existent Board";
            string title = "Sample Task";
            string description = "Sample Task Description";
            DateTime dueDate = DateTime.Now.AddDays(7);

            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            serviceFactory.BS.CreateBoard(validEmail, boardName1);

            // Add a task to the first column
            string resultAddTask = serviceFactory.BS.AddTask(validEmail, boardName1, title, description, dueDate);
            Response taskResponse = JsonSerializer.Deserialize<Response>(resultAddTask);
            int taskId = GetTaskIdFromResponse(taskResponse);

            // Test 47: Successfully get column name
            string result1 = serviceFactory.BS.GetColumnName(validEmail, boardName1, 0);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            string returnValue = ConvertReturnValue<string>(r1?.ReturnValue);
            Console.WriteLine(returnValue == "backlog" ? "GetColumnNameTest 47 Passed" : "GetColumnNameTest 47 Failed");


            // Test 48: Invalid User
            string result2 = serviceFactory.BS.GetColumnName(invalidEmail, boardName1, 0);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "GetColumnNameTest 48 Passed" : "GetColumnNameTest 48 Failed");

            // Test 49: Non-Existent Board
            string result3 = serviceFactory.BS.GetColumnName(validEmail, nonExistentBoardName, 0);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "GetColumnNameTest 49 Passed" : "GetColumnNameTest 49 Failed");

            // Test 50: Invalid Column Ordinal
            string result4 = serviceFactory.BS.GetColumnName(validEmail, boardName1, 5);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Invalid column ordinal." ? "GetColumnNameTest 50 Passed" : "GetColumnNameTest 50 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        public void GetColumnTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string boardName1 = "Test Board 1";
            string nonExistentBoardName = "Non-Existent Board";
            string title = "Sample Task";
            string description = "Sample Task Description";
            DateTime dueDate = DateTime.Now.AddDays(7);

            // Register a valid user and create a board
            serviceFactory.US.Register(validEmail, "Valid1Password!");
            serviceFactory.BS.CreateBoard(validEmail, boardName1);

            // Add tasks to the first column
            string resultAddTask = serviceFactory.BS.AddTask(validEmail, boardName1, title, description, dueDate);
            string resultAddTask2 = serviceFactory.BS.AddTask(validEmail, boardName1, "test2", "description2", DateTime.Now.AddMonths(1));

            // Test 51: Successfully get column
            string result1 = serviceFactory.BS.GetColumn(validEmail, boardName1, 0);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            List<taskSL> tasks = ConvertReturnValue<List<taskSL>>(r1?.ReturnValue);
            Console.WriteLine(tasks.Count == 2 && tasks[0].Title == title && tasks[1].Title == "test2" ? "GetColumnTest 51 Passed" : "GetColumnTest 51 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, "Valid1Password!");
            string resultd8 = serviceFactory.BS.GetColumn(validEmail, boardName1, 0);
            Response rd8 = JsonSerializer.Deserialize<Response>(resultd8);
            List<taskSL> tasksd1 = ConvertReturnValue<List<taskSL>>(rd8?.ReturnValue);
            Console.WriteLine(tasksd1.Count == 2 && tasksd1[0].Title == title && tasksd1[1].Title == "test2" ? "GetColumnTest 52 Passed" : "GetColumnTest 52 Failed");

            // Test 53: Invalid User
            string result2 = serviceFactory.BS.GetColumn(invalidEmail, boardName1, 0);
            Response r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "GetColumnTest 53 Passed" : "GetColumnTest 53 Failed");

            // Test 54: Non-Existent Board
            string result3 = serviceFactory.BS.GetColumn(validEmail, nonExistentBoardName, 0);
            Response r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "GetColumnTest 54 Passed" : "GetColumnTest 54 Failed");

            // Test 55: Invalid Column Ordinal
            string result4 = serviceFactory.BS.GetColumn(validEmail, boardName1, 5);
            Response r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Invalid column ordinal." ? "GetColumnTest 55 Passed" : "GetColumnTest 55 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        private int GetTaskIdFromResponse(Response? response)
        {
            if (response?.ReturnValue != null)
            {
                using (JsonDocument doc = JsonDocument.Parse(response.ReturnValue.ToString()))
                {
                    JsonElement root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("Id", out JsonElement idElement))
                    {
                        return idElement.GetInt32();
                    }
                }
            }
            return -1;
        }

        private int GetBoardIdFromResponse(Response? response)
        {
            if (response?.ReturnValue != null)
            {
                using (JsonDocument doc = JsonDocument.Parse(response.ReturnValue.ToString()))
                {
                    JsonElement root = doc.RootElement;
                    if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty("boardId", out JsonElement idElement))
                    {
                        return idElement.GetInt32();
                    }
                }
            }
            return -1;
        }


        public void RegisterTests()
        {
            Setup();

            string validEmail = "valid@example.com";
            string duplicateEmail = "duplicate@example.com";
            string invalidEmailFormat1 = "invalidEmail";
            string invalidEmailFormat2 = "invalidEmail@";
            string invalidEmailFormat3 = "invalid@Email";
            string shortPassword = "short";
            string longPassword = "ThisPasswordIsWayTooLong123";
            string noDigitPassword = "NoDigits";
            string noUpperPassword = "nodigits1";
            string noLowerPassword = "NOLOWERCASE1";
            string validPassword = "Valid1Password";
            string upperValidEmail = "VALid@example.com";

            // Test 56: Valid Registration
            string result1 = serviceFactory.US.Register(validEmail, validPassword);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "RegisterTest 56 Passed" : "RegisterTest 56 Failed");
            //Test bonus: check lower and upper case
            string resultA = serviceFactory.US.Register(upperValidEmail, validPassword);
            Response? rA = JsonSerializer.Deserialize<Response>(resultA);
            Console.WriteLine(rA?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest A Passed" : "RegisterTest A Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            string resultd9 = serviceFactory.US.Login(validEmail, validPassword);
            Response rd9 = JsonSerializer.Deserialize<Response>(resultd9);
            UserSL user = ConvertReturnValue<UserSL>(rd9?.ReturnValue);
            Console.WriteLine(user.email == validEmail ? "RegisterTest 57 Passed" : "RegisterTest 57 Failed");

            // Test 58: Duplicate Registration
            serviceFactory.US.Register(duplicateEmail, validPassword);
            string result2 = serviceFactory.US.Register(duplicateEmail, validPassword);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 58 Passed" : "RegisterTest 58 Failed");

            // Test 59: Invalid Email Format 1 (missing domain)
            string result3 = serviceFactory.US.Register(invalidEmailFormat1, validPassword);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 59 Passed" : "RegisterTest 59 Failed");

            // Test 60: Invalid Email Format 2 (missing domain name)
            string result4 = serviceFactory.US.Register(invalidEmailFormat2, validPassword);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 60 Passed" : "RegisterTest 60 Failed");

            // Test 61: Invalid Email Format 3 (missing TLD)
            string result5 = serviceFactory.US.Register(invalidEmailFormat3, validPassword);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 61 Passed" : "RegisterTest 61 Failed");

            // Test 62: Short Password
            string result6 = serviceFactory.US.Register(validEmail, shortPassword);
            Response? r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 62 Passed" : "RegisterTest 62 Failed");

            // Test 63: Long Password
            string result7 = serviceFactory.US.Register(validEmail, longPassword);
            Response? r7 = JsonSerializer.Deserialize<Response>(result7);
            Console.WriteLine(r7?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 63 Passed" : "RegisterTest 63 Failed");

            // Test 64: Password without Digit
            string result8 = serviceFactory.US.Register(validEmail, noDigitPassword);
            Response? r8 = JsonSerializer.Deserialize<Response>(result8);
            Console.WriteLine(r8?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 64 Passed" : "RegisterTest 64 Failed");

            // Test 65: Password without Uppercase Letter
            string result9 = serviceFactory.US.Register(validEmail, noUpperPassword);
            Response? r9 = JsonSerializer.Deserialize<Response>(result9);
            Console.WriteLine(r9?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 65 Passed" : "RegisterTest 65 Failed");

            // Test 66: Password without Lowercase Letter
            string result10 = serviceFactory.US.Register(validEmail, noLowerPassword);
            Response? r10 = JsonSerializer.Deserialize<Response>(result10);
            Console.WriteLine(r10?.ErrorMessage == "Illegal password or user already exists." ? "RegisterTest 67 Passed" : "RegisterTest 67 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        public void LoginTests()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string wrongPassword = "Wrong1Password";
            string validPassword = "Valid1Password";
            string shortPassword = "short";
            string longPassword = "ThisPasswordIsWayTooLong123";
            string noDigitPassword = "NoDigits";
            string noUpperPassword = "nodigits1";
            string noLowerPassword = "NOLOWERCASE1";

            // Register and logout a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            serviceFactory.US.Logout(validEmail);

            // Test 68: Valid Login
            string result1 = serviceFactory.US.Login(validEmail, validPassword);
            serviceFactory.US.Logout(validEmail);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "LoginTest 68 Passed" : "LoginTest 68 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            string result10 = serviceFactory.US.Login(validEmail, validPassword);
            serviceFactory.US.Logout(validEmail);
            Response rd10 = JsonSerializer.Deserialize<Response>(result10);
            Console.WriteLine(rd10?.ErrorMessage == null ? "LoginTest 69 Passed" : "LoginTest 69 Failed");

            // Test 70: Invalid User
            string result2 = serviceFactory.US.Login(invalidEmail, validPassword);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user or password not compliant with the rules." ? "LoginTest 70 Passed" : "LoginTest 70 Failed");

            // Test 71: Wrong Password
            string result3 = serviceFactory.US.Login(validEmail, wrongPassword);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "Wrong password." ? "LoginTest 71 Passed" : "LoginTest 71 Failed");

            // Test 72: Short Password
            string result4 = serviceFactory.US.Login(validEmail, shortPassword);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "No such user or password not compliant with the rules." ? "LoginTest 72 Passed" : "LoginTest 72 Failed");

            // Test 73: Long Password
            string result5 = serviceFactory.US.Login(validEmail, longPassword);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "No such user or password not compliant with the rules." ? "LoginTest 73 Passed" : "LoginTest 73 Failed");

            // Test 74: Password without Digit
            string result6 = serviceFactory.US.Login(validEmail, noDigitPassword);
            Response? r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "No such user or password not compliant with the rules." ? "LoginTest 74 Passed" : "LoginTest 74 Failed");

            // Test 75: Password without Uppercase Letter
            string result7 = serviceFactory.US.Login(validEmail, noUpperPassword);
            Response? r7 = JsonSerializer.Deserialize<Response>(result7);
            Console.WriteLine(r7?.ErrorMessage == "No such user or password not compliant with the rules." ? "LoginTest 75 Passed" : "LoginTest 75 Failed");

            // Test 76: Password without Lowercase Letter
            string result8 = serviceFactory.US.Login(validEmail, noLowerPassword);
            Response? r8 = JsonSerializer.Deserialize<Response>(result8);
            Console.WriteLine(r8?.ErrorMessage == "No such user or password not compliant with the rules." ? "LoginTest 76 Passed" : "LoginTest 76 Failed");

            // Test 77: Login Already Logged-in User
            string result9 = serviceFactory.US.Login(validEmail, validPassword); // Log in first time
            Response? r9_1 = JsonSerializer.Deserialize<Response>(result9);
            string result9_2 = serviceFactory.US.Login(validEmail, validPassword); // Attempt to log in again
            Response? r9_2 = JsonSerializer.Deserialize<Response>(result9_2);
            Console.WriteLine(r9_2?.ErrorMessage == "User already logged in." ? "LoginTest 77 Passed" : "LoginTest 77 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        public void LogoutTests()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";

            // Register and login a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            serviceFactory.US.Login(validEmail, validPassword);

            // Test 78: Valid Logout
            string result1 = serviceFactory.US.Logout(validEmail);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            Console.WriteLine(r1?.ErrorMessage == null ? "LogoutTest 78 Passed" : "LogoutTest 78 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, validPassword);
            string resultd11 = serviceFactory.US.Logout(validEmail);
            Response rd11 = JsonSerializer.Deserialize<Response>(resultd11);
            Console.WriteLine(rd11?.ErrorMessage == null ? "LogoutTest 79 Passed" : "LogoutTest 79 Failed");

            // Test 80: Logout Invalid User
            string result2 = serviceFactory.US.Logout(invalidEmail);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "LogoutTest 80 Passed" : "LogoutTest 80 Failed");

            // Test 81: Logout Not Logged In User
            serviceFactory.US.Logout(validEmail); // Log out first
            string result3 = serviceFactory.US.Logout(validEmail); // Attempt to log out again
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "Not logged in." ? "LogoutTest 81 Passed" : "LogoutTest 81 Failed");

            // Test 82: Logout with Incorrect Email Format
            string result4 = serviceFactory.US.Logout("invalidEmailFormat");
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "No such user." ? "LogoutTest 82 Passed" : "LogoutTest 82 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }


        public void UpdateTaskDueDateTests()
        {
            Setup();

            string validEmail = "valid@example.com";
            string validEmail2 = "valid2@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";
            string boardName = "Test Board";
            DateTime validDueDate = DateTime.Now.AddDays(5);
            DateTime pastDueDate = DateTime.Now.AddDays(-5); // Invalid due date in the past
            int validColumnOrdinal = 0;
            int invalidColumnOrdinal = 99;
            int taskId = -1; // Assuming a task with ID 1 exists

            // Register and login a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            serviceFactory.US.Login(validEmail, validPassword);
            string res2 = serviceFactory.BS.CreateBoard(validEmail, boardName);
            Response r2d = JsonSerializer.Deserialize<Response>(res2);
            int boardId = GetBoardIdFromResponse(r2d);
            string res = serviceFactory.BS.AddTask(validEmail, boardName, "Initial Title", "Initial Description", validDueDate);
            Response r0 = JsonSerializer.Deserialize<Response>(res);
            int taskId0 = GetTaskIdFromResponse(r0);


            // Test 83: Valid Update Task Due Date
            serviceFactory.TS.AssignTask("",boardName,validColumnOrdinal,taskId0, validEmail);
            string result1 = serviceFactory.TS.UpdateTaskDueDate(validEmail, boardName, validColumnOrdinal, taskId0, validDueDate.AddHours(8));
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            taskSL task = ConvertReturnValue<taskSL>(r1?.ReturnValue);
            Console.WriteLine(task.DueDate.Equals(validDueDate.AddHours(8)) ? "UpdateTaskDueDateTest 83 Passed" : "UpdateTaskDueDateTest 83 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, validPassword);
            string resultd12 = serviceFactory.BS.GetColumn(validEmail, boardName, 0);
            Response rd12 = JsonSerializer.Deserialize<Response>(resultd12);
            List<taskSL> list = ConvertReturnValue<List<taskSL>>(rd12?.ReturnValue);
            bool b = true;
            foreach (taskSL taskSL in list)
            {
                if (!taskSL.DueDate.Equals(validDueDate.AddHours(8)))
                    b = false;
            }
            if (list.Count != 1)
                b = false;
            Console.WriteLine(b ? "UpdateTaskDueDateTest 84 Passed" : "UpdateTaskDueDateTest 84 Failed");

            // Test 85: Invalid User
            string result2 = serviceFactory.TS.UpdateTaskDueDate(invalidEmail, boardName, validColumnOrdinal, taskId0, validDueDate);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "UpdateTaskDueDateTest 85 Passed" : "UpdateTaskDueDateTest 85 Failed");

            // Test 86: Invalid Board
            string result3 = serviceFactory.TS.UpdateTaskDueDate(validEmail, "Invalid Board", validColumnOrdinal, taskId0, validDueDate);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "UpdateTaskDueDateTest 86 Passed" : "UpdateTaskDueDateTest 86 Failed");

            // Test 87: Invalid Column Ordinal
            string result4 = serviceFactory.TS.UpdateTaskDueDate(validEmail, boardName, invalidColumnOrdinal, taskId0, validDueDate);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Invalid column ordinal." ? "UpdateTaskDueDateTest 87 Passed" : "UpdateTaskDueDateTest 87 Failed");

            // Test 88: Past Due Date
            string result5 = serviceFactory.TS.UpdateTaskDueDate(validEmail, boardName, validColumnOrdinal, taskId0, pastDueDate);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "Due date must be in the future." ? "UpdateTaskDueDateTest 88 Passed" : "UpdateTaskDueDateTest 88 Failed");

            //Test bonus: Change task by different assignee
            serviceFactory.US.Register(validEmail2, validPassword);
            serviceFactory.BS.JoinBoard(validEmail2, boardId);
            string result10 = serviceFactory.TS.UpdateTaskDueDate(validEmail2, boardName, validColumnOrdinal, taskId0, validDueDate);
            Response? r10 = JsonSerializer.Deserialize<Response>(result10);
            Console.WriteLine(r10?.ErrorMessage == "Not the correct Assignee." ? "UpdateTaskDescriptionTest bonus2 Passed" : "UpdateTaskDescriptionTest bonus2 Failed");

            //Test 89: Update task in "done" column
            serviceFactory.BS.AdvanceTask(validEmail, boardName, 0, taskId0);
            serviceFactory.BS.AdvanceTask(validEmail, boardName, 1, taskId0);
            string result6 = serviceFactory.TS.UpdateTaskDueDate(validEmail, boardName, 2, taskId0, validDueDate);
            Response? r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Can't update finished tasks." ? "UpdateTaskDueDateTest 89 Passed" : "UpdateTaskDueDateTest 89 Failed");

            

            //Delete this scope's data
            serviceFactory.DeleteData();

        }


        public void UpdateTaskDescriptionTests()
        {
            Setup();

            string validEmail = "valid@example.com";
            string validEmail2 = "valid2@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";
            string boardName = "Test Board";
            string validDescription = "Valid Description";
            int validColumnOrdinal = 0;
            int invalidColumnOrdinal = 99;
            int taskId = -1; // Assuming a task with ID -1 exists
            DateTime dueDate = DateTime.Now.AddDays(5);
            string InvalidDescription = "";

            for (int i = 0; i < 10; i++)
            {
                InvalidDescription += "1234567890123456789012345678901234567890";
            }
            // Register and login a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            serviceFactory.US.Login(validEmail, validPassword);
            string res2 = serviceFactory.BS.CreateBoard(validEmail, boardName);
            Response r2d = JsonSerializer.Deserialize<Response>(res2);
            int boardId = GetBoardIdFromResponse(r2d);
            string res = serviceFactory.BS.AddTask(validEmail, boardName, "Initial Title", "Initial Description", dueDate);
            Response r0 = JsonSerializer.Deserialize<Response>(res);
            int taskId0 = GetTaskIdFromResponse(r0);

            // Test 90: Valid Update Task Description
            serviceFactory.TS.AssignTask("", boardName, validColumnOrdinal, taskId0, validEmail);
            string result1 = serviceFactory.TS.UpdateTaskDescription(validEmail, boardName, validColumnOrdinal, taskId0, validDescription);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            taskSL task = ConvertReturnValue<taskSL>(r1?.ReturnValue);
            Console.WriteLine((task.Description).Equals(validDescription) ? "UpdateTaskDescriptionTest 90 Passed" : "UpdateTaskDescriptionTest 90 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, validPassword);
            string resultd13 = serviceFactory.BS.GetColumn(validEmail, boardName, 0);
            Response rd13 = JsonSerializer.Deserialize<Response>(resultd13);
            List<taskSL> list = ConvertReturnValue<List<taskSL>>(rd13?.ReturnValue);
            bool b = true;
            foreach (taskSL taskSL in list)
            {
                if (!taskSL.Description.Equals(validDescription))
                    b = false;
            }
            if (list.Count != 1)
                b = false;
            Console.WriteLine(b ? "UpdateTaskDescriptionTest 91 Passed" : "UpdateTaskDescriptionTest 91 Failed");


            // Test 92: Invalid User
            string result2 = serviceFactory.TS.UpdateTaskDescription(invalidEmail, boardName, validColumnOrdinal, taskId0, validDescription);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "UpdateTaskDescriptionTest 92 Passed" : "UpdateTaskDescriptionTest 92 Failed");

            // Test 93: Invalid Board
            string result3 = serviceFactory.TS.UpdateTaskDescription(validEmail, "Invalid Board", validColumnOrdinal, taskId0, validDescription);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "UpdateTaskDescriptionTest 93 Passed" : "UpdateTaskDescriptionTest 93 Failed");

            // Test 94: Invalid Column Ordinal
            string result4 = serviceFactory.TS.UpdateTaskDescription(validEmail, boardName, invalidColumnOrdinal, taskId0, validDescription);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Invalid column ordinal." ? "UpdateTaskDescriptionTest 94 Passed" : "UpdateTaskDescriptionTest 94 Failed");

            //Test 95: Invalid Board
            string result5 = serviceFactory.TS.UpdateTaskDescription(validEmail, "", validColumnOrdinal, taskId0, validDescription);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "No such board." ? "UpdateTaskDescriptionTest 95 Passed" : "UpdateTaskDescriptionTest 95 Failed");

            //Test 96: Invalid Task
            string result6 = serviceFactory.TS.UpdateTaskDescription(validEmail, boardName, validColumnOrdinal, 99, validDescription);
            Response? r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Task does not exist" ? "UpdateTaskDescriptionTest 96 Passed" : "UpdateTaskDescriptionTest 96 Failed");

            //Test 97: Invalid description
            string result7 = serviceFactory.TS.UpdateTaskDescription(validEmail, boardName, validColumnOrdinal, taskId0, InvalidDescription);
            Response? r7 = JsonSerializer.Deserialize<Response>(result7);
            Console.WriteLine(r7?.ErrorMessage == "Description too long." ? "UpdateTaskDescriptionTest 97 Passed" : "UpdateTaskDescriptionTest 97 Failed");

            //Test bonus: Change task by different assignee
            serviceFactory.US.Register(validEmail2, validPassword);
            serviceFactory.BS.JoinBoard(validEmail2, boardId);
            string result10 = serviceFactory.TS.UpdateTaskDescription(validEmail2, boardName, validColumnOrdinal, taskId0, validDescription);
            Response? r10 = JsonSerializer.Deserialize<Response>(result10);
            Console.WriteLine(r10?.ErrorMessage == "Not the correct Assignee." ? "UpdateTaskDescriptionTest bonus3 Passed" : "UpdateTaskDescriptionTest bonus3 Failed");

            //Test 98: Update task in "done" column
            serviceFactory.BS.AdvanceTask(validEmail, boardName, 0, taskId);
            serviceFactory.BS.AdvanceTask(validEmail, boardName, 1, taskId);
            string result8 = serviceFactory.TS.UpdateTaskDescription(validEmail, boardName, 2, taskId0, validDescription);
            Response? r8 = JsonSerializer.Deserialize<Response>(result8);
            Console.WriteLine(r8?.ErrorMessage == "Can't update finished tasks." ? "UpdateTaskDescriptionTest 98 Passed" : "UpdateTaskDescriptionTest 98 Failed");

            //Test 99: User not logged in
            serviceFactory.US.Logout(validEmail);
            string result9 = serviceFactory.TS.UpdateTaskDescription(validEmail, boardName, validColumnOrdinal, taskId0, validDescription);
            Response? r9 = JsonSerializer.Deserialize<Response>(result9);
            Console.WriteLine(r9?.ErrorMessage == "No such user." ? "UpdateTaskDescriptionTest 99 Passed" : "UpdateTaskDescriptionTest 99 Failed");

            

            //Delete this scope's data
            serviceFactory.DeleteData();

        }


        public void UpdateTaskTitleTests()
        {
            Setup();

            string validEmail = "valid@example.com";
            string validEmail2 = "valid2@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";
            string boardName = "Test Board";
            string validTitle = "Valid Title";
            string invalidTitle = ""; // Example of invalid title
            string invalidTitle2 = "123456789012345678901234567890123456789012345678901234567890";
            int validColumnOrdinal = 0;
            int invalidColumnOrdinal = 99;
            int taskId = -1; // Assuming a task with ID -1 exists
            DateTime dueDate = DateTime.Now.AddDays(5);

            // Register a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            string res2 = serviceFactory.BS.CreateBoard(validEmail, boardName);
            Response r2d = JsonSerializer.Deserialize<Response>(res2);
            int boardId = GetBoardIdFromResponse(r2d);
            string res = serviceFactory.BS.AddTask(validEmail, boardName, "Initial Title", "Initial Description", dueDate);
            Response r0 = JsonSerializer.Deserialize<Response>(res);
            int taskId0 = GetTaskIdFromResponse(r0);

            // Test 100: Valid Update Task Title
            serviceFactory.TS.AssignTask("", boardName, validColumnOrdinal, taskId0, validEmail);
            string result1 = serviceFactory.TS.UpdateTaskTitle(validEmail, boardName, validColumnOrdinal, taskId0, validTitle);
            Response? r1 = JsonSerializer.Deserialize<Response>(result1);
            taskSL task = ConvertReturnValue<taskSL>(r1?.ReturnValue);
            Console.WriteLine(task.Title.Equals(validTitle) ? "UpdateTaskTitleTest 100 Passed" : "UpdateTaskTitleTest 100 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, validPassword);
            string resultd14 = serviceFactory.BS.GetColumn(validEmail, boardName, 0);
            Response rd14 = JsonSerializer.Deserialize<Response>(resultd14);
            List<taskSL> list = ConvertReturnValue<List<taskSL>>(rd14?.ReturnValue);
            bool b = true;
            foreach (taskSL taskSL in list)
            {
                if (!taskSL.Title.Equals(validTitle))
                    b = false;
            }
            if (list.Count != 1)
                b = false;
            Console.WriteLine(b ? "UpdateTaskDueDateTest 101 Passed" : "UpdateTaskDueDateTest 101 Failed");

            // Test 102: Invalid User
            string result2 = serviceFactory.TS.UpdateTaskTitle(invalidEmail, boardName, validColumnOrdinal, taskId0, validTitle);
            Response? r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such user." ? "UpdateTaskTitleTest 102 Passed" : "UpdateTaskTitleTest 102 Failed");

            // Test 103: Invalid Board
            string result3 = serviceFactory.TS.UpdateTaskTitle(validEmail, "Invalid Board", validColumnOrdinal, taskId0, validTitle);
            Response? r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such board." ? "UpdateTaskTitleTest 103 Passed" : "UpdateTaskTitleTest 103 Failed");

            // Test 104: Invalid Column Ordinal
            string result4 = serviceFactory.TS.UpdateTaskTitle(validEmail, boardName, invalidColumnOrdinal, taskId0, validTitle);
            Response? r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "Invalid column ordinal." ? "UpdateTaskTitleTest 104 Passed" : "UpdateTaskTitleTest 104 Failed");

            // Test 105: Invalid Title
            string result5 = serviceFactory.TS.UpdateTaskTitle(validEmail, boardName, validColumnOrdinal, taskId0, invalidTitle);
            Response? r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "Invalid title." ? "UpdateTaskTitleTest 105 Passed" : "UpdateTaskTitleTest 105 Failed");

            //Test 106: Invalid Title 2nd test
            string result6 = serviceFactory.TS.UpdateTaskTitle(validEmail, boardName, validColumnOrdinal, taskId0, invalidTitle2);
            Response? r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Invalid title." ? "UpdateTaskTitleTest 106 Passed" : "UpdateTaskTitleTest 106 Failed");

            //Test 110: Change task by different assignee
            serviceFactory.US.Register(validEmail2, validPassword);
            serviceFactory.BS.JoinBoard(validEmail2, boardId);
            string result9 = serviceFactory.TS.UpdateTaskTitle(validEmail2, boardName, validColumnOrdinal, taskId0, validTitle);
            Response? r9 = JsonSerializer.Deserialize<Response>(result9);
            Console.WriteLine(r9?.ErrorMessage == "Not the correct Assignee." ? "UpdateTaskTitleTest 110 Passed" : "UpdateTaskTitleTest 110 Failed");

            //Test 107: Update task in "done" column
            serviceFactory.BS.AdvanceTask(validEmail, boardName, 0, taskId0);
            serviceFactory.BS.AdvanceTask(validEmail, boardName, 1, taskId0);
            string result10 = serviceFactory.TS.UpdateTaskTitle(validEmail, boardName, 2, taskId0, validTitle);
            Response? r10 = JsonSerializer.Deserialize<Response>(result10);
            Console.WriteLine(r10?.ErrorMessage == "Can't update finished tasks." ? "UpdateTaskTitleTest 107 Passed" : "UpdateTaskTitleTest 107 Failed");

            //Test 108: User not logged in
            serviceFactory.US.Logout(validEmail);
            string result7 = serviceFactory.TS.UpdateTaskTitle(validEmail, boardName, validColumnOrdinal, taskId0, validTitle);
            Response? r7 = JsonSerializer.Deserialize<Response>(result7);
            Console.WriteLine(r7?.ErrorMessage == "No such user." ? "UpdateTaskTitleTest 108 Passed" : "UpdateTaskTitleTest 108 Failed");

            //Test 109: User not registered
            string result8 = serviceFactory.TS.UpdateTaskTitle("blabla", boardName, validColumnOrdinal, taskId0, validTitle);
            Response? r8 = JsonSerializer.Deserialize<Response>(result8);
            Console.WriteLine(r8?.ErrorMessage == "No such user." ? "UpdateTaskTitleTest 109 Passed" : "UpdateTaskTitleTest 109 Failed");

            

            //Delete this scope's data
            serviceFactory.DeleteData();

        }

        public void GetUserBoardsTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";
            string boardName1 = "Test Board1";
            string boardName2 = "Test Board2";
            string boardName3 = "Test Board3";


            // Register a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response r = JsonSerializer.Deserialize<Response>(res);
            int BoardId = GetBoardIdFromResponse(r);

            //Test 111: One board to get
            string result1 = serviceFactory.BS.GetUserBoards(validEmail);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            List<int> list = ConvertReturnValue<List<int>>(r1?.ReturnValue);
            bool b = true;
            foreach (int i in list)
            {
                if (i != BoardId)
                    b = false;
            }
            if (list.Count != 1)
                b = false;
            Console.WriteLine(b ? "GetUserBoardsTest 111 Passed" : "GetUserBoardsTest 111 Failed");

            //Test 112: Multiple Boards to get
            string res2 = serviceFactory.BS.CreateBoard(validEmail, boardName2);
            string res3 = serviceFactory.BS.CreateBoard(validEmail, boardName3);
            Response r2 = JsonSerializer.Deserialize<Response>(res2);
            int BoardId2 = GetBoardIdFromResponse(r2);
            Response r3 = JsonSerializer.Deserialize<Response>(res3);
            int BoardId3 = GetBoardIdFromResponse(r3);
            string result2 = serviceFactory.BS.GetUserBoards(validEmail);
            Response r4 = JsonSerializer.Deserialize<Response>(result2);
            List<int> list2 = ConvertReturnValue<List<int>>(r4?.ReturnValue);
            bool d = true;
            foreach (int i in list2)
            {
                if (i != BoardId && i != BoardId2 && i != BoardId3)
                    d = false;
            }
            if (list2.Count != 3)
                d = false;
            Console.WriteLine(d ? "GetUserBoardsTest 112 Passed" : "GetUserBoardsTest 112 Failed");

            //Test 113: Invalid UserEmail
            string result3 = serviceFactory.BS.GetUserBoards(invalidEmail);
            Response r5 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r5?.ErrorMessage == "No such user." ? "GetUserBoardsTest 113 Passed" : "GetUserBoardsTest 113 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, validPassword);
            string result4 = serviceFactory.BS.GetUserBoards(validEmail);
            Response r6 = JsonSerializer.Deserialize<Response>(result4);
            List<int> list3 = ConvertReturnValue<List<int>>(r6?.ReturnValue);
            bool temp = true;
            foreach (int i in list3)
            {
                if (i != BoardId && i != BoardId2 && i != BoardId3)
                    temp = false;
            }
            if (list3.Count != 3)
                temp = false;
            Console.WriteLine(temp ? "GetUserBoardsTest 114 Passed" : "GetUserBoardsTest 114 Failed");

            //Test 115: Empty Board list
            serviceFactory.BS.DeleteBoard(validEmail, boardName1);
            serviceFactory.BS.DeleteBoard(validEmail, boardName2);
            serviceFactory.BS.DeleteBoard(validEmail, boardName3);
            string result5 = serviceFactory.BS.GetUserBoards(validEmail);
            Response r7 = JsonSerializer.Deserialize<Response>(result5);
            List<int> list4 = ConvertReturnValue<List<int>>(r7?.ReturnValue);
            Console.WriteLine(list4.Count == 0 ? "GetUserBoardsTest 115 Passed" : "GetUserBoardsTest 115 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();

        }
        public void JoinBoardTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";
            string boardName1 = "Test Board1";
            int invalidBoardId = -10;

            // Register a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response r = JsonSerializer.Deserialize<Response>(res);
            int BoardId = GetBoardIdFromResponse(r);
            serviceFactory.BS.LeaveBoard(validEmail, BoardId);

            //Test 116: Successful join to a board
            serviceFactory.BS.JoinBoard(validEmail, BoardId);
            string result1 = serviceFactory.BS.GetUserBoards(validEmail);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            List<int> list = ConvertReturnValue<List<int>>(r1?.ReturnValue);
            bool b = true;
            foreach (int i in list)
            {
                if (i != BoardId)
                    b = false;
            }
            if (list.Count == 0)
                b = false;
            Console.WriteLine(b ? "JoinBoardTest 116 Passed" : "JoinBoardTest 116 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, validPassword);
            string result2 = serviceFactory.BS.GetUserBoards(validEmail);
            Response r2 = JsonSerializer.Deserialize<Response>(result2);
            List<int> list2 = ConvertReturnValue<List<int>>(r2?.ReturnValue);
            bool b2 = true;
            foreach (int i in list2)
            {
                if (i != BoardId)
                    b2 = false;
            }
            if (list2.Count == 0)
                b2 = false;
            Console.WriteLine(b2 ? "JoinBoardTest 117 Passed" : "JoinBoardTest 117 Failed");

            //Test 118: InvalidEmail 
            string result3 = serviceFactory.BS.JoinBoard(invalidEmail, BoardId);
            Response r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such user." ? "JoinBoardTest 118 Passed" : "JoinBoardTest 118 Failed");

            //Test 119: Non existing board ID
            string result4 = serviceFactory.BS.JoinBoard(validEmail, invalidBoardId);
            Response r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "No such board." ? "JoinBoardTest 119 Passed" : "JoinBoardTest 119 Failed");

            //Test 120: Already joined to the board
            string result5 = serviceFactory.BS.JoinBoard(validEmail, BoardId);
            Response r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "Already joined to the board." ? "JoinBoardTest 120 Passed" : "JoinBoardTest 120 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
        }

        public void LeaveBoardTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string validEmail2 = "valid2@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";
            string boardName1 = "Test Board1";
            string boardName2 = "Test Board2";
            int invalidBoardId = -10;

            // Register a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response r = JsonSerializer.Deserialize<Response>(res);
            int BoardId = GetBoardIdFromResponse(r);
            serviceFactory.US.Register(validEmail2, validPassword);
            serviceFactory.BS.JoinBoard(validEmail2, BoardId);

            //Test 121: Successfully leaving the board
            serviceFactory.BS.LeaveBoard(validEmail2, BoardId);
            string result1 = serviceFactory.BS.GetUserBoards(validEmail2);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            List<int> list = ConvertReturnValue<List<int>>(r1?.ReturnValue);
            bool b = true;
            if (list.Count != 0)
                b = false;
            Console.WriteLine(b ? "LeaveBoardTest 121 Passed" : "LeaveBoardTest 121 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail2, validPassword);
            string result2 = serviceFactory.BS.GetUserBoards(validEmail2);
            Response r3 = JsonSerializer.Deserialize<Response>(result2);
            List<int> list2 = ConvertReturnValue<List<int>>(r3?.ReturnValue);
            bool b2 = true;
            if (list2.Count != 0)
                b2 = false;
            Console.WriteLine(b2 ? "LeaveBoardTest 122 Passed" : "LeaveBoardTest 122 Failed");

            serviceFactory.US.Login(validEmail, validPassword);
            //Test 123: InvalidEmail 
            string result3 = serviceFactory.BS.LeaveBoard(invalidEmail, BoardId);
            Response r4 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r4?.ErrorMessage == "No such user." ? "LeaveBoardTest 123 Passed" : "LeaveBoardTest 123 Failed");

            //Test 124: Non existing board ID
            string result4 = serviceFactory.BS.LeaveBoard(validEmail, invalidBoardId);
            Response r5 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r5?.ErrorMessage == "No such board." ? "LeaveBoardTest 124 Passed" : "LeaveBoardTest 124 Failed");

            //Test 125: Owner tries leaving the board
            string result5 = serviceFactory.BS.LeaveBoard(validEmail, BoardId);
            Response r6 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r6?.ErrorMessage == "Owner can't leave." ? "LeaveBoardTest 125 Passed" : "LeaveBoardTest 125 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();

        }

        public void AssignTaskTest()
        {
            Setup();
            string validEmail = "valid@example.com";
            string invalidEmail = "invalid@example.com";
            string validPassword = "Valid1Password";
            string boardName1 = "Test Board1";
            string title = "Sample Task";
            string description = "Sample Task Description";
            string boardName2 = "TestBoard2";
            DateTime dueDate = DateTime.Now.AddDays(7);


            // Register a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response r = JsonSerializer.Deserialize<Response>(res);
            string t = serviceFactory.BS.AddTask(validEmail, boardName1, title, description, dueDate);
            Response res1 = JsonSerializer.Deserialize<Response>(t);
            int taskId = GetTaskIdFromResponse(res1);

            //Test 126: Successfully assigned task
            string result1 = serviceFactory.TS.AssignTask("", boardName1, 0, taskId, validEmail);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            taskSL task2 = ConvertReturnValue<taskSL>(r1?.ReturnValue);
            Console.WriteLine(task2.Assignee == validEmail ? "AssignTaskTest 126 Passed" : "AssignTaskTest 126 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail, validPassword);
            string c = serviceFactory.BS.GetColumn(validEmail, boardName1, 0);
            Response r2 = JsonSerializer.Deserialize<Response>(c);
            List<taskSL> list = ConvertReturnValue<List<taskSL>>(r2?.ReturnValue);
            bool b = true;
            foreach (taskSL task in list)
            {
                if (task.Assignee != validEmail)
                    b = false;
            }
            Console.WriteLine(b ? "AssignTaskTest 127 Passed" : "AssignTaskTest 127 Failed");


            // Test 128: Invalid user email
            string result4 = serviceFactory.TS.AssignTask(validEmail, boardName1, 0, taskId, invalidEmail);
            Response r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "No such user." ? "AssignTaskTest 128 Passed" : "AssignTaskTest 128 Failed");

            //Test 129: Invalid board name 
            string result5 = serviceFactory.TS.AssignTask(validEmail, boardName2, 0, taskId, validEmail);
            Response r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "No such board." ? "AssignTaskTest 129 Passed" : "AssignTaskTest 129 Failed");

            //Test 130: Invalid column ordinal
            string result6 = serviceFactory.TS.AssignTask(validEmail, boardName1, 99, taskId, validEmail);
            Response r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Invalid column ordinal." ? "AssignTaskTest 130 Passed" : "AssignTaskTest 130 Failed");

            //Test 131: Invalid task ID
            string result7 = serviceFactory.TS.AssignTask(validEmail, boardName1, 0, 88, validEmail);
            Response r7 = JsonSerializer.Deserialize<Response>(result7);
            Console.WriteLine(r7?.ErrorMessage == "No such task." ? "AssignTaskTest 131 Passed" : "AssignTaskTest 131 Failed");

            //Test 132: Invalid assignee 
            string result8 = serviceFactory.TS.AssignTask("test", boardName1, 0, taskId, validEmail);
            Response r8 = JsonSerializer.Deserialize<Response>(result8);
            Console.WriteLine(r8?.ErrorMessage == "No such user." ? "AssignTaskTest 132 Passed" : "AssignTaskTest 132 Failed");

            //Test 133: AssignTask to yourself
            //string result9 = serviceFactory.TS.AssignTask(validEmail, boardName1, 0, taskId, validEmail);
            //Response r9 = JsonSerializer.Deserialize<Response>(result9);
            //Console.WriteLine(r9?.ErrorMessage == "Already the assignee." ? "AssignTaskTest 133 Passed" : "AssignTaskTest 133 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();

        }

        public void GetBoardNameTest()
        {
            Setup();
            string validEmail = "valid@example.com";
            string validPassword = "Valid1Password";
            string boardName1 = "Test Board1";

            // Register a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response r = JsonSerializer.Deserialize<Response>(res);
            int BoardId = GetBoardIdFromResponse(r);

            //Test 134: Successfully got board name
            string result1 = serviceFactory.BS.GetBoardName(BoardId);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            string name = ConvertReturnValue<string>(r1?.ReturnValue);
            Console.WriteLine(name == boardName1 ? "GetBoardNameTest 134 Passed" : "GetBoardNameTest 134 Failed");

            //Test 135: Invalid boardId
            string result2 = serviceFactory.BS.GetBoardName(10);
            Response r2 = JsonSerializer.Deserialize<Response>(result2);
            Console.WriteLine(r2?.ErrorMessage == "No such board." ? "GetBoardNameTest 135 Passed" : "GetBoardNameTest 135 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();

        }

        public void TransferOwnershipTest()
        {
            Setup();

            string validEmail = "valid@example.com";
            string validEmail2 = "valid2@example.com";
            string validPassword = "Valid1Password";
            string validPassword2 = "Valid2Password";
            string boardName1 = "Test Board1";

            // Register a user for testing
            serviceFactory.US.Register(validEmail, validPassword);
            serviceFactory.US.Register(validEmail2, validPassword2);
            string res = serviceFactory.BS.CreateBoard(validEmail, boardName1);
            Response r = JsonSerializer.Deserialize<Response>(res);
            int BoardId = GetBoardIdFromResponse(r);
            serviceFactory.BS.JoinBoard(validEmail2, BoardId);

            //Test 136: successfully transfered ownership
            string result1 = serviceFactory.BS.TransferOwnership(validEmail, validEmail2, boardName1);
            Response r1 = JsonSerializer.Deserialize<Response>(result1);
            BoardSL b = ConvertReturnValue<BoardSL>(r1?.ReturnValue);
            Console.WriteLine(b.boardOwner == validEmail2 ? "TransferOwnershipTest 136 Passed" : "TransferOwnershipTest 136 Failed");

            //Database Test:
            Setup();
            serviceFactory.LoadData();
            serviceFactory.US.Login(validEmail2, validPassword2);
            serviceFactory.BS.DeleteBoard(validEmail2, boardName1);
            string result2 = serviceFactory.BS.GetUserBoards(validEmail2);
            Response r2 = JsonSerializer.Deserialize<Response>(result2);
            List<int> list = ConvertReturnValue<List<int>>(r2?.ReturnValue);
            Console.WriteLine(list.Count == 0 ? "TransferOwnershipTest 137 Passed" : "TransferOwnershipTest 137 Failed");

            //Test 138: Invalid owner email
            string result3 = serviceFactory.BS.TransferOwnership(validEmail, validEmail, boardName1);
            Response r3 = JsonSerializer.Deserialize<Response>(result3);
            Console.WriteLine(r3?.ErrorMessage == "No such user." ? "TransferOwnershipTest 138 Passed" : "TransferOwnershipTest 138 Failed");

            //Test 139: Invalid new owner email
            string result4 = serviceFactory.BS.TransferOwnership(validEmail2, "email", boardName1);
            Response r4 = JsonSerializer.Deserialize<Response>(result4);
            Console.WriteLine(r4?.ErrorMessage == "No such user." ? "TransferOwnershipTest 139 Passed" : "TransferOwnershipTest 139 Failed");

            //Test 140: Invalid board name
            string result5 = serviceFactory.BS.TransferOwnership(validEmail2, validEmail, "board");
            Response r5 = JsonSerializer.Deserialize<Response>(result5);
            Console.WriteLine(r5?.ErrorMessage == "No such user." ? "TransferOwnershipTest 140 Passed" : "TransferOwnershipTest 140 Failed");

            //Test 141: Already the owner
            serviceFactory.BS.CreateBoard(validEmail2, boardName1);
            string result6 = serviceFactory.BS.TransferOwnership(validEmail2, validEmail2, boardName1);
            Response r6 = JsonSerializer.Deserialize<Response>(result6);
            Console.WriteLine(r6?.ErrorMessage == "Already the owner." ? "TransferOwnershipTest 141 Passed" : "TransferOwnershipTest 141 Failed");

            //Delete this scope's data
            serviceFactory.DeleteData();
            serviceFactory.DeleteData();
        }


        public static void Main(string[] args)
        {
            main test = new main();
            log4net.Config.XmlConfigurator.Configure();

            test.AddTaskTest();

            test.CreateBoardTest();
            test.DeleteBoardTest();
            test.LimitColumnTest();
            
            test.AdvanceTaskTest();
            test.InProgressTasksTest();
            test.GetColumnLimitTest();
            test.GetColumnNameTest();
            test.GetColumnTest();
            test.RegisterTests();
            test.LoginTests();
            test.LogoutTests();
            test.UpdateTaskDueDateTests();
            test.UpdateTaskDescriptionTests();
            test.UpdateTaskTitleTests();
            test.GetUserBoardsTest();
            test.JoinBoardTest();
            test.LeaveBoardTest();
            test.AssignTaskTest();
            test.GetBoardNameTest();
            test.TransferOwnershipTest();

        }
    }
}

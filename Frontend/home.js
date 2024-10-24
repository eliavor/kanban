const list = document.getElementById('list');
const boards = document.getElementById('boards');
const boardsById = {};

const email = localStorage.getItem('email')
const jwt = localStorage.getItem('jwt')

console.log(encodeURIComponent(email));
console.log(encodeURIComponent(jwt));

fetch(`http://localhost:5208/Board/getUserBoards?JWT=${encodeURIComponent(jwt)}&email=${encodeURIComponent(email)}`)
.then(res => res.json())
.then(data => {
    presentData(data);
    console.log(data);
});


// Main function to present the data
function presentData(data) {
    if (data.ErrorMessage == null) {
        data.ReturnValue.forEach(element => {
            createBoardItem(element);
        });
    }
    else if(data.ErrorMessage == "Invalid token."){
        alert(data.ErrorMessage);
    }
}

// Function to create the <li> board item
function createBoardItem(element) {
    let li = document.createElement("li");
    li.textContent = element.Item2;
    li.id = `board#${element.Item1}`;
    boardsById[element.Item1] = element.Item2;

    let addTaskBtn = createAddTaskButton(li, element.Item1);
    li.appendChild(addTaskBtn);

    let taskForm = createTaskForm(element.Item1);
    li.appendChild(taskForm);

    let showTasks = createShowTasksButton(element.Item1);
    li.appendChild(showTasks);

    boards.appendChild(li);
}

// Function to create the 'Add Task' button
function createAddTaskButton(li, boardId) {
    let addTaskBtn = document.createElement("button");
    addTaskBtn.textContent = "Add Task";
    
    addTaskBtn.addEventListener('click', () => {
        let taskForm = li.querySelector("form");
        taskForm.style.display = taskForm.style.display === "none" ? "block" : "none";
    });

    return addTaskBtn;
}

// Function to create the task form
function createTaskForm(boardId) {
    let taskForm = document.createElement("form");
    taskForm.style.display = "none";  // Hide the form initially

    let titleInput = createInputField("text", "Title");
    let descriptionInput = createTextArea("Description");
    let dueDateInput = createInputField("datetime-local", "");

    let submitBtn = document.createElement("button");
    submitBtn.type = "submit";
    submitBtn.textContent = "Submit";

    taskForm.appendChild(titleInput);
    taskForm.appendChild(descriptionInput);
    taskForm.appendChild(dueDateInput);
    taskForm.appendChild(submitBtn);

    taskForm.addEventListener('submit', (event) => {
        handleFormSubmit(event, taskForm, boardId, titleInput, descriptionInput, dueDateInput);
    });

    return taskForm;
}

// Function to create an input field
function createInputField(type, placeholder) {
    let input = document.createElement("input");
    input.type = type;
    input.placeholder = placeholder;
    return input;
}

// Function to create a text area
function createTextArea(placeholder) {
    let textarea = document.createElement("textarea");
    textarea.placeholder = placeholder;
    return textarea;
}

// Handle the task form submission
function handleFormSubmit(event, form, boardId, titleInput, descriptionInput, dueDateInput) {
    event.preventDefault();

    let title = titleInput.value;
    let description = descriptionInput.value;
    let dueDate = new Date(dueDateInput.value).toISOString();

    console.log(`Task created for board ${boardId}:`);
    console.log(`Title: ${title}`);
    console.log(`Description: ${description}`);
    console.log(`Due Date: ${dueDate}`);

    data = {
        email:email,
        jwt:jwt,
        boardName:boardsById[boardId],
        title:title,
        description:description,
        dueDate:dueDate
    }

    console.log(data);
    fetch('http://localhost:5208/Board/addTask',{
        method:'POST',
        headers:{
            'Content-Type':'application/json'
        },
        body:JSON.stringify(data)
    })
    .then(boardClicked(boardId))
    .catch(error => console.error(error));

    form.style.display = "none";  // Hide form after submission
    titleInput.value = "";        // Clear form
    descriptionInput.value = "";
    dueDateInput.value = "";
}

// Function to create the 'Show Tasks' button
function createShowTasksButton(boardId) {
    let showTasks = document.createElement("button");
    showTasks.textContent = "Show Tasks";
    showTasks.addEventListener('click', () => boardClicked(boardId));
    return showTasks;
}

function boardClicked(boardID){
    const UL = document.getElementById(`list#${boardID}`);
    if(UL == null){
        fetch(`http://localhost:5208/Board/getBoardTasks?JWT=${encodeURIComponent(jwt)}&boardID=${boardID}`)
        .then(res => res.json())
        .then(data => createDropDown(data, boardID))
        .catch(error => console.log(error));
    }
    else{
        UL.remove();
    }
}


function createDropDown(data, boardID){
    let board = document.getElementById(`board#${boardID}`);
    let oldUL = document.getElementById(`list#${boardID}`);
    if(oldUL != null) oldUL.remove();
    if(data.ErrorMessage == null){
        let innerUL = document.createElement('ul');
        innerUL.id = `list#${boardID}`;
        data.ReturnValue.forEach(task =>{
            let arr = task.split(';');
            let li = document.createElement("li");
            li.innerHTML = `<h4>${arr[1]}: </h4> ${arr[2]}`;
            li.id = `${boardID}:${arr[0]}`;

            innerUL.appendChild(li);
        })
        board.appendChild(innerUL);
    }
    else if(data.ErrorMessage == "Invalid token."){
        alert(data.ErrorMessage);
    }
}
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


function presentData(data){
    if(data.ErrorMessage == null){
        data.ReturnValue.forEach(element => {
            let li = document.createElement("li");
            li.textContent = element.Item2;
            boardsById[element.Item2] = element.Item1;
            boards.appendChild(li);

            li.addEventListener('click', () => boardClicked(element.Item1))
        });
    }
}

function boardClicked(boardID){
    console.log(boardID);
    fetch(`http://localhost:5208/Board/getBoardTasks?JWT=${encodeURIComponent(jwt)}&boardID=${boardID}`)
    .then(res => {
        console.log("trying to log")
        
        return res.json();
    })
    .then(data => console.log("hello"))
    .catch(error => console.log("error"));
}
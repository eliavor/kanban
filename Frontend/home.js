const list = document.getElementById('list');

fetch('http://localhost:5208/User/login',{
    method:'POST',
    headers:{
        'Content-Type':'application/json'
    },
    body:JSON.stringify(data)
})
.then(res => res.json())
.then(data => presentData(data));


function presentData(data){
    list.innerHTML = `<p> ${data} </p>`
}
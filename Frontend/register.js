const emailInput = document.querySelector('#email_input')
const passwordInput = document.querySelector('#password_input')
const submit = document.querySelector('#register-form');
const msg = document.querySelector('#msg');

submit.addEventListener('submit', OnSubmit);

function OnSubmit(e){
    e.preventDefault();

    if(emailInput.value === '' || passwordInput.value === ''){
        msg.innerHTML = '<label id="msg">Please make sure to fill all fields.</label>';
    }
    else{
        msg.innerHTML = '<label id="msg"></label>';
        console.log(`email is:  + ${emailInput.value}`);
        console.log(`password is:  ${passwordInput.value}`);
        data = {
            email:emailInput.value,
            password:passwordInput.value
        };
        jsonData = JSON.stringify(data)
        console.log(jsonData)

        fetch('http://localhost:5208/User/register',{
            method:'POST',
            headers:{
                'Content-Type':'application/json'
            },
            body:JSON.stringify(data)
        })
        .then(res => res.json())
        .then(data => keepToken(data));

        window.location.href = "home.html";
    }
}

function keepToken(data){
    const jwt = data.ReturnValue.JWT;
    localStorage.setItem('jwt', jwt);
}
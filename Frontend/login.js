const emailInput = document.querySelector('#email_input')
const passwordInput = document.querySelector('#password_input')
const submit = document.querySelector('#login-form');
const msg = document.querySelector('#msg');

submit.addEventListener('submit', OnSubmit);

function OnSubmit(e){
    e.preventDefault();

    if(emailInput.value === '' || passwordInput.value === ''){
        msg.innerHTML = '<label id="msg">Please make sure to fill all fields.</label>';
        setTimeout(() => msg.innerHTML = "", 3000)
    }
    else{
        localStorage.clear()
        msg.innerHTML = '<label id="msg"></label>';
        console.log(`email is:  + ${emailInput.value}`);
        console.log(`password is:  ${passwordInput.value}`);
        data = {
            email:emailInput.value,
            password:passwordInput.value
        };
        jsonData = JSON.stringify(data)
        console.log(jsonData)

        fetch('http://localhost:5208/User/login',{
            method:'POST',
            headers:{
                'Content-Type':'application/json'
            },
            body:JSON.stringify(data)
        })
        .then(res => res.json())
        .then(data => keepToken(data)).catch(error => console.error(error));

        localStorage.setItem('email', emailInput.value);

        
    }
}

function keepToken(data){
    if(data.ErrorMessage == null){
        const jwt = data.ReturnValue.JWT;
        console.log(jwt);
        localStorage.setItem('jwt', jwt);
        window.location.href = "home.html";
    }
    else{
        console.log(data.ErrorMessage);
        localStorage.setItem('error',data);
    }
}
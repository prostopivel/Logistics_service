function encryptPassword(event, request, uri) {
    event.preventDefault();

    var email = document.getElementById('Email').value;
    var password = document.getElementById('PasswordHash').value;
    var realm = $('meta[name="realm"]').attr('content');

    var encryptedPassword = md5(email + ':' + realm + ':' + password);

    document.getElementById('PasswordHash').value = encryptedPassword;

    var user = {
        Name: document.querySelector('input[name="Name"]').value,
        Role: document.querySelector('select[name="Role"]').value,
        Email: email,
        PasswordHash: encryptedPassword
    };

    auth(request, uri, JSON.stringify(user));
}

function createOrder(event, request, uri) {
    event.preventDefault();

    var order = {
        BeginningAddress: document.querySelector('input[name="BeginningAddress"]').value,
        DestinationAddress: document.querySelector('input[name="DestinationAddress"]').value,
        ArrivalTime: document.querySelector('input[name="ArrivalTime"]').value,
        WeightCargo: document.querySelector('input[name="WeightCargo"]').value
    };

    auth(request, uri, JSON.stringify(order));
}

function ChangeWidth(newWidth) {
    console.log(newWidth);
    const container = document.querySelector('.container');
    if (container) {
        container.style.maxWidth = newWidth;
    } else {
        console.error('Элемент с классом .container не найден');
    }
}

function sendOrder(request, uri, data) {
    auth(request, uri, JSON.stringify(data));
}

function viewMap(request, uri) {
    ChangeWidth('1400px');
    auth(request, uri);

    setTimeout(() => {
        if (document.getElementById('imageContainer')) {
            fillModel();
        }
    }, 1000);
}

function viewMapLine(request, uri) {
    ChangeWidth('1400px');

    var dataSE = {
        Start: document.getElementById('startNum').value,
        End: document.getElementById('endNum').value
    };

    auth(request, uri, JSON.stringify(dataSE));

    setTimeout(() => {
        if (document.getElementById('imageContainer')) {
            fillModelLine();
        }
    }, 1000);
}

function fillModel() {
    var modelElement = document.getElementById('model');
    if (modelElement) {
        var model = JSON.parse(modelElement.textContent);
        FillButtons(model);
    } else {
        console.error('Элемент с id="model" не найден');
    }
}

function fillModelLine() {
    fillModel();

    var modelElementLine = document.getElementById('modelLine');
    if (modelElementLine) {
        var modelLine = JSON.parse(modelElementLine.textContent);
        FillButtons(modelLine, 'yellow');
    } else {
        console.error('Элемент с id="modelLine" не найден');
    }
}
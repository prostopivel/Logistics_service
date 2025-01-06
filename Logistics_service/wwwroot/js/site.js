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
    };

    auth(request, uri, JSON.stringify(order));
}

function assignOrder(event, request, uri) {
    event.preventDefault();

    var order = {
        StartPointId: parseInt(document.querySelector('input[name="StartPointId"]').value, 10),
        EndPointId: parseInt(document.querySelector('input[name="EndPointId"]').value, 10),
        VehicleId: document.querySelector('select[name="VehicleId"]').value,
        ArrivalTime: document.querySelector('input[name="ArrivalTime"]').value,
        CustomerEmail: document.getElementById('CustomerEmail').textContent
    };

    auth(request, uri, JSON.stringify(order));
}

function addTransport(event, request, uri) {
    event.preventDefault();

    var vehicle = {
        Speed: document.querySelector('input[name="Speed"]').value,
        GarageId: document.querySelector('input[name="GarageId"]').value
    };

    auth(request, uri, JSON.stringify(vehicle));
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

function sendReasonOrder(request, uri, data) {
    let reason = document.querySelector('#reason').value;

    reason = reason.replace(/\s+/g, '_');

    data.Reason = reason;

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
        else {
            setTimeout(() => {
                if (document.getElementById('imageContainer')) {
                    fillModelLine();
                }
            }, 1000);
        }
    }, 1000);
}

function viewMapRedLine(request, uri) {
    ChangeWidth('1400px');

    auth(request, uri);

    setTimeout(() => {
        if (document.getElementById('imageContainer')) {
            fillModelRedLine();
        }
        else {
            setTimeout(() => {
                if (document.getElementById('imageContainer')) {
                    fillModelRedLine();
                }
            }, 1000);
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

    var modelElementLine1 = document.getElementById('modelLine');
    var modelElementLine2 = document.getElementById('modelRedLine');
    var modelElementLine3 = document.getElementById('modelAddLine');
    if (modelElementLine1 && modelElementLine2 && modelElementLine3) {
        var modelRedLine1 = JSON.parse(modelElementLine1.textContent);
        var modelRedLine2 = JSON.parse(modelElementLine2.textContent);
        var modelRedLine3 = JSON.parse(modelElementLine3.textContent);
        FillColorButtons(modelRedLine1, modelRedLine2, modelRedLine3, 'yellow', 'red', 'blue');
    } else if (modelElementLine1 && modelElementLine2) {
        var modelRedLine1 = JSON.parse(modelElementLine1.textContent);
        var modelRedLine2 = JSON.parse(modelElementLine2.textContent);
        FillColorButtons(modelRedLine1, modelRedLine2, null, 'yellow', 'red', 'green');
    }
    else {
        console.error('Элемент с id="modelLine" или id="modelRedLine" не найден');
    }
}

function fillModelRedLine() {
    fillModel();

    var modelElementLine1 = document.getElementById('modelLine');
    var modelElementLine2 = document.getElementById('modelRedLine');
    if (modelElementLine1 && modelElementLine2) {
        var modelRedLine1 = JSON.parse(modelElementLine1.textContent);
        var modelRedLine2 = JSON.parse(modelElementLine2.textContent);
        FillColorButtons(modelRedLine1, modelRedLine2, null, 'yellow', 'red', 'green');
    } else {
        console.error('Элемент с id="modelLine" или id="modelRedLine" не найден');
    }
}
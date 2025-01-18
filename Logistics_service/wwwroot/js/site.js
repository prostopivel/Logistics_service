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
        Id: 0,
        StartPointId: parseInt(document.querySelector('input[name="StartPointId"]').value, 10),
        EndPointId: parseInt(document.querySelector('input[name="EndPointId"]').value, 10),
        VehicleId: document.querySelector('select[name="VehicleId"]').value,
        ArrivalTime: document.querySelector('input[name="ArrivalTime"]').value,
        CustomerEmail: document.getElementById('CustomerEmail').textContent
    };

    auth(request, uri, JSON.stringify(order));
}

function assignAdminOrder(event, request, uri, id) {
    event.preventDefault();

    var order = {
        Id: parseInt(id),
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

function sendRouteOrder(request, uri, date) {
    let fullUri = `${uri}/${date}`;

    auth(request, fullUri);
}

function sendRouteOrderLine(request, uri, date, data) {
    let fullUri = `${uri}/${date}`;

    auth(request, fullUri, data);
}

function sendReasonOrder(request, uri, data) {
    let reason = document.querySelector('#reason').value;

    reason = reason.replace(/\s+/g, '_');

    data.Reason = reason;

    auth(request, uri, JSON.stringify(data));
}

function sendDate(request, uri) {
    if (document.querySelector('#dateSet')) {
        let date = document.querySelector('#dateSet').value;

        sendRouteOrder(request, uri, date);
    }
    else {
        auth(request, uri);
    }
}

function sendDateLine(request, uri, data) {
    if (document.querySelector('#dateSet')) {
        let date = document.querySelector('#dateSet').value;

        sendRouteOrderLine(request, uri, date, data);
    }
    else {
        auth(request, uri);
    }
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

    var line = {
        Start: document.getElementById('startNum').value,
        End: document.getElementById('endNum').value
    };

    sendDateLine(request, uri, JSON.stringify(line));

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

    sendDate(request, uri);

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
    if (modelElementLine1) {
        var modelRedLine1 = JSON.parse(modelElementLine1.textContent);
        FillColorButtons(modelRedLine1, 'yellow');
    }
    if (modelElementLine2) {
        var modelRedLine2 = JSON.parse(modelElementLine2.textContent);
        FillColorButtons(modelRedLine2, 'red');
    }
    if (modelElementLine3) {
        var modelRedLine3 = JSON.parse(modelElementLine3.textContent);
        FillColorButtons(modelRedLine3, 'blue');
    }
}

function fillModelRedLine() {
    fillModel();

    var modelElementLine1 = document.getElementById('modelLine');
    var modelElementLine2 = document.getElementById('modelRedLine');
    if (modelElementLine1) {
        var modelRedLine1 = JSON.parse(modelElementLine1.textContent);
        FillColorButtons(modelRedLine1, 'yellow');
    }
    if (modelElementLine2) {
        var modelRedLine2 = JSON.parse(modelElementLine2.textContent);
        FillColorButtons(modelRedLine2, 'red');
    }
}
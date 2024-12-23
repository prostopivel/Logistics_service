let isFunctionCalled = false;

document.addEventListener('DOMContentLoaded', function () {
    if (!isFunctionCalled) {
        authFirst();
        isFunctionCalled = true;
    }
});

function auth(request, uri, data = '') {
    if (auth.xhrGetParams) {
        auth.xhrGetParams.abort();
    }
    if (auth.xhrSetParams) {
        auth.xhrSetParams.abort();
    }

    var xhrGetParams = new XMLHttpRequest();
    xhrGetParams.open(request, uri, true);
    xhrGetParams.onreadystatechange = function () {
        if (xhrGetParams.readyState === 4) {
            if (xhrGetParams.status === 401) {
                var responseObject = JSON.parse(xhrGetParams.responseText);

                var email = sessionStorage.getItem('UserName');
                var password = sessionStorage.getItem('Password');

                var realm = responseObject.realm;
                var qop = responseObject.qop;
                var nonce = responseObject.nonce;
                var opaque = responseObject.opaque;

                var nc = '00000001';
                var cnonce = generateCnonce();

                var role = sessionStorage.getItem('Role');

                var A1 = email + ':' + realm + ':' + password;
                var A2 = 'POST:' + uri;

                var HA1 = md5(A1);
                var HA2 = md5(A2);

                var response = md5(HA1 + ':' + nonce + ':' + nc + ':' + cnonce + ':' + qop + ':' + HA2);

                var digest = 'username="' + email + '", realm="' + realm + '", nonce="' + nonce + '", uri="' + uri + '", qop=' + qop + ', nc=' + nc + ', cnonce="' + cnonce + '", response="' + response + '", opaque="' + opaque + '", role="' + role + '"';

                var xhrSetParams = new XMLHttpRequest();
                xhrSetParams.open(request, uri, true);
                xhrSetParams.setRequestHeader('Content-Type', 'application/json');
                xhrSetParams.setRequestHeader('Authorization', 'Digest ' + digest);
                xhrSetParams.onreadystatechange = function () {

                    if (xhrSetParams.status === 200) {
                        if (xhrSetParams.readyState === 4) {
                            var container = document.getElementById('containerBox');
                            if (container) {
                                container.innerHTML = xhrSetParams.responseText;
                            } else {
                                console.error('Элемент с id="containerBox" не найден');
                            }

                            var role = sessionStorage.getItem('Role');
                            var dashboardLink = document.getElementById('dashdoardReturn');
                            if (dashboardLink) {
                                dashboardLink.onclick = function (event) {
                                    event.preventDefault();
                                    auth('GET', '/dashboard/' + role);

                                    ChangeWidth('1000px');
                                };
                            }
                        }
                    } else {
                        console.error('Ошибка: ' + xhrSetParams.statusText);
                    }
                };
                xhrSetParams.send(data);

            } else if (xhrGetParams.status === 200) {

                var container = document.getElementById('containerBox');
                if (container) {
                    container.innerHTML = xhrGetParams.responseText;
                } else {
                    console.error('Элемент с id="containerBox" не найден');
                }
            } else {
                console.error('Ошибка: ' + xhrGetParams.statusText);
            }
        }
    };
    xhrGetParams.send();
}

function authFirst() {
    var email = sessionStorage.getItem('UserName');
    var password = sessionStorage.getItem('Password');

    var authenticateHeader = $('meta[name="www-authenticate"]').attr('content');
    var authenticateParams = parseAuthenticateHeader(authenticateHeader);

    sessionStorage.setItem('Role', authenticateParams.role);

    var digest = computeDigestResponse(email, password, authenticateParams);

    $('#email').val(email);
    $('#digest').val(digest);
    $('#authenticateHeader').val(authenticateHeader);

    $('#authForm').attr('action', authenticateParams.returnUrl);

    var xhr = new XMLHttpRequest();
    xhr.open('GET', authenticateParams.returnUrl, true);
    xhr.setRequestHeader('Authorization', 'Digest ' + digest);
    xhr.onreadystatechange = function () {
        if (xhr.status === 200) {
            if (xhr.readyState === 4) {
                var container = document.getElementById('containerBox');
                if (container) {
                    container.innerHTML = xhr.responseText;
                } else {
                    console.error('Элемент с id="containerBox" не найден');
                }
            }
        } else {
            console.error('Ошибка: ' + xhr.statusText);
        }
    };
    xhr.send();
}

function parseAuthenticateHeader(header) {
    header = header.replace(/^Digest\s*/, '');
    var params = {};
    var parts = header.split(',');
    parts.forEach(function (part) {
        var kv = part.split('=');
        params[kv[0].trim()] = kv[1].replace(/"/g, '').trim();
    });

    return params;
}

function computeDigestResponse(username, password, params) {
    var realm = params.realm;
    var nonce = params.nonce;
    var uri = params.returnUrl;
    var qop = params.qop;
    var nc = '00000001';
    var cnonce = generateCnonce();
    var role = params.role;

    var A1 = username + ':' + realm + ':' + password;
    var A2 = 'POST:' + uri;

    var HA1 = md5(A1);
    var HA2 = md5(A2);

    var response = md5(HA1 + ':' + nonce + ':' + nc + ':' + cnonce + ':' + qop + ':' + HA2);

    return 'username="' + username + '", realm="' + realm + '", nonce="' + nonce + '", uri="' + uri + '", qop=' + qop + ', nc=' + nc + ', cnonce="' + cnonce + '", response="' + response + '", opaque="' + params.opaque + '", role="' + role + '"';
}

function generateCnonce() {
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    for (var i = 0; i < 8; i++)
        text += possible.charAt(Math.floor(Math.random() * possible.length));
    return text;
}

function md5(input) {
    var hash = CryptoJS.MD5(input);
    return hash.toString(CryptoJS.enc.Hex);
}

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

function ChangeWidth(newWidth) {
    console.log(newWidth);
    const container = document.querySelector('.container');
    if (container) {
        container.style.maxWidth = newWidth;
    } else {
        console.error('Элемент с классом .container не найден');
    }
}
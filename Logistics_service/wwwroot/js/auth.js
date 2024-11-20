function auth() {

    var userName = sessionStorage.getItem('UserName');
    var password = sessionStorage.getItem('Password')

    // Получение данных из мета-тега
    var authenticateHeader = $('meta[name="www-authenticate"]').attr('content');
    var authenticateParams = parseAuthenticateHeader(authenticateHeader);

    // Формирование дайджест-ответа
    var response = computeDigestResponse(email, password, authenticateParams);


    // Отправка запроса с заголовком Authorization
    $.ajax({
        url: '/Auth/Login',
        type: 'POST',
        headers: {
            'Authorization': 'Digest ' + response
        },
        data: {
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val() // Добавляем CSRF-токен
        },
        success: function (data) {
            // Обработка успешного ответа
            alert('Успешный вход!');
        },
        error: function (xhr, status, error) {
            // Обработка ошибки
            alert('Ошибка: ' + xhr.responseText);
        }
    });
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
    var uri = window.location.pathname;
    var qop = params.qop;
    var nc = '00000001';
    var cnonce = generateCnonce();

    var A1 = username + ':' + realm + ':' + password;
    var A2 = 'POST:' + uri;

    var HA1 = md5(A1);
    var HA2 = md5(A2);

    var response = md5(HA1 + ':' + nonce + ':' + nc + ':' + cnonce + ':' + qop + ':' + HA2);

    return 'username="' + username + '", realm="' + realm + '", nonce="' + nonce + '", uri="' + uri + '", qop=' + qop + ', nc=' + nc + ', cnonce="' + cnonce + '", response="' + response + '", opaque="' + params.opaque + '"';
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
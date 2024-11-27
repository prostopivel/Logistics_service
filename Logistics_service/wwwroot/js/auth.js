function auth() {
    var email = sessionStorage.getItem('UserName');
    var password = sessionStorage.getItem('Password');

    var authenticateHeader = $('meta[name="www-authenticate"]').attr('content');
    var authenticateParams = parseAuthenticateHeader(authenticateHeader);

    var response = computeDigestResponse(email, password, authenticateParams);

    $.ajax({
        url: authenticateParams.returnUrl,
        type: 'GET',
        headers: {
            'Authorization': 'Digest ' + response
        },
        data: {
        },
        success: function (data) {
            alert('Успешный вход!');
            if (data.redirectUrl) {
                window.location.href = data.redirectUrl;
            }
        },
        error: function (xhr, status, error) {
            var errorMessage = xhr.responseJSON ? xhr.responseJSON.message : xhr.responseText;
            alert('Ошибка: ' + errorMessage);
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
    var uri = params.returnUrl; //window.location.pathname;
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
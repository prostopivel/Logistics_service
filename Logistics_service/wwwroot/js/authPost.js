document.addEventListener("DOMContentLoaded", function () {
    console.log("DOMContentLoaded event fired");

    function md5(input) {
        var hash = CryptoJS.MD5(input);
        return hash.toString(CryptoJS.enc.Hex);
    }

    function encryptPassword() {
        var email = document.getElementById("Email").value;
        var password = document.getElementById("PasswordHash").value;
        var realm = @ViewBag.Realm;

        var encryptedPassword = md5(email + ":" + realm + ":" + password);
        document.getElementById("PasswordHash").value = encryptedPassword;
        console.log("Encrypted Password: " + encryptedPassword);
    }

    function submitForm() {
        console.log("Submitting form");
        event.preventDefault();

        var form = document.getElementById('addUserForm');
        if (!form) {
            console.error("Form not found");
            return;
        }

        encryptPassword();
        var formData = new FormData(form);
        console.log("append");

        var xhr = new XMLHttpRequest();
        xhr.open('POST', '@Html.Raw(Url.Action("AdminPost", "Admin"))', true);
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');

        xhr.onreadystatechange = function () {
            if (xhr.readyState === 4) {
                if (xhr.status === 200) {
                    console.log("Form submitted successfully");
                    document.body.innerHTML = xhr.responseText;
                } else {
                    console.error('Ошибка: ' + xhr.statusText);
                }
            }
        };
        xhr.send(formData);
    }
});
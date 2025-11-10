$('#registerBtn').on('click', function (event) {
    event.preventDefault();

    const userName = $('#fullName').val().trim();
    const email = $('#registerEmail').val().trim();
    const password = $('#registerPassword').val().trim();
    const confirmPassword = $('#confirmPassword').val().trim();

    $("#error-message, #success-message").hide();

    if (!userName || !email || !password || !confirmPassword) {
        $("#error-message").text("Please fill in all required fields.").show();
        return;
    }

    if (password !== confirmPassword) {
        $("#error-message").text("Passwords do not match.").show();
        return;
    }

    $.ajax({
        url: 'https://localhost:7020/api/User/register',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ userName, email, password }),
        success: function (response) {
            $("#success-message")
                .text("Registration successful! Redirecting to login page...")
                .fadeIn();

            setTimeout(function () {
                window.location.href = 'login.html';
            }, 3000);
        },
        error: function (jqXHR, textStatus, errorThrown) {
            if (jqXHR.responseJSON && jqXHR.responseJSON.message) {
                $("#error-message").text(jqXHR.responseJSON.message).show();
            } else {
                $("#error-message").text("An error occurred. Please try again.").show();
            }
        }
    });
});
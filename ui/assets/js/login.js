$('#loginBtn').on('click', function () {
    const email = $('#floatingEmail').val();
    const password = $('#floatingPassword').val();
    if (!email || !password) {
        $("#error-message").text("Please fill in all required fields.").show();
        return;
    }
    $.ajax({
        url: 'https://localhost:7020/api/User/login',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email, password }),
        success: function (response) {
            $("#success-message")
                .text("Login successful! Redirecting to home page...")
                .fadeIn();

            setTimeout(function () {
                window.location.href = 'index.html';
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

$('#googleLoginBtn').on('click', function () {
    const feCallbackUrl = 'http://127.0.0.1:5500/PRN232_Group1_EbayClone_BuyerService/ui/google-callback.html';

    const googleOAuthUrl = 'https://accounts.google.com/o/oauth2/v2/auth' +
        '?client_id=399694141537-0t854jihnmp6h6qfqnb9hnv66eb4oa53.apps.googleusercontent.com' +
        '&redirect_uri=' + feCallbackUrl +
        '&response_type=code' +
        '&scope=email profile';
    window.location.href = googleOAuthUrl;
});

// $('#registerBtn').on('click', function () {
//     window.location.href = '/register';
// });
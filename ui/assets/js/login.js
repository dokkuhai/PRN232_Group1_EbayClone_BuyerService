
$(document).ready(function () {
    document.cookie = "";
    console.log(document.cookie);
    const token = Cookies.get('RememberMeToken');
    console.log("RememberMeToken from cookie:", token);
    if (!token) {
        console.log("No Remember Me token found in cookies.");
        return;

    }
    $.ajax({
        url: 'https://localhost:7020/api/User/login-with-remember-token',
        method: 'GET',
        data: { token: token },
        success: function (response) {
            console.log('Auto-login successful:', response);
            localStorage.setItem("accessToken", response.token);
            localStorage.setItem("userId", response.userId);
            localStorage.setItem("userName", response.userName);
            $("#success-message")
                .text("Auto-login successful! Redirecting to home page...")
                .fadeIn();
            setTimeout(function () {
                window.location.href = ' https://ebay.dokkuhai.dpdns.org/';
            }
                , 1000);

        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.log('No valid Remember Me token found or auto-login failed.');
        }
    });
});
function getCookie(name) {
    const value = Cookies.get('RememberMeToken');
    console.log("Document cookies:", document.cookie);
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) return parts.pop().split(';').shift();
}



$('#loginBtn').on('click', function () {
    const email = $('#floatingEmail').val();
    console.log("Email entered:", email);
    if (!email) {
        $("#error-message").text("Please fill in all required fields.").show();
        return;
    }
    const rememberMe = $('#stay-signed-in').is(':checked');
    console.log("Attempting login with email:", email, "Remember me:", rememberMe);
    $.ajax({
        url: '/api/user/login',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email, rememberMe }),
        success: function (response) {
            console.log('Login successful:', response);
            localStorage.setItem("accessToken", response.token);
            localStorage.setItem("userId", response.userId);
            localStorage.setItem("userName", response.userName);
            $("#success-message")
                .text("Login successful! Redirecting to home page...")
                .fadeIn();

            setTimeout(function () {
                window.location.href = ' https://ebay.dokkuhai.dpdns.org/';
            }, 1000);
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
    const feCallbackUrl = 'https://ebay.dokkuhai.dpdns.org/google-callback.html';

    const googleOAuthUrl = 'https://accounts.google.com/o/oauth2/v2/auth' +
        '?client_id=399694141537-0t854jihnmp6h6qfqnb9hnv66eb4oa53.apps.googleusercontent.com' +
        '&redirect_uri=' + feCallbackUrl +
        '&response_type=code' +
        '&scope=email profile';
    window.location.href = googleOAuthUrl;
});

$('#facebookLoginBtn').on('click', function () {
    const feCallbackUrl = ' https://ebay.dokkuhai.dpdns.org/facebook-callback.html';
    const facebookOAuthUrl = 'https://www.facebook.com/v16.0/dialog/oauth' +
        '?client_id=2714688805552787' +
        '&redirect_uri=' + feCallbackUrl +
        '&response_type=code' +
        '&scope=email,public_profile';
    window.location.href = facebookOAuthUrl;
});
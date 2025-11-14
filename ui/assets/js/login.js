(async function checkRememberMe() {
    const sessionToken = localStorage.getItem("accessToken");
    // if (sessionToken) {
    //     return;
    // }
    console.log("No session token found, checking remember-me login...");
    var cookies = document.cookie.split(';');
    try {
        const res = await fetch("https://localhost:7020/api/User/remember-me-login", {
            method: "GET",
            credentials: "include"
        });

        if (res.ok) {
            const data = await res.json();
            console.log("User is already logged in:", data);
            localStorage.setItem("accessToken", data.token);
            localStorage.setItem("userId", data.userId);
            localStorage.setItem("userName", data.userName);
            if (window.location.pathname.includes("login.html")) {
                window.location.href = "/index.html";
            }
        } else {
        }
    } catch (err) {
        console.error("Error checking auth status:", err);
    }
})();

$('#loginBtn').on('click', function () {
    const email = $('#floatingEmail').val();
    const password = $('#floatingPassword').val();
    if (!email || !password) {
        $("#error-message").text("Please fill in all required fields.").show();
        return;
    }
    const rememberMe = $('#rememberMeCheck').is(':checked');
    console.log("Attempting login with email:", email, "Remember me:", rememberMe);
    $.ajax({
        url: 'https://localhost:7020/api/User/login',
        method: 'POST',
        contentType: 'application/json',
        data: JSON.stringify({ email, password, rememberMe }),
        success: function (response) {
            console.log('Login successful:', response);
            localStorage.setItem("accessToken", response.token);
            localStorage.setItem("userId", response.userId);
            localStorage.setItem("userName", response.userName);
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
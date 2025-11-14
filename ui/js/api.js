// Base URL
const API_BASE_URL = "https://localhost:7020";

$.ajaxSetup({
    beforeSend: function (xhr, settings) {
        // Gắn token
        const token = localStorage.getItem("accessToken");
        if (token) {
            xhr.setRequestHeader("Authorization", "Bearer " + token);
        }

        if (!/^https?:\/\//i.test(settings.url)) {
            settings.url = API_BASE_URL + settings.url;
        }
    }
});

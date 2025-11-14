
$(document).ready(function () {
    // const role = Cookies.get('role');
    // console.log('User role:', role);
    // if (role) {

    //     if (role === 'Staff') {
    //         $('#staff-nav').removeClass('d-none');
    //         $('#student-nav').addClass('d-none');

    //         $('#staff-page').removeClass('d-none');
    //     } else if (role === 'Lecturer') {
    //         $('#lecturer-nav').removeClass('d-none');
    //         $('#student-nav').addClass('d-none');

    //         $('#lecturer-page').removeClass('d-none');
    //     } else if (role === 'Admin') {
    //         $('#admin-nav').removeClass('d-none');
    //         $('#student-nav').addClass('d-none');

    //         $('#admin-page').removeClass('d-none');
    //         $('#staff-page').removeClass('d-none');
    //         $('#lecturer-page').removeClass('d-none');
    //         $('#student-page').removeClass('d-none');
    //         $('#stat-page').removeClass('d-none');
    //     } else {
    //         $('#student-nav').removeClass('d-none');

    //         $('#student-page').removeClass('d-none');
    //     }
    // } else {
    // }
});

$('.logoutBtn').click(function () {

    // Cookies.remove('accessToken');
    // Cookies.remove('role');
    // Cookies.remove('username');
    // Cookies.remove('userId');

    window.location.href = '/';
});

function onXhrError(jqXHR, textStatus, errorThrown) {
    if (jqXHR.status === 401 || jqXHR.status === 409) {
        $("#error-message").text(jqXHR.responseJSON.message).show();
        return;
    } if (jqXHR.status === 403) {
        return alert('You do not have permission to access this resource.');
    } if (jqXHR.status === 400) {
        return alert(jqXHR.responseJSON.message);
    }
    console.error('Error:', errorThrown);
    alert('An error occurred: ' + errorThrown);
    return;
}



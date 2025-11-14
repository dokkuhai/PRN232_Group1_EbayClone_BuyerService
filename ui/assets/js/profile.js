
const editUsernameBtn = document.getElementById('editUsernameBtn');
const usernameEditForm = document.getElementById('usernameEditForm');
const currentUsernameSpan = document.getElementById('currentUsername');
const newUsernameInput = document.getElementById('newUsername');
const cancelUsernameEditBtn = document.getElementById('cancelUsernameEdit');
const saveUsernameEditBtn = document.getElementById('saveUsernameEdit');

function showEditForm() {
    usernameEditForm.classList.remove('hidden');
    editUsernameBtn.classList.add('hidden'); // Ẩn nút Edit
    currentUsernameSpan.classList.add('hidden'); // Ẩn username hiện tại
    newUsernameInput.value = currentUsernameSpan.textContent.trim(); // Đổ dữ liệu hiện tại vào input
}

function hideEditForm() {
    usernameEditForm.classList.add('hidden');
    editUsernameBtn.classList.remove('hidden'); // Hiện lại nút Edit
    currentUsernameSpan.classList.remove('hidden'); // Hiện lại username hiện tại
}

editUsernameBtn.addEventListener('click', showEditForm);

cancelUsernameEditBtn.addEventListener('click', hideEditForm);

saveUsernameEditBtn.addEventListener('click', function () {
    const updatedUsername = newUsernameInput.value.trim();
    if (updatedUsername) {
        currentUsernameSpan.textContent = updatedUsername;
        const userId = 3; // Giả sử userId là 3, thay đổi theo logic của bạn
        $.ajax({
            url: `https://ebay.dokkuhai.dpdns.org/api/Profile/update`,
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userId, username: updatedUsername }),
            success: function () {
                alert('Username updated successfully!');
            },
            error: function (error) {
                console.error('Error updating username:', error);
                alert('Failed to update username. Please try again.');
            }
        });
        hideEditForm(); // Ẩn form sau khi lưu
    } else {
        alert('Username cannot be empty!');
    }
});



// === Phone Edit Logic ===
const editPhoneBtn = document.getElementById('editPhoneBtn');
const phoneEditForm = document.getElementById('phoneEditForm');
const currentPhoneSpan = document.getElementById('currentPhone');
const newPhoneInput = document.getElementById('newPhone');
const cancelPhoneEditBtn = document.getElementById('cancelPhoneEdit');
const savePhoneEditBtn = document.getElementById('savePhoneEdit');

// Các phần tử khác của phone
const phoneStatus = document.getElementById('phoneStatus');
const phoneVerifyLink = document.getElementById('phoneVerifyLink');

function showPhoneEditForm() {
    phoneEditForm.classList.remove('hidden');
    // Ẩn các phần tử trong info item
    currentPhoneSpan.classList.add('hidden');
    phoneStatus.classList.add('hidden');
    phoneVerifyLink.classList.add('hidden');
    editPhoneBtn.classList.add('hidden');

    newPhoneInput.value = currentPhoneSpan.textContent.trim();
}

function hidePhoneEditForm() {
    phoneEditForm.classList.add('hidden');
    // Hiện lại các phần tử trong info item
    currentPhoneSpan.classList.remove('hidden');
    phoneStatus.classList.remove('hidden');
    phoneVerifyLink.classList.remove('hidden');
    editPhoneBtn.classList.remove('hidden');
}

editPhoneBtn.addEventListener('click', function (e) {
    e.preventDefault(); // Ngăn thẻ <a> chuyển trang
    showPhoneEditForm();
});

cancelPhoneEditBtn.addEventListener('click', hidePhoneEditForm);

savePhoneEditBtn.addEventListener('click', function () {
    const updatedPhone = newPhoneInput.value.trim();
    if (updatedPhone) {
        currentPhoneSpan.textContent = updatedPhone;
        if (!updatedPhone.match(/^\d{10,15}$/))
            return alert('Invalid phone number format!');
        const userId = 3; // Giả sử userId là 3, thay đổi theo logic của bạn
        $.ajax({
            url: `https://ebay.dokkuhai.dpdns.org/api/Profile/update`,
            method: 'PUT',
            contentType: 'application/json',
            data: JSON.stringify({ userId: userId, phoneNumber: updatedPhone }),
            success: function () {
                alert('Phone number updated successfully!');
            },
            error: function (error) {
                console.error('Error updating phone number:', error);
                alert('Failed to update phone number. Please try again.');
            }
        });
        hidePhoneEditForm();
    } else {
        alert('Phone cannot be empty!');
    }
});


$(document).ready(function () {
    const userId = 3;

    $.ajax({
        url: `https://ebay.dokkuhai.dpdns.org/api/Profile/${userId}`,
        method: 'GET',
        success: function (profile) {
            $('#currentUsername').text(profile.userName);
            $('#currentEmail').text(profile.email);
            $('#currentPhone').text(profile.phoneNumber);
            if (profile.isEmailVerified) {
                $('#emailStatus').addClass('hidden');
                $('#emailVerifyLink').addClass('hidden');
                $('#emailVerifiedStatus').removeClass('hidden');
            }
            var lastUpdated = new Date(profile.lastUpdated);
            if (!isNaN(lastUpdated)) {
                $('#lastUpdated').text(lastUpdated.toLocaleString());
            }
        },
        error: function (error) {
            console.error('Error fetching profile data:', error);
        }
    });
});

// === Email Verification Logic ===
$('#emailVerifyLink').on('click', function (e) {
    e.preventDefault();
    const userId = 3; // Giả sử userId là 3, thay đổi theo logic của bạn
    $.ajax({
        url: `https://ebay.dokkuhai.dpdns.org/api/Profile/send-verification-email/${userId}`,
        method: 'POST',
        contentType: 'application/json',
        success: function (data) {
            alert(data.message);
        },
        error: function (error) {
            console.error('Error sending verification email:', error);
            alert('Failed to send verification email. Please try again.');
        }
    });
});
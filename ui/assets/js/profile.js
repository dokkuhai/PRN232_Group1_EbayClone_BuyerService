
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

// Hàm ẩn form chỉnh sửa
function hideEditForm() {
    usernameEditForm.classList.add('hidden');
    editUsernameBtn.classList.remove('hidden'); // Hiện lại nút Edit
    currentUsernameSpan.classList.remove('hidden'); // Hiện lại username hiện tại
}

// Xử lý khi nhấn nút "Edit"
editUsernameBtn.addEventListener('click', showEditForm);

// Xử lý khi nhấn nút "Cancel"
cancelUsernameEditBtn.addEventListener('click', hideEditForm);

// Xử lý khi nhấn nút "Save"
saveUsernameEditBtn.addEventListener('click', function () {
    const updatedUsername = newUsernameInput.value.trim();
    if (updatedUsername) {
        currentUsernameSpan.textContent = updatedUsername; // Cập nhật hiển thị username
        // Thêm logic gửi dữ liệu lên server ở đây
        console.log('New username saved:', updatedUsername);
        alert('Username updated successfully (simulated)!'); // Thông báo giả định
        hideEditForm(); // Ẩn form sau khi lưu
    } else {
        alert('Username cannot be empty!');
    }
});
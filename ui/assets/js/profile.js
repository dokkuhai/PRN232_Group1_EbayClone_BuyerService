showAddAddressInput = () => {
    const inputField = document.querySelector('#manageAddressModal input[type="text"]');
    const addButton = document.querySelector('#manageAddressModal button.btn-primary.mt-3');
    inputField.removeAttribute('hidden');
    addButton.removeAttribute('hidden');
}
hideAddAddressInput = () => {
    const inputField = document.querySelector('#manageAddressModal input[type="text"]');
    const addButton = document.querySelector('#manageAddressModal button.btn-primary.mt-3');
    inputField.setAttribute('hidden', 'true');
    addButton.setAttribute('hidden', 'true');
}
showAddPaymentInput = () => {
    const inputField = document.querySelector('#managePaymentModal input[type="text"]');
    const addButton = document.querySelector('#managePaymentModal button.btn-primary.mt-3');
    inputField.removeAttribute('hidden');
    addButton.removeAttribute('hidden');
}
hideAddPaymentInput = () => {
    const inputField = document.querySelector('#managePaymentModal input[type="text"]');
    const addButton = document.querySelector('#managePaymentModal button.btn-primary.mt-3');
    inputField.setAttribute('hidden', 'true');
    addButton.setAttribute('hidden', 'true');
}

$('#savePaymentBtn').on('click', function () {
    const paymentEmail = $('#paymentMethod').val();
    const address = $('#shippingAddress').val();

    if (!paymentEmail || !address) {
        $('#payment-error').text('Please fill in all required fields.').removeClass('d-none');
        $('#payment-success').addClass('d-none');
        return;
    }

    // Gửi request API (ví dụ)
    $.ajax({
        url: 'https://localhost:7020/api/user/payment-info',
        method: 'PUT',
        contentType: 'application/json',
        data: JSON.stringify({ paymentEmail, address }),
        success: function (res) {
            $('#payment-success').text('Payment & Address updated successfully!').removeClass('d-none');
            $('#payment-error').addClass('d-none');

            // Cập nhật thông tin hiển thị trên card (giả lập)
            $('.profile-card p:first strong').next().remove(); // xóa text cũ nếu có
            $('.profile-card p:first').html(`<strong>Default Payment:</strong> PayPal - ${paymentEmail}`);
            $('.profile-card p:last').html(`<strong>Shipping Address:</strong><br>${address}`);

            setTimeout(() => {
                $('#editPaymentModal').modal('hide');
            }, 1500);
        },
        error: function () {
            $('#payment-error').text('Failed to update information. Please try again.').removeClass('d-none');
            $('#payment-success').addClass('d-none');
        }
    });
});

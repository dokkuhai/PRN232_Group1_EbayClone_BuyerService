let checkoutItems = [];
const SHIPPING_TAX_RATE = 0.08; // Giả sử thuế suất 8%
let currentShippingCost = 0; // Biến lưu trữ chi phí vận chuyển hiện tại

// Hàm lấy dữ liệu từ sessionStorage và khởi tạo trang
function loadCheckoutData() {
    const cartDataJSON = sessionStorage.getItem('checkoutItems');
    const placeOrderBtn = document.getElementById('place-order-btn');

    if (!cartDataJSON) {
        const orderSummary = document.querySelector('.order-summary');
        orderSummary.innerHTML = '<h3>Không tìm thấy sản phẩm. Vui lòng quay lại trang giỏ hàng.</h3>';
        if (placeOrderBtn) placeOrderBtn.disabled = true;
        return;
    }

    try {
        checkoutItems = JSON.parse(cartDataJSON);
        renderOrderSummary(checkoutItems);
        if (placeOrderBtn) placeOrderBtn.disabled = false;
    } catch (e) {
        console.error("Lỗi khi phân tích dữ liệu giỏ hàng:", e);
    }
}

// Hàm hiển thị danh sách sản phẩm và tính Subtotal
function renderOrderSummary(items) {
    const cartItemsContainer = document.querySelector('.order-summary .cart-items');
    // Lấy phần tử Subtotal để cập nhật
    const subtotalElement = document.querySelector('.price-breakdown .price-row:nth-child(1) span:nth-child(2)');
    
    cartItemsContainer.innerHTML = '';
    let subtotal = 0;

    if (items.length === 0) {
        cartItemsContainer.innerHTML = '<p>Giỏ hàng trống.</p>';
        updatePriceBreakdown(0);
        return;
    }

    items.forEach(item => {
        const itemSubtotal = item.unitPrice * item.quantity;
        subtotal += itemSubtotal;

        const cartItemHTML = `
            <div class="cart-item">
                <img src="${item.productImage}" alt="${item.productName}">
                <div class="item-details">
                    <h4>${item.productName}</h4>
                    <p class="seller">by ${item.sellerName}</p>
                    <p class="quantity">Qty: ${item.quantity}</p>
                </div>
                <div class="item-price">$${itemSubtotal.toFixed(2)}</div>
            </div>
        `;
        cartItemsContainer.insertAdjacentHTML('beforeend', cartItemHTML);
    });

    // Cập nhật Subtotal element
    if (subtotalElement) {
        subtotalElement.textContent = `$${subtotal.toFixed(2)}`;
    }
    
    updatePriceBreakdown(subtotal);
}

// Hàm cập nhật chi tiết giá (Subtotal, Tax, Total)
function updatePriceBreakdown(subtotal) {
    const tax = subtotal * SHIPPING_TAX_RATE;
    const total = subtotal + currentShippingCost + tax;

    // Cập nhật các trường hiển thị
    document.getElementById("tax").textContent = `$${tax.toFixed(2)}`;
    document.getElementById('total-price').textContent = `$${total.toFixed(2)}`;
}


// ============== LOGIC CŨ ĐƯỢC CHỈNH SỬA VÀ GIỮ LẠI ==============

// Payment method toggle
const paymentRadios = document.querySelectorAll('input[name="payment"]');
const creditCardForm = document.getElementById('credit-card-form');

paymentRadios.forEach(radio => {
    radio.addEventListener('change', (e) => {
        if (e.target.value === 'credit') {
            creditCardForm.style.display = 'block';
        } else {
            creditCardForm.style.display = 'none';
        }
        
        // Update UI
        document.querySelectorAll('.payment-options .radio-option').forEach(opt => {
            opt.classList.remove('selected');
        });
        e.target.closest('.radio-option').classList.add('selected');
    });
});

// Billing address toggle
const sameAddressCheckbox = document.getElementById('same-address');
const billingForm = document.getElementById('billing-form');

if (sameAddressCheckbox) {
    sameAddressCheckbox.addEventListener('change', (e) => {
        billingForm.style.display = e.target.checked ? 'none' : 'block';
    });
}


// Shipping method change
const shippingRadios = document.querySelectorAll('input[name="shipping"]');
const shippingCostElement = document.getElementById('shipping-cost');

const shippingPrices = {
    standard: 0,
    express: 9.99,
    overnight: 19.99
};

shippingRadios.forEach(radio => {
    radio.addEventListener('change', (e) => {
        const newCost = shippingPrices[e.target.value];
        currentShippingCost = newCost; // Cập nhật chi phí vận chuyển toàn cục
        shippingCostElement.textContent = newCost === 0 ? 'FREE' : '$' + newCost.toFixed(2);
        
        // Tái tính toán tổng tiền dựa trên subtotal hiện tại
        const subtotalText = document.querySelector('.price-breakdown .price-row:nth-child(1) span:nth-child(2)').textContent;
        const subtotal = parseFloat(subtotalText.replace('$', ''));

        updatePriceBreakdown(subtotal);
        
        // Update UI
        document.querySelectorAll('.shipping-options .radio-option').forEach(opt => {
            opt.classList.remove('selected');
        });
        e.target.closest('.radio-option').classList.add('selected');
    });
});

// Card number formatting
const cardNumberInput = document.getElementById('cardnumber');
if (cardNumberInput) {
    cardNumberInput.addEventListener('input', (e) => {
        let value = e.target.value.replace(/\s+/g, '').replace(/[^0-9]/gi, '');
        let formattedValue = value.replace(/(\d{4})/g, '$1 ').trim();
        e.target.value = formattedValue;
    });
}

// Expiry date formatting
const expiryInput = document.getElementById('expiry');
if (expiryInput) {
    expiryInput.addEventListener('input', (e) => {
        let value = e.target.value.replace(/\D/g, '');
        if (value.length >= 2) {
            value = value.slice(0, 2) + '/' + value.slice(2, 4);
        }
        e.target.value = value;
    });
}

// CVV formatting
const cvvInput = document.getElementById('cvv');
if (cvvInput) {
    cvvInput.addEventListener('input', (e) => {
        e.target.value = e.target.value.replace(/\D/g, '');
    });
}

// Place order button
const placeOrderBtn = document.getElementById('place-order-btn');
const confirmationModal = document.getElementById('confirmation-modal');
const closeBtn = document.querySelector('.close-btn');

if (placeOrderBtn) {
    placeOrderBtn.addEventListener('click', async () => {
        // Validate form
        const requiredFields = document.querySelectorAll('input[required], select[required]');
        let isValid = true;
        
        requiredFields.forEach(field => {
            if (field.closest('.form-section').style.display !== 'none' && !field.value) { 
                isValid = false;
                field.style.borderColor = '#e53238';
            } else {
                field.style.borderColor = '#ddd';
            }
        });
        
        if (isValid) {
            const checkoutData = JSON.parse(sessionStorage.getItem('checkoutItems'));
            
            // 1. Thu thập dữ liệu địa chỉ
            const shippingInfo = {
                fullName: document.getElementById('name').value,
                phone: document.getElementById('phone').value, // Đã thêm Phone Number
                street: document.getElementById('address').value,
                city: document.getElementById('city').value,
                state: document.getElementById('state').value,
                country: document.getElementById('country').value // Đã sửa thành input text
            };
            
            // 2. Chuẩn bị danh sách items cho API
            const itemsForApi = checkoutData.map(item => ({
                ProductId: item.ProductId, 
                Quantity: item.quantity,
                UnitPrice: item.unitPrice
            }));
            
            // 3. Chuẩn bị Payload
            const payload = {
                shippingInfo: shippingInfo,
                items: itemsForApi,
                shippingCost: currentShippingCost, 
                taxRate: SHIPPING_TAX_RATE 
            };
            
            try {
                const accessToken = localStorage.getItem("accessToken");
                
                // 4. Gọi API Đặt hàng .NET Core
                const res = await $.ajax({
                    url: '/api/Orders',
                    method: 'POST',
                    contentType: 'application/json',
                    headers: {
                        'Authorization': `Bearer ${accessToken}` 
                    },
                    data: JSON.stringify(payload)
                });

                if (res && res.orderId) {
                    // Thành công: Hiển thị modal và xóa session
                    document.getElementById('order-number').textContent = res.orderNumber;
                    confirmationModal.classList.add('show');
                    sessionStorage.removeItem('checkoutItems');
                } else {
                    alert('Đặt hàng thất bại: Lỗi không xác định.');
                }
            } catch (xhr) {
                console.error("Lỗi đặt hàng:", xhr.responseText);
                // Hiển thị thông báo lỗi từ server nếu có
                const errorMessage = xhr.responseJSON?.Message || 'Không thể kết nối hoặc đặt hàng thất bại.';
                alert(`Lỗi: ${errorMessage}`);
            }
        } else {
            alert('Vui lòng điền đầy đủ các trường bắt buộc.');
        }
    });
}

if (closeBtn) {
    closeBtn.addEventListener('click', () => {
        confirmationModal.classList.remove('show');
    });
}

// Close modal when clicking outside
window.addEventListener('click', (e) => {
    if (e.target === confirmationModal) {
        confirmationModal.classList.remove('show');
    }
});


// Khởi tạo: Tải dữ liệu giỏ hàng khi trang được tải
document.addEventListener('DOMContentLoaded', loadCheckoutData);
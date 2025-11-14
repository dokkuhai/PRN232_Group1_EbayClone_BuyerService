let cartData = [];
let quantityTimeouts = {}; // Lưu timeout cho mỗi cart item

document.addEventListener("DOMContentLoaded", () => {
  fetchCartData();
});

function fetchCartData() {
  let url = "/api/Cart";
  const guestToken = localStorage.getItem("guestToken");
  const accessToken = localStorage.getItem("accessToken");

  if (guestToken) {
    url += `?token=${guestToken}`;
  }

  $.ajax({
    url: url,
    method: "GET",
    success: function (res) {
      if (res.statusCode === 200 && res.data) {
        cartData = res.data.map((item) => ({
          id: item.cartItemId,
          title: item.productName,
          price: item.unitPrice,
          quantity: item.quantity,
          image: item.productImage,
          seller: item.sellerName,
          availableStock: item.availableStock,
        }));
      } else {
        cartData = [];
      }
      renderCart();
    },
    error: function (xhr) {
      console.error("Fetch cart lỗi:", xhr.responseText);
      if(xhr.status === 401 && accessToken){
        // localStorage.removeItem("accessToken");
      }
    },
  });
}
function onQuantityInputChange(id, value) {
  const item = cartData.find(i => i.id === id);
  if (!item) return;

  let quantity = parseInt(value);
  if (isNaN(quantity) || quantity < 1) quantity = 1;
  if (quantity > item.availableStock) {
    quantity = item.availableStock;
    showToast(`Only ${item.availableStock} item${item.availableStock > 1 ? 's' : ''} left in stock`);
  }

  item.quantity = quantity;
  renderCart();

  if (quantityTimeouts[id]) clearTimeout(quantityTimeouts[id]);
  quantityTimeouts[id] = setTimeout(() => {
    updateCartQuantityApi(id, item.quantity);
    quantityTimeouts[id] = null;
  }, 1000);
}


function renderCart() {
  const cartItemsContainer = document.getElementById("cartItems");
  const emptyCart = document.getElementById("emptyCart");

  if (cartData.length === 0) {
    cartItemsContainer.style.display = "none";
    emptyCart.style.display = "block";
    updateSummary();
    return;
  }

  cartItemsContainer.style.display = "flex";
  emptyCart.style.display = "none";
  cartItemsContainer.innerHTML = "";

  cartData.forEach((item) => {
    const cartItem = document.createElement("div");
    cartItem.className = "cart-item";

    let quantityControls = "";
    if (item.availableStock === 0) {
      quantityControls = `<div class="out-of-stock">Out of stock</div>`;
    } else {
      quantityControls = `
        <div class="quantity-control">
          <button ${item.quantity <= 1 ? "disabled" : ""} onclick="decreaseQuantity(${item.id})">−</button>
          <input type="number" value="${item.quantity}" min="1" 
          onchange="onQuantityInputChange(${item.id}, this.value)">
          <button ${item.quantity >= item.availableStock ? "disabled" : ""} onclick="increaseQuantity(${item.id})">+</button>
        </div>
      `;
    }

    cartItem.innerHTML = `
      <div class="item-image">
          <img src="${item.image}" alt="${item.title}">
      </div>
      <div class="item-details">
          <a href="#" class="item-title">${item.title}</a>
          <div class="item-seller">Sold by <a><strong>${item.seller}</strong></a></div>
          <div>
            <span class="item-price">
              US $${(item.price * item.quantity).toLocaleString("en-US", {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2,
              })}
              <br/>
              (${(item.price * item.quantity * 25000).toLocaleString("vi-VN")} VND)
            </span>
          </div>
          <div class="item-controls">
              ${quantityControls}
              <button class="remove-btn" onclick="removeItem(${item.id})">Remove</button>
          </div>
      </div>
    `;
    cartItemsContainer.appendChild(cartItem);
  });

  updateSummary();
}


// Increase quantity
function increaseQuantity(id) {
  changeQuantity(id, 1);
}

// Decrease quantity
function decreaseQuantity(id) {
  changeQuantity(id, -1);
}

// Hàm thay đổi quantity
function changeQuantity(id, delta) {
  const item = cartData.find((item) => item.id === id);
  if (!item || item.availableStock === 0) return;

  const newQuantity = item.quantity + delta;

  if (newQuantity < 1) return;
  if (newQuantity > item.availableStock) return;

  item.quantity = newQuantity;
  renderCart();

  if (quantityTimeouts[id]) clearTimeout(quantityTimeouts[id]);

  quantityTimeouts[id] = setTimeout(() => {
    updateCartQuantityApi(id, item.quantity);
    quantityTimeouts[id] = null;
  }, 1000);
}



// Gọi API update quantity
function updateCartQuantityApi(id, quantity) {
  const guestToken = localStorage.getItem("guestToken");
  let url = `/api/Cart/${id}`;
  if (guestToken) url += `?token=${guestToken}`;

  $.ajax({
    url: url,
    method: "PATCH",
    contentType: "application/json",
    data: JSON.stringify({ quantity }),
    success: function (res) {
      console.log("Quantity updated:", res);
    },
    error: function (xhr) {
      console.error("Update quantity lỗi:", xhr.responseText);
    },
  });
}

// Remove item
function removeItem(id) {
  const guestToken = localStorage.getItem("guestToken");
  let url = `/api/Cart/${id}`;
  if (guestToken) url += `?token=${guestToken}`;

  $.ajax({
    url: url,
    method: "DELETE",
    success: function (res) {
      fetchCartData();
    },
    error: function (xhr) {
      console.error("Xoá item lỗi:", xhr.responseText);
    },
  });
}

// Update order summary
function updateSummary() {
  const subtotal = cartData.reduce((sum, item) => sum + item.price * item.quantity, 0);
  const tax = subtotal * 0.08;
  const total = subtotal + tax;

  document.getElementById("subtotal").textContent = `$${subtotal.toFixed(2)}`;
  document.getElementById("tax").textContent = `$${tax.toFixed(2)}`;
  document.getElementById("total").textContent = `$${total.toFixed(2)}`;
}
function showToast(message, duration = 2000) {
  const toast = document.getElementById("toast");
  toast.textContent = message;
  toast.classList.add("show");

  setTimeout(() => {
    toast.classList.remove("show");
  }, duration);
}

function proceedToCheckout() {
    if (cartData.length === 0) {
        showToast("Giỏ hàng trống, không thể tiến hành thanh toán.", 3000);
        return;
    }

    // Lọc và chuẩn bị dữ liệu cần thiết cho trang checkout
    const dataToTransfer = cartData.map(item => ({
        // LƯU Ý: Frontend đang dùng 'cartItemId' làm placeholder cho ProductId
        ProductId: item.id, // Dùng item.id (cartItemId) tạm thời, API sẽ cần ProductId thật
        productName: item.title,
        sellerName: item.seller,
        quantity: item.quantity,
        unitPrice: item.price,
        productImage: item.image
    }));

    try {
        // Lưu trữ dữ liệu vào sessionStorage
        sessionStorage.setItem('checkoutItems', JSON.stringify(dataToTransfer));
        
        // Chuyển hướng người dùng đến trang thanh toán
        window.location.href = 'checkout.html'; 
        
    } catch (error) {
        console.error("Lỗi khi lưu trữ hoặc chuyển hướng:", error);
        showToast("Có lỗi xảy ra, vui lòng thử lại.", 3000);
    }
}

// Bắt sự kiện click cho nút "Proceed to Checkout"
document.addEventListener("DOMContentLoaded", () => {
    // Đảm bảo fetchCartData() được gọi
    // fetchCartData(); 
    // Hàm này đã được gọi ở đầu file, không cần thêm lại.

    const checkoutBtn = document.querySelector(".checkout-btn");
    if (checkoutBtn) {
        checkoutBtn.addEventListener('click', proceedToCheckout);
    }
});
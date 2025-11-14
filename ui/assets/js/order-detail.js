// Order Detail Management
const OrderDetail = {
    BUYER_ID: parseInt(localStorage.getItem('userId')), // Get from authentication
    currentOrderId: null,
    orderData: null,

    init() {
        // Redirect to login if not authenticated
        const token = localStorage.getItem('accessToken');
        if (!token) {
            window.location.href = 'login.html';
            return;
        }
        
        const urlParams = new URLSearchParams(window.location.search);
        this.currentOrderId = urlParams.get('orderId');

        if (!this.currentOrderId) {
            this.showError('Order ID not provided');
            return;
        }

        this.loadOrderDetail();
    },

    async loadOrderDetail() {
        const loading = document.getElementById('loading');
        if (loading) loading.classList.remove('d-none');

        try {
            const token = localStorage.getItem('accessToken');
            const response = await fetch(`/api/orders/${this.currentOrderId}`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            const result = await response.json();

            if (result.statusCode === 200) {
                this.orderData = result.data;
                this.displayOrderDetail(this.orderData);
            } else {
                this.showError('Failed to load order: ' + result.message);
            }
        } catch (error) {
            console.error('Error loading order:', error);
            this.showError('Failed to load order details. Please try again.');
        } finally {
            if (loading) loading.classList.add('d-none');
        }
    },

    displayOrderDetail(order) {
        // Order Summary
        const orderIdDisplay = document.getElementById('orderIdDisplay');
        const orderDate = document.getElementById('orderDate');
        const orderStatus = document.getElementById('orderStatus');

        if (orderIdDisplay) orderIdDisplay.textContent = order.orderId;
        if (orderDate) {
            orderDate.textContent = new Date(order.orderDate).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });
        }
        if (orderStatus) {
            orderStatus.textContent = order.status;
            orderStatus.className = 'status-badge ' + this.getStatusClass(order.status);
        }
        
        const orderSummary = document.getElementById('orderSummary');
        if (orderSummary) orderSummary.classList.remove('d-none');

        // Order Items
        this.displayOrderItems(order.items, order.totalPrice);

        // Shipping Address
        this.displayShippingAddress(order.shippingAddress);

        // Shipping Timeline
        this.displayShippingTimeline(order.shippingInfos);

        // Return Requests
        this.displayReturnRequests(order.returnRequests);
    },

    displayOrderItems(items, totalPrice) {
        const itemsList = document.getElementById('itemsList');
        const orderTotal = document.getElementById('orderTotal');
        const orderItems = document.getElementById('orderItems');

        if (!itemsList || !items) return;

        itemsList.innerHTML = items.map(item => `
            <div class="item-row">
                <div class="d-flex justify-content-between">
                    <div>
                        <strong>${item.productName || 'Product #' + item.productId}</strong>
                        <div class="text-muted small">Quantity: ${item.quantity}</div>
                    </div>
                    <div class="text-end">
                        <div>$${item.unitPrice.toFixed(2)} each</div>
                        <div class="fw-bold">$${item.subtotal.toFixed(2)}</div>
                    </div>
                </div>
            </div>
        `).join('');

        if (orderTotal) orderTotal.textContent = '$' + totalPrice.toFixed(2);
        if (orderItems) orderItems.classList.remove('d-none');
    },

    displayShippingAddress(address) {
        const addressDetails = document.getElementById('addressDetails');
        const shippingAddress = document.getElementById('shippingAddress');

        if (!addressDetails || !address) return;

        addressDetails.innerHTML = `
            <p class="mb-1">${address.street || ''}</p>
            <p class="mb-1">${address.city || ''}, ${address.state || ''}</p>
            <p class="mb-0">${address.country || ''}</p>
        `;

        if (shippingAddress) shippingAddress.classList.remove('d-none');
    },

    displayShippingTimeline(shippingInfos) {
        const timelineList = document.getElementById('timelineList');
        const shippingTimeline = document.getElementById('shippingTimeline');

        if (!timelineList || !shippingInfos || shippingInfos.length === 0) return;

        timelineList.innerHTML = shippingInfos.map(shipping => `
            <div class="timeline-item">
                <div><strong>${shipping.status}</strong></div>
                <div class="small text-muted">${shipping.carrier} - ${shipping.trackingNumber}</div>
                <div class="small text-muted">
                    Est. Arrival: ${new Date(shipping.estimatedArrival).toLocaleDateString()}
                </div>
            </div>
        `).join('');

        if (shippingTimeline) shippingTimeline.classList.remove('d-none');
    },

    displayReturnRequests(returnRequests) {
        const returnsList = document.getElementById('returnsList');
        const btnRequestReturn = document.getElementById('btnRequestReturn');
        const returnRequestsDiv = document.getElementById('returnRequests');

        if (!returnsList) return;

        if (returnRequests && returnRequests.length > 0) {
            returnsList.innerHTML = returnRequests.map(returnReq => `
                <div class="alert alert-info">
                    <div class="d-flex justify-content-between align-items-center">
                        <div>
                            <strong>Return Request #${returnReq.id}</strong>
                            <div class="mt-1"><small>${returnReq.reason}</small></div>
                            <div class="small text-muted">
                                Requested on ${new Date(returnReq.createdAt).toLocaleDateString()}
                            </div>
                        </div>
                        <span class="status-badge ${this.getStatusClass(returnReq.status)}">${returnReq.status}</span>
                    </div>
                </div>
            `).join('');

            // Check if there's already a pending return
            const hasPendingReturn = returnRequests.some(r => r.status === 'Pending');
            if (hasPendingReturn && btnRequestReturn) {
                btnRequestReturn.disabled = true;
                btnRequestReturn.innerHTML = '<i class="fas fa-clock me-2"></i>Return Request Pending';
            }
        } else {
            returnsList.innerHTML = '<p class="text-muted">No return requests yet.</p>';
        }

        if (returnRequestsDiv) returnRequestsDiv.classList.remove('d-none');
    },

    async submitReturnRequest() {
        const reason = document.getElementById('returnReason')?.value.trim();

        if (!reason) {
            alert('Please provide a reason for the return');
            return;
        }

        try {
            const token = localStorage.getItem('accessToken');
            const response = await fetch(`/api/orders/${this.currentOrderId}/return-requests`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    userId: this.BUYER_ID,
                    reason: reason
                })
            });

            const result = await response.json();

            if (result.statusCode === 201) {
                // Close modal
                const modalElement = document.getElementById('returnModal');
                if (modalElement && typeof bootstrap !== 'undefined') {
                    const modal = bootstrap.Modal.getInstance(modalElement);
                    if (modal) modal.hide();
                }

                // Show success message
                alert('Return request submitted successfully!');

                // Reload order details
                this.loadOrderDetail();

                // Clear form
                const returnReason = document.getElementById('returnReason');
                if (returnReason) returnReason.value = '';
            } else {
                alert('Failed to submit return request: ' + result.message);
            }
        } catch (error) {
            console.error('Error submitting return request:', error);
            alert('Failed to submit return request. Please try again.');
        }
    },

    getStatusClass(status) {
        const statusMap = {
            'Pending': 'status-pending',
            'In Transit': 'status-in-transit',
            'Delivered': 'status-delivered',
            'Shipped': 'status-shipped',
            'Approved': 'status-approved',
            'Rejected': 'status-rejected',
            'Cancelled': 'status-cancelled'
        };
        return statusMap[status] || 'status-pending';
    },

    showError(message) {
        const loading = document.getElementById('loading');
        if (!loading) return;
        
        loading.innerHTML = `
            <div class="alert alert-danger">
                <i class="fas fa-exclamation-triangle me-2"></i>${message}
            </div>
            <button onclick="history.back()" class="btn btn-back">
                <i class="fas fa-arrow-left me-2"></i>Go Back
            </button>
        `;
    }
};

// Global function for modal button
function submitReturnRequest() {
    OrderDetail.submitReturnRequest();
}

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('orderSummary')) {
        OrderDetail.init();
    }
});

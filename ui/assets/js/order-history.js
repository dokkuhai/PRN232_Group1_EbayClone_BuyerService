// Order History Management
const OrderHistory = {
    BUYER_ID: 1, // In real app, this would come from authentication/session
    currentPage: 1,
    pageSize: 10,
    allOrders: [],

    init() {
        this.loadOrders();
        this.attachEventListeners();
    },

    attachEventListeners() {
        const applyFiltersBtn = document.getElementById('applyFilters');
        if (applyFiltersBtn) {
            applyFiltersBtn.addEventListener('click', () => {
                this.currentPage = 1;
                this.loadOrders();
            });
        }
    },

    async loadOrders() {
        const loading = document.getElementById('loading');
        const container = document.getElementById('ordersContainer');

        if (loading) loading.classList.remove('d-none');

        try {
            const statusFilter = document.getElementById('statusFilter')?.value || '';
            let url = `/api/orders?buyerId=${this.BUYER_ID}&page=${this.currentPage}&pageSize=${this.pageSize}`;

            if (statusFilter) {
                url += `&status=${encodeURIComponent(statusFilter)}`;
            }

            const response = await fetch(url);
            const result = await response.json();

            if (result.statusCode === 200) {
                this.allOrders = result.data || [];
                this.applySorting();
                this.displayOrders(this.allOrders);
            } else {
                this.showError('Failed to load orders: ' + result.message);
            }
        } catch (error) {
            console.error('Error loading orders:', error);
            this.showError('Failed to load orders. Please try again.');
        } finally {
            if (loading) loading.classList.add('d-none');
        }
    },

    applySorting() {
        const sortOrder = document.getElementById('sortOrder')?.value || 'newest';

        switch (sortOrder) {
            case 'oldest':
                this.allOrders.sort((a, b) => new Date(a.orderDate) - new Date(b.orderDate));
                break;
            case 'price-high':
                this.allOrders.sort((a, b) => b.totalPrice - a.totalPrice);
                break;
            case 'price-low':
                this.allOrders.sort((a, b) => a.totalPrice - b.totalPrice);
                break;
            case 'newest':
            default:
                this.allOrders.sort((a, b) => new Date(b.orderDate) - new Date(a.orderDate));
                break;
        }
    },

    displayOrders(orders) {
        const container = document.getElementById('ordersContainer');
        if (!container) return;

        container.innerHTML = '';

        if (!orders || orders.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-box-open"></i>
                    <h3>No orders found</h3>
                    <p class="text-muted">You haven't placed any orders yet, or no orders match your filters.</p>
                    <a href="shop.html" class="btn btn-view-detail mt-3">
                        <i class="fas fa-shopping-cart me-2"></i>Start Shopping
                    </a>
                </div>
            `;
            return;
        }

        orders.forEach(order => {
            const orderCard = this.createOrderCard(order);
            container.innerHTML += orderCard;
        });
    },

    createOrderCard(order) {
        const orderDate = new Date(order.orderDate).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });

        const statusClass = this.getStatusClass(order.status);
        const shippingInfo = order.latestShippingStatus ? 
            `<div class="shipping-status"><i class="fas fa-truck"></i> Shipping: ${order.latestShippingStatus}</div>` : 
            '';

        return `
            <div class="order-card">
                <div class="order-header">
                    <div>
                        <div class="order-id">Order #${order.orderId}</div>
                        <small class="text-muted">${orderDate}</small>
                    </div>
                    <span class="status-badge ${statusClass}">${order.status}</span>
                </div>
                <div class="order-info">
                    <div class="order-details">
                        <span><i class="fas fa-box"></i>${order.itemCount} item(s)</span>
                        <span class="order-total">$${order.totalPrice.toFixed(2)}</span>
                    </div>
                    <button onclick="OrderHistory.viewOrderDetail(${order.orderId})" class="btn btn-view-detail">
                        <i class="fas fa-eye me-2"></i>View Details
                    </button>
                </div>
                ${shippingInfo}
            </div>
        `;
    },

    getStatusClass(status) {
        const statusMap = {
            'Pending': 'status-pending',
            'In Transit': 'status-in-transit',
            'Delivered': 'status-delivered',
            'Cancelled': 'status-cancelled'
        };
        return statusMap[status] || 'status-pending';
    },

    viewOrderDetail(orderId) {
        window.location.href = `order-detail.html?orderId=${orderId}`;
    },

    showError(message) {
        const container = document.getElementById('ordersContainer');
        if (!container) return;
        
        container.innerHTML = `
            <div class="alert alert-danger" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>${message}
            </div>
        `;
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('ordersContainer')) {
        OrderHistory.init();
    }
});

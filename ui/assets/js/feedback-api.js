const FEEDBACK_API_BASE = 'https://localhost:7020/api/feedback';

// ============================================================
// UTILITY FUNCTIONS
// ============================================================

function getToken() {
    return localStorage.getItem('jwt_token');
}

function isLoggedIn() {
    return !!getToken();
}

function showToast(message, type = 'info') {
    // Create toast element
    const toast = document.createElement('div');
    toast.className = `toast-notification toast-${type}`;
    toast.innerHTML = `
        <div class="toast-content">
            <i class="bi bi-${type === 'success' ? 'check-circle' : type === 'error' ? 'x-circle' : 'info-circle'}"></i>
            <span>${message}</span>
        </div>
    `;
    
    // Add to body
    document.body.appendChild(toast);
    
    // Show toast
    setTimeout(() => toast.classList.add('show'), 10);
    
    // Remove after 3s
    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 300);
    }, 3000);
}

// ============================================================
// FEEDBACK API CALLS
// ============================================================

const FeedbackAPI = {
    
    // ===== PUBLIC ENDPOINTS =====
    
    /**
     * Get seller feedback statistics
     * @param {number} sellerId 
     */
    async getSellerStats(sellerId) {
        try {
            const response = await fetch(`${FEEDBACK_API_BASE}/seller/${sellerId}`);
            if (!response.ok) throw new Error('Failed to fetch seller stats');
            return await response.json();
        } catch (error) {
            console.error('Error getting seller stats:', error);
            return null;
        }
    },
    
    /**
     * Get seller feedbacks (paginated)
     * @param {number} sellerId 
     * @param {number} page 
     * @param {number} pageSize 
     */
    async getSellerFeedbacks(sellerId, page = 1, pageSize = 20) {
        try {
            const response = await fetch(
                `${FEEDBACK_API_BASE}/seller/${sellerId}/list?page=${page}&pageSize=${pageSize}`
            );
            if (!response.ok) throw new Error('Failed to fetch feedbacks');
            return await response.json();
        } catch (error) {
            console.error('Error getting seller feedbacks:', error);
            return { data: [] };
        }
    },
    
    /**
     * Get feedback by ID
     * @param {number} feedbackId 
     */
    async getFeedbackById(feedbackId) {
        try {
            const response = await fetch(`${FEEDBACK_API_BASE}/${feedbackId}`);
            if (!response.ok) throw new Error('Feedback not found');
            return await response.json();
        } catch (error) {
            console.error('Error getting feedback:', error);
            return null;
        }
    },
    
    // ===== PROTECTED ENDPOINTS =====
    
    /**
     * Leave feedback for seller
     * @param {object} data - CreateFeedbackDto
     */
    async leaveFeedback(data) {
        const token = getToken();
        if (!token) {
            showToast('Please login to leave feedback', 'error');
            setTimeout(() => window.location.href = 'login.html', 1000);
            return null;
        }
        
        try {
            const response = await fetch(`${FEEDBACK_API_BASE}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(data)
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to leave feedback');
            }
            
            return await response.json();
        } catch (error) {
            console.error('Error leaving feedback:', error);
            showToast(error.message, 'error');
            throw error;
        }
    },
    
    /**
     * Get my feedbacks (buyer)
     */
    async getMyFeedbacks() {
        const token = getToken();
        if (!token) {
            showToast('Please login', 'error');
            return { data: [] };
        }
        
        try {
            const response = await fetch(`${FEEDBACK_API_BASE}/my-feedbacks`, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (!response.ok) throw new Error('Failed to fetch my feedbacks');
            return await response.json();
        } catch (error) {
            console.error('Error getting my feedbacks:', error);
            return { data: [] };
        }
    },
    
    /**
     * Get received feedbacks (seller)
     * @param {number} page 
     * @param {number} pageSize 
     */
    async getReceivedFeedbacks(page = 1, pageSize = 20) {
        const token = getToken();
        if (!token) {
            showToast('Please login', 'error');
            return { data: [] };
        }
        
        try {
            const response = await fetch(
                `${FEEDBACK_API_BASE}/received?page=${page}&pageSize=${pageSize}`,
                {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                }
            );
            
            if (!response.ok) throw new Error('Failed to fetch received feedbacks');
            return await response.json();
        } catch (error) {
            console.error('Error getting received feedbacks:', error);
            return { data: [] };
        }
    },
    
    /**
     * Check if can leave feedback for order
     * @param {number} orderId 
     */
    async canLeaveFeedback(orderId) {
        const token = getToken();
        if (!token) {
            return { canLeave: false, reason: 'Not logged in' };
        }
        
        try {
            const response = await fetch(
                `${FEEDBACK_API_BASE}/order/${orderId}/can-leave`,
                {
                    headers: {
                        'Authorization': `Bearer ${token}`
                    }
                }
            );
            
            if (!response.ok) throw new Error('Failed to check eligibility');
            return await response.json();
        } catch (error) {
            console.error('Error checking can leave feedback:', error);
            return { canLeave: false, reason: 'Error checking' };
        }
    },
    
    /**
     * Update feedback
     * @param {number} feedbackId 
     * @param {object} data - UpdateFeedbackDto
     */
    async updateFeedback(feedbackId, data) {
        const token = getToken();
        if (!token) {
            showToast('Please login', 'error');
            return null;
        }
        
        try {
            const response = await fetch(`${FEEDBACK_API_BASE}/${feedbackId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(data)
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to update feedback');
            }
            
            return await response.json();
        } catch (error) {
            console.error('Error updating feedback:', error);
            showToast(error.message, 'error');
            throw error;
        }
    },
    
    /**
     * Delete feedback
     * @param {number} feedbackId 
     */
    async deleteFeedback(feedbackId) {
        const token = getToken();
        if (!token) {
            showToast('Please login', 'error');
            return false;
        }
        
        try {
            const response = await fetch(`${FEEDBACK_API_BASE}/${feedbackId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });
            
            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.message || 'Failed to delete feedback');
            }
            
            return true;
        } catch (error) {
            console.error('Error deleting feedback:', error);
            showToast(error.message, 'error');
            throw error;
        }
    }
};

// ============================================================
// UTILITY: Format Date
// ============================================================

function formatDate(dateString) {
    const date = new Date(dateString);
    const now = new Date();
    const diffTime = Math.abs(now - date);
    const diffDays = Math.floor(diffTime / (1000 * 60 * 60 * 24));
    
    if (diffDays === 0) return 'Today';
    if (diffDays === 1) return 'Yesterday';
    if (diffDays < 7) return `${diffDays} days ago`;
    if (diffDays < 30) return `${Math.floor(diffDays / 7)} weeks ago`;
    if (diffDays < 365) return `${Math.floor(diffDays / 30)} months ago`;
    
    return date.toLocaleDateString('en-US', { 
        year: 'numeric', 
        month: 'short', 
        day: 'numeric' 
    });
}

// ============================================================
// UTILITY: Render Stars
// ============================================================

function renderStars(rating, maxStars = 5) {
    const fullStars = Math.floor(rating);
    const emptyStars = maxStars - fullStars;
    
    return '★'.repeat(fullStars) + '☆'.repeat(emptyStars);
}

function renderStarsHTML(rating, maxStars = 5, className = '') {
    return `<span class="stars ${className}">${renderStars(rating, maxStars)}</span>`;
}
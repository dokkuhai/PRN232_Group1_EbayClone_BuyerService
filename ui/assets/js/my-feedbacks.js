// My Feedbacks Management - Following Order History Pattern
const MyFeedbacks = {
    allFeedbacks: [],
    editModal: null,
    editRatings: {},

    init() {
        // Redirect to login if not authenticated
        const token = localStorage.getItem('accessToken');
        const userId = localStorage.getItem('userId');
        
        if (!token || !userId) {
            window.location.href = 'login.html';
            return;
        }
        
        this.loadFeedbacks();
        this.attachEventListeners();
        this.initModal();
    },

    attachEventListeners() {
        const applyFiltersBtn = document.getElementById('applyFilters');
        if (applyFiltersBtn) {
            applyFiltersBtn.addEventListener('click', () => {
                this.loadFeedbacks();
            });
        }

        // Save edit button
        const saveEditBtn = document.getElementById('saveEditBtn');
        if (saveEditBtn) {
            saveEditBtn.addEventListener('click', () => {
                this.saveEdit();
            });
        }

        // Comment character count
        const editComment = document.getElementById('editComment');
        if (editComment) {
            editComment.addEventListener('input', () => {
                document.getElementById('editCommentCount').textContent = editComment.value.length;
            });
        }
    },

    initModal() {
        const modalElement = document.getElementById('editFeedbackModal');
        if (modalElement) {
            this.editModal = new bootstrap.Modal(modalElement);
            
            // Feedback type selector
            document.querySelectorAll('#editFeedbackModal .feedback-type-btn').forEach(btn => {
                btn.addEventListener('click', () => {
                    document.querySelectorAll('#editFeedbackModal .feedback-type-btn').forEach(b => b.classList.remove('active'));
                    btn.classList.add('active');
                    document.getElementById('editFeedbackType').value = btn.dataset.type;
                });
            });

            // Star ratings
            this.initStarRatings();
        }
    },

    initStarRatings() {
        document.querySelectorAll('#editFeedbackModal .star-rating-input').forEach(ratingGroup => {
            const stars = ratingGroup.querySelectorAll('.star');
            const ratingName = ratingGroup.dataset.ratingName;
            
            stars.forEach(star => {
                star.addEventListener('click', () => {
                    const value = parseInt(star.dataset.value);
                    this.editRatings[ratingName] = value;
                    
                    stars.forEach((s, idx) => {
                        if (idx < value) {
                            s.classList.add('active');
                        } else {
                            s.classList.remove('active');
                        }
                    });
                });
            });
        });
    },

    async loadFeedbacks() {
        const loading = document.getElementById('loading');
        const container = document.getElementById('feedbacksContainer');

        if (loading) loading.classList.remove('d-none');

        try {
            const token = localStorage.getItem('accessToken');
            const typeFilter = document.getElementById('typeFilter')?.value || '';
            let url = 'https://ebay.dokkuhai.dpdns.org/api/feedback/my-feedbacks';

            const response = await fetch(url, {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) {
                throw new Error('Failed to load feedbacks');
            }

            const result = await response.json();
            this.allFeedbacks = result.data || [];

            // Apply filters
            if (typeFilter) {
                this.allFeedbacks = this.allFeedbacks.filter(f => f.feedbackType === typeFilter);
            }

            // Apply sorting
            this.applySorting();

            // Update stats
            this.updateStats();

            // Display feedbacks
            this.displayFeedbacks(this.allFeedbacks);
        } catch (error) {
            console.error('Error loading feedbacks:', error);
            this.showError('Failed to load feedbacks. Please try again.');
        } finally {
            if (loading) loading.classList.add('d-none');
        }
    },

    applySorting() {
        const sortOrder = document.getElementById('sortOrder')?.value || 'newest';

        switch (sortOrder) {
            case 'oldest':
                this.allFeedbacks.sort((a, b) => new Date(a.createdAt) - new Date(b.createdAt));
                break;
            case 'newest':
            default:
                this.allFeedbacks.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
                break;
        }
    },

    updateStats() {
        const positive = this.allFeedbacks.filter(f => f.feedbackType === 'positive').length;
        const neutral = this.allFeedbacks.filter(f => f.feedbackType === 'neutral').length;
        const negative = this.allFeedbacks.filter(f => f.feedbackType === 'negative').length;

        document.getElementById('positiveCount').textContent = positive;
        document.getElementById('neutralCount').textContent = neutral;
        document.getElementById('negativeCount').textContent = negative;
    },

    displayFeedbacks(feedbacks) {
        const container = document.getElementById('feedbacksContainer');
        if (!container) return;

        container.innerHTML = '';

        if (!feedbacks || feedbacks.length === 0) {
            container.innerHTML = `
                <div class="empty-state">
                    <i class="fas fa-comment-slash"></i>
                    <h3>No Feedback Yet</h3>
                    <p class="text-muted">You haven't left any feedback yet.</p>
                    <a href="order-history.html" class="btn btn-view-detail mt-3">
                        <i class="fas fa-shopping-bag me-2"></i>View My Orders
                    </a>
                </div>
            `;
            return;
        }

        feedbacks.forEach(feedback => {
            const feedbackCard = this.createFeedbackCard(feedback);
            container.innerHTML += feedbackCard;
        });
    },

    createFeedbackCard(feedback) {
        const createdDate = new Date(feedback.createdAt).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });

        const daysSince = Math.floor((new Date() - new Date(feedback.createdAt)) / (1000 * 60 * 60 * 24));
        const canEdit = daysSince <= 60;

        const typeClass = this.getTypeClass(feedback.feedbackType);
        const stars = this.renderStars;

        return `
            <div class="feedback-card">
                <div class="feedback-header">
                    <div class="feedback-seller-info">
                        <div class="feedback-seller-name">
                            Feedback for 
                            <a href="seller-feedback.html?sellerId=${feedback.sellerId}">
                                ${feedback.sellerUsername || 'Unknown Seller'}
                            </a>
                        </div>
                        <div class="feedback-meta">
                            <i class="fas fa-box"></i>Order #${feedback.orderNumber || feedback.orderId} • 
                            <i class="fas fa-calendar"></i>${createdDate}
                            ${feedback.isEdited ? '<span class="badge bg-secondary ms-2">Edited</span>' : ''}
                        </div>
                    </div>
                    <span class="feedback-type-badge ${typeClass}">
                        ${feedback.feedbackType.charAt(0).toUpperCase() + feedback.feedbackType.slice(1)}
                    </span>
                </div>
                
                ${feedback.comment ? `
                    <div class="feedback-comment">${this.escapeHtml(feedback.comment)}</div>
                ` : ''}
                
                <div class="feedback-ratings-detail">
                    <div class="feedback-rating-item">
                        <span class="label">Item Description</span>
                        <span class="value">${stars(feedback.itemDescriptionRating)}</span>
                    </div>
                    <div class="feedback-rating-item">
                        <span class="label">Communication</span>
                        <span class="value">${stars(feedback.communicationRating)}</span>
                    </div>
                    <div class="feedback-rating-item">
                        <span class="label">Shipping Speed</span>
                        <span class="value">${stars(feedback.shippingSpeedRating)}</span>
                    </div>
                    <div class="feedback-rating-item">
                        <span class="label">Shipping Cost</span>
                        <span class="value">${stars(feedback.shippingCostRating)}</span>
                    </div>
                </div>
                
                <div class="feedback-actions">
                    ${canEdit ? `
                        <button onclick="MyFeedbacks.openEditModal(${feedback.id})" class="btn btn-edit">
                            <i class="fas fa-edit me-2"></i>Edit
                        </button>
                        <button onclick="MyFeedbacks.deleteFeedback(${feedback.id})" class="btn btn-delete">
                            <i class="fas fa-trash me-2"></i>Delete
                        </button>
                        <div class="edit-within-notice">
                            <i class="fas fa-info-circle"></i>Can edit within 60 days (${60 - daysSince} days left)
                        </div>
                    ` : `
                        <div class="cannot-edit-notice">
                            <i class="fas fa-lock"></i>Cannot edit (more than 60 days old)
                        </div>
                    `}
                </div>
            </div>
        `;
    },

    renderStars(rating, maxStars = 5) {
        const fullStars = Math.floor(rating);
        const emptyStars = maxStars - fullStars;
        return '★'.repeat(fullStars) + '☆'.repeat(emptyStars);
    },

    getTypeClass(type) {
        return type; // Will use: positive, neutral, negative
    },

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    openEditModal(feedbackId) {
        const feedback = this.allFeedbacks.find(f => f.id === feedbackId);
        if (!feedback) return;

        // Populate form
        document.getElementById('editFeedbackId').value = feedbackId;
        document.getElementById('editComment').value = feedback.comment || '';
        document.getElementById('editCommentCount').textContent = (feedback.comment || '').length;

        // Set feedback type
        document.querySelectorAll('#editFeedbackModal .feedback-type-btn').forEach(btn => {
            btn.classList.remove('active');
            if (btn.dataset.type === feedback.feedbackType) {
                btn.classList.add('active');
            }
        });
        document.getElementById('editFeedbackType').value = feedback.feedbackType;

        // Set ratings
        this.editRatings = {
            editItemDescRating: feedback.itemDescriptionRating,
            editCommRating: feedback.communicationRating,
            editShipSpeedRating: feedback.shippingSpeedRating,
            editShipCostRating: feedback.shippingCostRating
        };

        // Update star displays
        Object.keys(this.editRatings).forEach(key => {
            const container = document.querySelector(`[data-rating-name="${key}"]`);
            if (container) {
                const stars = container.querySelectorAll('.star');
                stars.forEach((star, idx) => {
                    if (idx < this.editRatings[key]) {
                        star.classList.add('active');
                    } else {
                        star.classList.remove('active');
                    }
                });
            }
        });

        // Show modal
        this.editModal.show();
    },

    async saveEdit() {
        const feedbackId = parseInt(document.getElementById('editFeedbackId').value);
        const feedbackType = document.getElementById('editFeedbackType').value;
        const comment = document.getElementById('editComment').value;

        if (!feedbackType) {
            this.showToast('Please select feedback type', 'error');
            return;
        }

        const updateData = {
            feedbackType: feedbackType,
            comment: comment || null,
            itemDescriptionRating: this.editRatings.editItemDescRating || null,
            communicationRating: this.editRatings.editCommRating || null,
            shippingSpeedRating: this.editRatings.editShipSpeedRating || null,
            shippingCostRating: this.editRatings.editShipCostRating || null
        };

        try {
            const token = localStorage.getItem('accessToken');
            const response = await fetch(`https://ebay.dokkuhai.dpdns.org/api/feedback/${feedbackId}`, {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(updateData)
            });

            if (!response.ok) {
                throw new Error('Failed to update feedback');
            }

            this.showToast('Feedback updated successfully!', 'success');
            this.editModal.hide();
            this.loadFeedbacks();
        } catch (error) {
            console.error('Error updating feedback:', error);
            this.showToast('Failed to update feedback', 'error');
        }
    },

    async deleteFeedback(feedbackId) {
        if (!confirm('Are you sure you want to delete this feedback? This action cannot be undone.')) {
            return;
        }

        try {
            const token = localStorage.getItem('accessToken');
            const response = await fetch(`https://ebay.dokkuhai.dpdns.org/api/feedback/${feedbackId}`, {
                method: 'DELETE',
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (!response.ok) {
                throw new Error('Failed to delete feedback');
            }

            this.showToast('Feedback deleted successfully', 'success');
            this.loadFeedbacks();
        } catch (error) {
            console.error('Error deleting feedback:', error);
            this.showToast('Failed to delete feedback', 'error');
        }
    },

    showError(message) {
        const container = document.getElementById('feedbacksContainer');
        if (!container) return;

        container.innerHTML = `
            <div class="alert alert-danger" role="alert">
                <i class="fas fa-exclamation-triangle me-2"></i>${message}
            </div>
        `;
    },

    showToast(message, type = 'info') {
        // Simple toast notification
        const toast = document.createElement('div');
        toast.className = `alert alert-${type === 'success' ? 'success' : 'danger'} position-fixed`;
        toast.style.top = '20px';
        toast.style.right = '20px';
        toast.style.zIndex = '9999';
        toast.innerHTML = `<i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'} me-2"></i>${message}`;
        
        document.body.appendChild(toast);
        
        setTimeout(() => {
            toast.remove();
        }, 3000);
    }
};

// Initialize on page load
document.addEventListener('DOMContentLoaded', () => {
    if (document.getElementById('feedbacksContainer')) {
        MyFeedbacks.init();
    }
});
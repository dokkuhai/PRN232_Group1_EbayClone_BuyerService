// ============================================================
// STAR RATING COMPONENT
// ============================================================

/**
 * Initialize star rating inputs
 * @param {string} containerSelector - CSS selector for rating containers
 * @param {object} ratingsObject - Object to store rating values
 */
function initStarRatings(containerSelector = '.star-rating-input', ratingsObject = {}) {
    document.querySelectorAll(containerSelector).forEach(ratingGroup => {
        const stars = ratingGroup.querySelectorAll('.star');
        const ratingName = ratingGroup.dataset.ratingName;
        
        // Initialize rating value
        if (!ratingsObject[ratingName]) {
            ratingsObject[ratingName] = 0;
        }
        
        stars.forEach(star => {
            // Hover effect
            star.addEventListener('mouseenter', function() {
                const value = parseInt(this.dataset.value);
                highlightStars(stars, value);
            });
            
            // Click to select
            star.addEventListener('click', function() {
                const value = parseInt(this.dataset.value);
                ratingsObject[ratingName] = value;
                
                // Update hidden input if exists
                const hiddenInput = document.getElementById(ratingName);
                if (hiddenInput) {
                    hiddenInput.value = value;
                }
                
                // Keep selected stars active
                highlightStars(stars, value);
                
                // Trigger custom event
                const event = new CustomEvent('ratingChange', {
                    detail: { name: ratingName, value: value }
                });
                ratingGroup.dispatchEvent(event);
            });
        });
        
        // Reset on mouse leave to show selected rating
        ratingGroup.addEventListener('mouseleave', function() {
            const selectedValue = ratingsObject[ratingName];
            highlightStars(stars, selectedValue);
        });
    });
}

/**
 * Highlight stars up to specified value
 * @param {NodeList} stars - Star elements
 * @param {number} value - Number of stars to highlight
 */
function highlightStars(stars, value) {
    stars.forEach((star, idx) => {
        if (idx < value) {
            star.classList.add('active');
        } else {
            star.classList.remove('active');
        }
    });
}

/**
 * Set star rating value programmatically
 * @param {string} ratingName - Name of rating input
 * @param {number} value - Rating value (1-5)
 */
function setStarRating(ratingName, value) {
    const container = document.querySelector(`[data-rating-name="${ratingName}"]`);
    if (!container) return;
    
    const stars = container.querySelectorAll('.star');
    highlightStars(stars, value);
    
    const hiddenInput = document.getElementById(ratingName);
    if (hiddenInput) {
        hiddenInput.value = value;
    }
}

// ============================================================
// FEEDBACK TYPE SELECTOR COMPONENT
// ============================================================

/**
 * Initialize feedback type selector buttons
 * @param {string} containerSelector - CSS selector for button container
 * @param {function} onChange - Callback when type changes
 */
function initFeedbackTypeSelector(containerSelector = '.feedback-type-selector', onChange = null) {
    const container = document.querySelector(containerSelector);
    if (!container) return;
    
    const buttons = container.querySelectorAll('.feedback-type-btn');
    let selectedType = null;
    
    buttons.forEach(btn => {
        btn.addEventListener('click', function() {
            // Remove active from all
            buttons.forEach(b => b.classList.remove('active'));
            
            // Add active to clicked
            this.classList.add('active');
            selectedType = this.dataset.type;
            
            // Update hidden input if exists
            const hiddenInput = document.getElementById('feedbackType');
            if (hiddenInput) {
                hiddenInput.value = selectedType;
            }
            
            // Trigger callback
            if (onChange && typeof onChange === 'function') {
                onChange(selectedType);
            }
            
            // Trigger custom event
            const event = new CustomEvent('feedbackTypeChange', {
                detail: { type: selectedType }
            });
            container.dispatchEvent(event);
        });
    });
    
    return {
        getSelectedType: () => selectedType,
        setSelectedType: (type) => {
            const btn = container.querySelector(`[data-type="${type}"]`);
            if (btn) btn.click();
        }
    };
}

// ============================================================
// FEEDBACK CARD RENDERER
// ============================================================

/**
 * Render a single feedback card
 * @param {object} feedback - Feedback data
 * @param {object} options - Rendering options
 * @returns {string} HTML string
 */
function renderFeedbackCard(feedback, options = {}) {
    const {
        showProduct = false,
        showSeller = true,
        showActions = false,
        onEdit = null,
        onDelete = null
    } = options;
    
    const daysSince = Math.floor((new Date() - new Date(feedback.createdAt)) / (1000 * 60 * 60 * 24));
    const canEdit = daysSince <= 60;
    
    return `
        <div class="feedback-item" data-feedback-id="${feedback.id}">
            <div class="feedback-header">
                <div class="feedback-buyer-info">
                    <img src="${feedback.buyerAvatar || 'https://via.placeholder.com/48'}" 
                         alt="${feedback.buyerUsername}" 
                         class="feedback-avatar">
                    <div>
                        <div class="feedback-buyer-name">${feedback.buyerUsername}</div>
                        <div class="feedback-date">${formatDate(feedback.createdAt)}</div>
                        ${feedback.isVerifiedPurchase ? '<span class="badge bg-success"><i class="bi bi-check-circle"></i> Verified</span>' : ''}
                    </div>
                </div>
                <span class="feedback-badge ${feedback.feedbackType}">
                    ${feedback.feedbackType.charAt(0).toUpperCase() + feedback.feedbackType.slice(1)}
                </span>
            </div>
            
            ${showProduct ? `
                <div class="mt-2">
                    <a href="product-detail.html?id=${feedback.productId}" class="product-link">
                        ${feedback.productTitle}
                    </a>
                </div>
            ` : ''}
            
            ${showSeller ? `
                <div class="mt-2 text-muted small">
                    Seller: <a href="seller-feedback.html?sellerId=${feedback.sellerId}" class="product-link">
                        ${feedback.sellerUsername}
                    </a>
                </div>
            ` : ''}
            
            ${feedback.comment ? `
                <div class="feedback-comment">${escapeHtml(feedback.comment)}</div>
            ` : ''}
            
            <div class="feedback-ratings-detail">
                <div class="feedback-rating-item">
                    <span class="label">Item Description</span>
                    <span class="value">${renderStars(feedback.itemDescriptionRating)}</span>
                </div>
                <div class="feedback-rating-item">
                    <span class="label">Communication</span>
                    <span class="value">${renderStars(feedback.communicationRating)}</span>
                </div>
                <div class="feedback-rating-item">
                    <span class="label">Shipping Speed</span>
                    <span class="value">${renderStars(feedback.shippingSpeedRating)}</span>
                </div>
                <div class="feedback-rating-item">
                    <span class="label">Shipping Cost</span>
                    <span class="value">${renderStars(feedback.shippingCostRating)}</span>
                </div>
            </div>
            
            ${feedback.isEdited ? `
                <div class="mt-2">
                    <span class="badge bg-secondary">
                        <i class="bi bi-pencil"></i> Edited
                    </span>
                </div>
            ` : ''}
            
            ${showActions ? `
                <div class="feedback-actions">
                    ${canEdit ? `
                        <button class="btn btn-outline-primary btn-sm edit-feedback-btn" 
                                data-feedback-id="${feedback.id}">
                            <i class="bi bi-pencil"></i> Edit
                        </button>
                        <button class="btn btn-outline-danger btn-sm delete-feedback-btn" 
                                data-feedback-id="${feedback.id}">
                            <i class="bi bi-trash"></i> Delete
                        </button>
                        <div class="edit-within-60">
                            <i class="bi bi-info-circle"></i> ${60 - daysSince} days left to edit
                        </div>
                    ` : `
                        <div class="cannot-edit">
                            <i class="bi bi-lock"></i> Cannot edit (more than 60 days old)
                        </div>
                    `}
                </div>
            ` : ''}
        </div>
    `;
}

/**
 * Render multiple feedback cards
 * @param {array} feedbacks - Array of feedback objects
 * @param {string} containerId - Container element ID
 * @param {object} options - Rendering options
 */
function renderFeedbackList(feedbacks, containerId, options = {}) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    if (!feedbacks || feedbacks.length === 0) {
        container.innerHTML = `
            <div class="empty-state">
                <i class="bi bi-inbox"></i>
                <h5>No Feedback Yet</h5>
                <p>${options.emptyMessage || 'No feedback to display.'}</p>
            </div>
        `;
        return;
    }
    
    container.innerHTML = feedbacks.map(feedback => 
        renderFeedbackCard(feedback, options)
    ).join('');
    
    // Attach event listeners if actions enabled
    if (options.showActions) {
        container.querySelectorAll('.edit-feedback-btn').forEach(btn => {
            btn.addEventListener('click', function() {
                const feedbackId = parseInt(this.dataset.feedbackId);
                if (options.onEdit) options.onEdit(feedbackId);
            });
        });
        
        container.querySelectorAll('.delete-feedback-btn').forEach(btn => {
            btn.addEventListener('click', function() {
                const feedbackId = parseInt(this.dataset.feedbackId);
                if (options.onDelete) options.onDelete(feedbackId);
            });
        });
    }
}

// ============================================================
// FEEDBACK STATS RENDERER
// ============================================================

/**
 * Render feedback statistics
 * @param {object} stats - FeedbackStatsDto
 * @param {string} containerId - Container element ID
 */
function renderFeedbackStats(stats, containerId) {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    const avgOverall = (
        stats.avgItemDescription + 
        stats.avgCommunication + 
        stats.avgShippingSpeed + 
        stats.avgShippingCost
    ) / 4;
    
    container.innerHTML = `
        <div class="row">
            <div class="col-lg-4">
                <div class="feedback-stats-card">
                    <div class="feedback-overall">
                        <p class="feedback-score">${avgOverall.toFixed(1)}</p>
                        <div class="feedback-stars">${renderStars(Math.round(avgOverall))}</div>
                        <p class="feedback-count">${stats.totalFeedbacks} total feedbacks</p>
                    </div>
                </div>
            </div>
            
            <div class="col-lg-8">
                <div class="feedback-stats-card">
                    <div class="rating-distribution">
                        <h5 class="mb-3">Rating Distribution</h5>
                        ${renderRatingBar('Positive', stats.positiveCount, stats.totalFeedbacks, '#16c79a')}
                        ${renderRatingBar('Neutral', stats.neutralCount, stats.totalFeedbacks, '#ffa726')}
                        ${renderRatingBar('Negative', stats.negativeCount, stats.totalFeedbacks, '#ef5350')}
                    </div>
                    
                    <div class="detailed-ratings">
                        ${renderDetailedRating('Item Description', stats.avgItemDescription)}
                        ${renderDetailedRating('Communication', stats.avgCommunication)}
                        ${renderDetailedRating('Shipping Speed', stats.avgShippingSpeed)}
                        ${renderDetailedRating('Shipping Cost', stats.avgShippingCost)}
                    </div>
                </div>
            </div>
        </div>
    `;
}

/**
 * Render a single rating bar
 * @param {string} label 
 * @param {number} count 
 * @param {number} total 
 * @param {string} color 
 * @returns {string}
 */
function renderRatingBar(label, count, total, color) {
    const percentage = total > 0 ? (count / total * 100) : 0;
    return `
        <div class="rating-bar-row">
            <span class="star-label">${label}</span>
            <div class="rating-bar-container">
                <div class="rating-bar-fill" style="width: ${percentage}%; background: ${color};"></div>
            </div>
            <span class="count">${count}</span>
        </div>
    `;
}

/**
 * Render a detailed rating item
 * @param {string} label 
 * @param {number} value 
 * @returns {string}
 */
function renderDetailedRating(label, value) {
    return `
        <div class="rating-item">
            <div class="label">${label}</div>
            <div class="value">${value.toFixed(1)}</div>
            <div class="stars">${renderStars(Math.round(value))}</div>
        </div>
    `;
}

// ============================================================
// LOADING STATES
// ============================================================

/**
 * Show loading spinner in container
 * @param {string} containerId 
 * @param {string} message 
 */
function showLoading(containerId, message = 'Loading...') {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    container.innerHTML = `
        <div class="text-center py-5">
            <div class="loading-spinner"></div>
            <p class="text-muted mt-3">${message}</p>
        </div>
    `;
}

/**
 * Show error message in container
 * @param {string} containerId 
 * @param {string} message 
 */
function showError(containerId, message = 'Something went wrong') {
    const container = document.getElementById(containerId);
    if (!container) return;
    
    container.innerHTML = `
        <div class="empty-state">
            <i class="bi bi-exclamation-triangle text-danger"></i>
            <h5>Error</h5>
            <p>${message}</p>
            <button class="btn btn-primary mt-3" onclick="location.reload()">
                Try Again
            </button>
        </div>
    `;
}

// ============================================================
// FORM VALIDATION
// ============================================================

/**
 * Validate feedback form
 * @param {object} data - Form data
 * @returns {object} { valid: boolean, errors: array }
 */
function validateFeedbackForm(data) {
    const errors = [];
    
    // Validate feedback type
    if (!data.feedbackType || !['positive', 'neutral', 'negative'].includes(data.feedbackType)) {
        errors.push('Please select your experience type (Positive/Neutral/Negative)');
    }
    
    // Validate ratings
    const requiredRatings = [
        'itemDescriptionRating',
        'communicationRating',
        'shippingSpeedRating',
        'shippingCostRating'
    ];
    
    requiredRatings.forEach(rating => {
        const value = parseInt(data[rating]);
        if (!value || value < 1 || value > 5) {
            errors.push(`Please rate ${rating.replace('Rating', '').replace(/([A-Z])/g, ' $1').toLowerCase()}`);
        }
    });
    
    // Validate comment length
    if (data.comment && data.comment.length > 500) {
        errors.push('Comment cannot exceed 500 characters');
    }
    
    return {
        valid: errors.length === 0,
        errors: errors
    };
}

// ============================================================
// UTILITY FUNCTIONS
// ============================================================

/**
 * Escape HTML to prevent XSS
 * @param {string} text 
 * @returns {string}
 */
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

/**
 * Truncate text to specified length
 * @param {string} text 
 * @param {number} maxLength 
 * @returns {string}
 */
function truncateText(text, maxLength = 100) {
    if (!text || text.length <= maxLength) return text;
    return text.substring(0, maxLength) + '...';
}

/**
 * Get feedback type color
 * @param {string} type - positive/neutral/negative
 * @returns {string} color code
 */
function getFeedbackTypeColor(type) {
    const colors = {
        positive: '#16c79a',
        neutral: '#ffa726',
        negative: '#ef5350'
    };
    return colors[type] || '#707a84';
}

/**
 * Get feedback type icon
 * @param {string} type 
 * @returns {string} Bootstrap icon class
 */
function getFeedbackTypeIcon(type) {
    const icons = {
        positive: 'bi-emoji-smile',
        neutral: 'bi-emoji-neutral',
        negative: 'bi-emoji-frown'
    };
    return icons[type] || 'bi-chat-left-text';
}

// ============================================================
// CONFIRMATION DIALOGS
// ============================================================

/**
 * Show confirmation dialog
 * @param {string} message 
 * @param {function} onConfirm 
 * @param {function} onCancel 
 */
function showConfirmDialog(message, onConfirm, onCancel = null) {
    const confirmed = confirm(message);
    if (confirmed && onConfirm) {
        onConfirm();
    } else if (!confirmed && onCancel) {
        onCancel();
    }
    return confirmed;
}

/**
 * Show delete confirmation with feedback details
 * @param {object} feedback 
 * @param {function} onConfirm 
 */
function confirmDeleteFeedback(feedback, onConfirm) {
    const message = `Are you sure you want to delete your ${feedback.feedbackType} feedback for ${feedback.sellerUsername}?\n\nThis action cannot be undone.`;
    return showConfirmDialog(message, onConfirm);
}

// ============================================================
// EXPORT FOR USE IN OTHER SCRIPTS
// ============================================================

// Make functions available globally
window.FeedbackUI = {
    // Components
    initStarRatings,
    initFeedbackTypeSelector,
    setStarRating,
    
    // Renderers
    renderFeedbackCard,
    renderFeedbackList,
    renderFeedbackStats,
    
    // Loading states
    showLoading,
    showError,
    
    // Validation
    validateFeedbackForm,
    
    // Utilities
    escapeHtml,
    truncateText,
    getFeedbackTypeColor,
    getFeedbackTypeIcon,
    
    // Dialogs
    showConfirmDialog,
    confirmDeleteFeedback
};

console.log('Feedback UI loaded successfully');
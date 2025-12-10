// Event Management System - Client-side JavaScript

document.addEventListener('DOMContentLoaded', function () {
    initializeNavigation();
    initializeEventListeners();
    initializeSearch();
    initializeAlertDismissal();
});

// ============= NAVIGATION =============
function initializeNavigation() {
    const hamburger = document.getElementById('hamburger');
    const navMenu = document.getElementById('navMenu');

    if (hamburger) {
        hamburger.addEventListener('click', function () {
            navMenu.classList.toggle('active');
        });

        // Close menu when link is clicked
        navMenu.querySelectorAll('a').forEach(link => {
            link.addEventListener('click', function (e) {
                // Don't close for dropdown toggles
                if (!this.classList.contains('dropdown-toggle')) {
                    navMenu.classList.remove('active');
                }
            });
        });

        // Close menu when clicking outside
        document.addEventListener('click', function (e) {
            if (!e.target.closest('.navbar')) {
                navMenu.classList.remove('active');
            }
        });
    }

    // Handle dropdown toggles on mobile
    const dropdownToggles = document.querySelectorAll('.dropdown-toggle');
    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            const dropdown = this.closest('.dropdown');
            dropdown.classList.toggle('active');
        });
    });
}

// ============= FORM VALIDATION =============
function initializeEventListeners() {
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', validateForm);
    });

    // Add smooth scrolling
    const links = document.querySelectorAll('a[href^="#"]');
    links.forEach(link => {
        link.addEventListener('click', smoothScroll);
    });
}

function validateForm(event) {
    const form = event.target;
    const inputs = form.querySelectorAll('input[required], textarea[required], select[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.value.trim()) {
            isValid = false;
            input.classList.add('error');
            addErrorState(input);
        } else {
            input.classList.remove('error');
            removeErrorState(input);
        }
    });

    if (!isValid) {
        event.preventDefault();
        scrollToTop();
    }
}

function addErrorState(input) {
    input.style.borderColor = '#e74c3c';
    input.style.boxShadow = '0 0 5px rgba(231, 76, 60, 0.3)';
}

function removeErrorState(input) {
    input.style.borderColor = '#ddd';
    input.style.boxShadow = 'none';
}

// ============= SEARCH FUNCTIONALITY =============
function initializeSearch() {
    const searchInput = document.getElementById('searchInput');
    if (!searchInput) return;

    searchInput.addEventListener('input', filterEvents);

    const filterButtons = document.querySelectorAll('.filter-btn');
    filterButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            // Update active state
            filterButtons.forEach(b => b.classList.remove('active'));
            this.classList.add('active');
            filterEvents();
        });
    });
}

function filterEvents() {
    const searchInput = document.getElementById('searchInput');
    const searchTerm = searchInput ? searchInput.value.toLowerCase() : '';
    const cards = document.querySelectorAll('.event-card');

    cards.forEach(card => {
        const title = card.dataset.title.toLowerCase();
        const location = card.dataset.location.toLowerCase();
        const category = card.dataset.category.toLowerCase();

        const matchesSearch = title.includes(searchTerm) || location.includes(searchTerm);

        if (matchesSearch) {
            card.style.display = 'block';
        } else {
            card.style.display = 'none';
        }
    });
}

// ============= ALERT DISMISSAL =============
function initializeAlertDismissal() {
    const closeButtons = document.querySelectorAll('[data-dismiss="alert"]');
    closeButtons.forEach(btn => {
        btn.addEventListener('click', function () {
            const alert = this.closest('.alert');
            if (alert) {
                alert.style.display = 'none';
            }
        });
    });
}

// ============= UTILITIES =============
function smoothScroll(event) {
    const href = this.getAttribute('href');
    if (href.startsWith('#')) {
        event.preventDefault();
        const target = document.querySelector(href);
        if (target) {
            target.scrollIntoView({ behavior: 'smooth' });
        }
    }
}

function formatDate(dateString) {
    const options = { year: 'numeric', month: 'long', day: 'numeric' };
    return new Date(dateString).toLocaleDateString('en-US', options);
}

function formatTime(dateString) {
    const options = { hour: '2-digit', minute: '2-digit' };
    return new Date(dateString).toLocaleTimeString('en-US', options);
}

function scrollToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
}

function showNotification(message, type = 'success') {
    const notification = document.createElement('div');
    notification.className = `notification notification-${type}`;
    notification.innerHTML = `
        <i class="fas fa-${type === 'success' ? 'check-circle' : 'exclamation-circle'}"></i>
        <span>${message}</span>
    `;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 1rem 1.5rem;
        background-color: ${type === 'success' ? '#27ae60' : '#e74c3c'};
        color: white;
        border-radius: 4px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
        z-index: 1000;
        animation: slideIn 0.3s ease-in-out;
        display: flex;
        align-items: center;
        gap: 0.75rem;
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 3000);
}

function getQueryParam(param) {
    const urlParams = new URLSearchParams(window.location.search);
    return urlParams.get(param);
}

function logEvent(eventName, data = {}) {
    console.log(`Event: ${eventName}`, data);
}

// ============= EVENT CARD EFFECTS =============
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            opacity: 0;
            transform: translateX(100px);
        }
        to {
            opacity: 1;
            transform: translateX(0);
        }
    }

    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @keyframes pulse {
        0%, 100% {
            opacity: 1;
        }
        50% {
            opacity: 0.5;
        }
    }

    @keyframes bounce {
        0%, 100% {
            transform: translateY(0);
        }
        50% {
            transform: translateY(-10px);
        }
    }

    @keyframes shimmer {
        0% {
            background-position: -1000px 0;
        }
        100% {
            background-position: 1000px 0;
        }
    }

    .fade-in-up {
        animation: fadeInUp 0.6s ease-out;
    }

    .bounce-in {
        animation: bounce 0.6s ease-out;
    }

    .card-animate {
        animation: fadeInUp 0.5s ease-out;
    }
`;
document.head.appendChild(style);

// ============= INTERSECTION OBSERVER FOR SCROLL ANIMATIONS =============
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver(function(entries) {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.classList.add('fade-in-up');
            observer.unobserve(entry.target);
        }
    });
}, observerOptions);

document.addEventListener('DOMContentLoaded', function() {
    // Observe all feature cards, event cards, and testimonials
    document.querySelectorAll('.feature-card, .event-card-mini, .testimonial-card').forEach(el => {
        observer.observe(el);
    });
});

/**
 * Dominicus Chat Application
 * Organized following SOLID principles:
 * - Single Responsibility: Each class has one job
 * - Open/Closed: Extendable without modification
 * - Liskov Substitution: Subtypes can be substituted for their base types
 * - Interface Segregation: Many specific interfaces better than one general
 * - Dependency Inversion: High-level modules don't depend on low-level modules
 */

// DOM Ready event handler
document.addEventListener('DOMContentLoaded', () => {
    // Initialize chat application
    const chatApp = new ChatApplication();
    chatApp.initialize();
});

/**
 * Main Chat Application Class
 * Coordinates the different components of the chat system
 */
class ChatApplication {
    constructor() {
        // UI Controllers
        this.uiController = new ChatUIController();
        
        // Service controllers
        this.messageService = new MessageService();
        
        // Event handlers
        this.eventHandler = new ChatEventHandler(this.uiController, this.messageService);
    }
    
    /**
     * Initialize the chat application components
     */
    initialize() {
        // Initialize UI components
        this.uiController.initialize();
        
        // Setup event listeners
        this.eventHandler.setupEventListeners();
        
        // Perform initial UI adjustments
        this.uiController.scrollToBottom();
        this.uiController.resizeTextarea();
        
        // Check if we're coming back from a form submission
        if (window.location.href.includes('?handler=') || 
            document.referrer.includes('?handler=')) {
            this.uiController.highlightLastMessage();
        }
    }
}

/**
 * Controller for Chat UI Elements
 * Handles all DOM manipulation and UI updates
 */
class ChatUIController {
    constructor() {
        this.elements = {};
    }
    
    /**
     * Initialize UI elements and templates
     */
    initialize() {
        this.elements = {
            chatMessages: document.getElementById('chatMessages'),
            questionForm: document.getElementById('questionForm'),
            textarea: document.getElementById('Question'),
            sendButton: document.querySelector('.send-button'),
            userMessageTemplate: document.getElementById('user-message-template'),
            botMessageTemplate: document.getElementById('bot-message-template'),
            typingIndicatorTemplate: document.getElementById('typing-indicator-template'),
            suggestedQuestions: document.querySelectorAll('.suggested-question')
        };
        
        // First render adjustments
        this.forceRepaint();
    }
    
    /**
     * Force a repaint of the chat messages
     */
    forceRepaint() {
        if (!this.elements.chatMessages) return;
        
        this.elements.chatMessages.style.display = 'none';
        setTimeout(() => {
            this.elements.chatMessages.style.display = 'block';
            this.scrollToBottom();
        }, 50);
    }
    
    /**
     * Scroll chat window to bottom
     */
    scrollToBottom() {
        if (!this.elements.chatMessages) return;
        this.elements.chatMessages.scrollTop = this.elements.chatMessages.scrollHeight;
    }
    
    /**
     * Add a user message to the chat window
     * @param {string} question - The question to add
     */
    addUserMessage(question) {
        const messageElement = this.elements.userMessageTemplate.content.cloneNode(true);
        const questionElement = messageElement.querySelector('.question');
        questionElement.textContent = question;
        
        this.elements.chatMessages.appendChild(messageElement);
        this.scrollToBottom();
    }
    
    /**
     * Add a bot message to the chat window
     * @param {object} response - The response object containing englishResponse and dominicanResponse
     */
    addBotMessage(response) {
        const messageElement = this.elements.botMessageTemplate.content.cloneNode(true);
        const englishElement = messageElement.querySelector('.english-response div');
        const dominicanElement = messageElement.querySelector('.dominican-response div');
        
        englishElement.textContent = response.englishResponse;
        dominicanElement.textContent = response.dominicanResponse;
        
        this.removeTypingIndicator();
        this.elements.chatMessages.appendChild(messageElement);
        this.scrollToBottom();
    }
    
    /**
     * Add typing indicator to show the bot is "typing"
     */
    addTypingIndicator() {
        if (!this.elements.chatMessages || !this.elements.typingIndicatorTemplate) return;
        
        // Remove any existing indicators first
        this.removeTypingIndicator();
        
        const typingIndicator = this.elements.typingIndicatorTemplate.content.cloneNode(true);
        this.elements.chatMessages.appendChild(typingIndicator);
        this.scrollToBottom();
    }
    
    /**
     * Remove typing indicator
     */
    removeTypingIndicator() {
        const indicators = document.querySelectorAll('.typing-indicator-container');
        indicators.forEach(indicator => indicator.remove());
    }
    
    /**
     * Toggle loading state of the send button
     * @param {boolean} isLoading - Whether the button should show loading state
     */
    toggleSendButtonLoading(isLoading) {
        if (!this.elements.sendButton) return;
        
        if (isLoading) {
            this.elements.sendButton.classList.add('loading');
            this.elements.sendButton.disabled = true;
        } else {
            this.elements.sendButton.classList.remove('loading');
            this.elements.sendButton.disabled = false;
        }
    }
    
    /**
     * Clear textarea input
     */
    clearInput() {
        if (!this.elements.textarea) return;
        this.elements.textarea.value = '';
        this.resizeTextarea();
    }
    
    /**
     * Resize textarea to fit content
     */
    resizeTextarea() {
        if (!this.elements.textarea) return;
        
        this.elements.textarea.style.height = 'auto';
        this.elements.textarea.style.height = `${this.elements.textarea.scrollHeight}px`;
    }
    
    /**
     * Highlight the last message (used after page loads)
     */
    highlightLastMessage() {
        // Remove any typing indicators
        this.removeTypingIndicator();
        
        // Highlight last bot message if it exists
        const messages = document.querySelectorAll('.chat-message');
        if (messages.length >= 2) {
            const lastMessage = messages[messages.length - 1];
            if (!lastMessage.classList.contains('user-message')) {
                lastMessage.classList.add('message-highlight');
            }
        }
        
        this.scrollToBottom();
    }
}

/**
 * Message Service
 * Handles communication with the server API
 */
class MessageService {
    constructor() {
        this.apiEndpoint = window.location.pathname + '?handler=Ask';
        this.antiForgeryToken = this.getAntiForgeryToken();
    }
    
    /**
     * Get the anti-forgery token from the page
     * @returns {string} The anti-forgery token
     */
    getAntiForgeryToken() {
        const tokenInput = document.querySelector('form#antiforgery-form input[name="__RequestVerificationToken"]');
        return tokenInput ? tokenInput.value : '';
    }
    
    /**
     * Send a message to the API
     * @param {string} question - The question to send
     * @returns {Promise} A promise that resolves with the response
     */
    async sendMessage(question) {
        try {
            const response = await fetch(this.apiEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': this.antiForgeryToken
                },
                body: JSON.stringify({ question })
            });
            
            let data;
            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                data = await response.json();
            } else {
                const text = await response.text();
                throw new Error(`Server returned non-JSON response: ${text.substring(0, 100)}...`);
            }
            
            if (!response.ok) {
                console.error('Server error response:', data);
                throw new Error(data.error || `Server error: ${response.status}`);
            }
            
            return data;
        } catch (error) {
            console.error('Error sending message:', error);
            throw error;
        }
    }
}

/**
 * Event Handler
 * Manages all event listeners and connections between UI and Services
 */
class ChatEventHandler {
    constructor(uiController, messageService) {
        this.uiController = uiController;
        this.messageService = messageService;
    }
    
    /**
     * Set up all event listeners
     */
    setupEventListeners() {
        // Form submission
        if (this.uiController.elements.questionForm) {
            this.uiController.elements.questionForm.addEventListener('submit', this.handleFormSubmit.bind(this));
        }
        
        // Textarea input for auto-resizing
        if (this.uiController.elements.textarea) {
            this.uiController.elements.textarea.addEventListener('input', () => {
                this.uiController.resizeTextarea();
            });
            
            // Handle Enter key
            this.uiController.elements.textarea.addEventListener('keydown', this.handleKeyPress.bind(this));
        }
        
        // Suggested questions
        if (this.uiController.elements.suggestedQuestions) {
            this.uiController.elements.suggestedQuestions.forEach(button => {
                button.addEventListener('click', this.handleSuggestedQuestion.bind(this));
            });
        }
        
        // Window load event
        window.addEventListener('load', () => {
            this.uiController.scrollToBottom();
        });
    }
    
    /**
     * Handle form submission
     * @param {Event} event - The form submission event
     */
    async handleFormSubmit(event) {
        event.preventDefault();
        
        const form = event.target;
        const textarea = this.uiController.elements.textarea;
        
        if (!form.checkValidity() || !textarea) {
            form.classList.add('was-validated');
            return;
        }
        
        const question = textarea.value.trim();
        if (!question) return;
        
        try {
            // Show loading state
            this.uiController.toggleSendButtonLoading(true);
            
            // Add user message to UI
            this.uiController.addUserMessage(question);
            
            // Show typing indicator
            setTimeout(() => {
                this.uiController.addTypingIndicator();
            }, 500);
            
            // Clear input
            this.uiController.clearInput();
            
            // Send to API
            const result = await this.messageService.sendMessage(question);
            
            // Handle response
            if (result.success) {
                const { englishResponse, dominicanResponse } = result.response;
                
                // Add slight delay to simulate typing
                setTimeout(() => {
                    this.uiController.addBotMessage({ englishResponse, dominicanResponse });
                    this.uiController.toggleSendButtonLoading(false);
                }, 1000);
            } else {
                throw new Error(result.error || 'Failed to get response');
            }
        } catch (error) {
            console.error('Error in submission:', error);
            this.uiController.removeTypingIndicator();
            this.uiController.toggleSendButtonLoading(false);
            
            // Add a bot message showing the error
            this.uiController.addBotMessage({
                englishResponse: "I'm sorry, I couldn't process your request at this time.",
                dominicanResponse: "Lo siento, no pude procesar tu solicitud en este momento."
            });
        }
    }
    
    /**
     * Handle keypress in textarea
     * @param {KeyboardEvent} event - The keypress event
     */
    handleKeyPress(event) {
        // Submit on Enter (without Shift)
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            this.uiController.elements.questionForm.dispatchEvent(new Event('submit'));
        }
    }
    
    /**
     * Handle suggested question click
     * @param {MouseEvent} event - The click event
     */
    handleSuggestedQuestion(event) {
        event.preventDefault();
        const button = event.currentTarget;
        
        if (this.uiController.elements.textarea) {
            this.uiController.elements.textarea.value = button.textContent.trim();
            this.uiController.elements.textarea.focus();
            this.uiController.resizeTextarea();
        }
    }
}

// Helper functions - general utilities
const Utils = {
    /**
     * Check if a value is empty (null, undefined, empty string)
     * @param {*} value - The value to check
     * @returns {boolean} Whether the value is empty
     */
    isEmpty(value) {
        return value === null || value === undefined || value === '';
    },
    
    /**
     * Debounce a function to prevent rapid firing
     * @param {Function} func - The function to debounce
     * @param {number} wait - The time to wait in milliseconds
     * @returns {Function} The debounced function
     */
    debounce(func, wait) {
        let timeout;
        return function(...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }
}; 
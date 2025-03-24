// Mock the global classes from chat.js
// This simulates the classes being available in the global scope

class ChatUIController {
    constructor() {
        this.elements = {};
    }

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
    }

    addUserMessage(question) {
        const messageElement = this.elements.userMessageTemplate.content.cloneNode(true);
        const questionElement = messageElement.querySelector('.question');
        questionElement.textContent = question;
        
        this.elements.chatMessages.appendChild(messageElement);
        this.scrollToBottom();
    }

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

    addTypingIndicator() {
        if (!this.elements.chatMessages || !this.elements.typingIndicatorTemplate) return;
        
        this.removeTypingIndicator();
        
        const typingIndicator = this.elements.typingIndicatorTemplate.content.cloneNode(true);
        this.elements.chatMessages.appendChild(typingIndicator);
        this.scrollToBottom();
    }

    removeTypingIndicator() {
        const existingIndicator = document.querySelector('.typing-indicator-container');
        if (existingIndicator) {
            existingIndicator.remove();
        }
    }

    scrollToBottom() {
        if (this.elements.chatMessages) {
            this.elements.chatMessages.scrollTop = this.elements.chatMessages.scrollHeight;
        }
    }

    toggleSendButtonLoading(isLoading) {
        if (this.elements.sendButton) {
            this.elements.sendButton.disabled = isLoading;
            if (isLoading) {
                this.elements.sendButton.classList.add('loading');
            } else {
                this.elements.sendButton.classList.remove('loading');
            }
        }
    }
}

class MessageService {
    constructor() {
        this.apiEndpoint = window.location.pathname + '?handler=Ask';
    }

    async sendMessage(message) {
        try {
            const token = this.getAntiForgeryToken();
            if (!token) {
                throw new Error('CSRF token not found');
            }

            const response = await fetch(this.apiEndpoint, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': token
                },
                body: JSON.stringify({ question: message })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('Error sending message:', error);
            throw error;
        }
    }

    getAntiForgeryToken() {
        const tokenElement = document.querySelector('#antiforgery-form input[name="__RequestVerificationToken"]');
        return tokenElement ? tokenElement.value : null;
    }
}

class ChatEventHandler {
    constructor(uiController, messageService) {
        this.uiController = uiController;
        this.messageService = messageService;
        this.setupEventListeners();
    }

    setupEventListeners() {
        if (this.uiController.elements.questionForm) {
            this.uiController.elements.questionForm.addEventListener('submit', this.handleFormSubmit.bind(this));
        }

        if (this.uiController.elements.textarea) {
            this.uiController.elements.textarea.addEventListener('keydown', this.handleKeyPress.bind(this));
        }

        if (this.uiController.elements.suggestedQuestions) {
            this.uiController.elements.suggestedQuestions.forEach(button => {
                button.addEventListener('click', () => {
                    this.uiController.elements.textarea.value = button.textContent.trim();
                    this.uiController.elements.questionForm.dispatchEvent(new Event('submit'));
                });
            });
        }
    }

    async handleFormSubmit(event) {
        event.preventDefault();
        
        const question = this.uiController.elements.textarea.value.trim();
        if (!question) return;
        
        try {
            this.uiController.toggleSendButtonLoading(true);
            this.uiController.elements.textarea.value = '';
            
            this.uiController.addUserMessage(question);
            this.uiController.addTypingIndicator();
            
            const response = await this.messageService.sendMessage(question);
            
            setTimeout(() => {
                this.uiController.addBotMessage(response);
                this.uiController.toggleSendButtonLoading(false);
            }, 1000);
            
        } catch (error) {
            console.error('Error in submission:', error);
            this.uiController.removeTypingIndicator();
            this.uiController.toggleSendButtonLoading(false);
            
            this.uiController.addBotMessage({
                englishResponse: "I'm sorry, I couldn't process your request at this time.",
                dominicanResponse: "Lo siento, no pude procesar tu solicitud en este momento."
            });
        }
    }

    handleKeyPress(event) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            this.uiController.elements.questionForm.dispatchEvent(new Event('submit'));
        }
    }
}

module.exports = {
    ChatUIController,
    MessageService,
    ChatEventHandler
};

global.Utils = {
    isEmpty(value) {
        return value === null || value === undefined || value === '';
    },
    
    debounce(func, wait) {
        let timeout;
        return function(...args) {
            clearTimeout(timeout);
            timeout = setTimeout(() => func.apply(this, args), wait);
        };
    }
}; 
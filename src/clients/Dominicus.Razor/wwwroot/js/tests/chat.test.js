/**
 * Chat.js Unit Tests
 * 
 * These tests cover the core functionality of the chat application.
 * To run: npm test
 */

const { ChatUIController, MessageService, ChatEventHandler } = require('./setup');

describe('ChatUIController', () => {
    let chatUIController;
    
    beforeEach(() => {
        // Set up DOM
        document.body.innerHTML = `
            <div id="chatMessages"></div>
            <form id="questionForm">
                <textarea id="Question"></textarea>
                <button type="submit" class="send-button"></button>
            </form>
            <template id="user-message-template">
                <div class="chat-message user-message mb-3 message-animate-in">
                    <div class="message-content">
                        <div class="message-bubble">
                            <div class="question"></div>
                        </div>
                    </div>
                </div>
            </template>
            <template id="bot-message-template">
                <div class="chat-message bot-message mb-3 message-animate-in">
                    <div class="message-content">
                        <div class="bot-avatar">
                            <i class="fas fa-robot"></i>
                        </div>
                        <div class="message-bubble">
                            <div class="english-response">
                                <small>English:</small>
                                <div></div>
                            </div>
                            <div class="dominican-response">
                                <small>Dominican:</small>
                                <div></div>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
            <template id="typing-indicator-template">
                <div class="typing-indicator-container"></div>
            </template>
        `;
        
        // Create a new instance of the controller
        chatUIController = new ChatUIController();
        chatUIController.initialize();
    });
    
    afterEach(() => {
        document.body.innerHTML = '';
        jest.clearAllMocks();
    });
    
    test('initialize() should correctly set up UI elements and templates', () => {
        expect(chatUIController.elements.chatMessages).toBeTruthy();
        expect(chatUIController.elements.questionForm).toBeTruthy();
        expect(chatUIController.elements.textarea).toBeTruthy();
        expect(chatUIController.elements.sendButton).toBeTruthy();
        expect(chatUIController.elements.userMessageTemplate).toBeTruthy();
        expect(chatUIController.elements.botMessageTemplate).toBeTruthy();
        expect(chatUIController.elements.typingIndicatorTemplate).toBeTruthy();
    });
    
    test('addUserMessage() should add a user message to the chat', () => {
        const testMessage = 'Hello, world!';
        chatUIController.addUserMessage(testMessage);
        
        const messageElement = document.querySelector('.user-message .question');
        expect(messageElement).toBeTruthy();
        expect(messageElement.textContent).toBe(testMessage);
    });
    
    test('addBotMessage() should add a bot message with English and Dominican responses', () => {
        const response = {
            englishResponse: 'Hello!',
            dominicanResponse: '¡Hola!'
        };
        
        chatUIController.addBotMessage(response);
        
        const englishElement = document.querySelector('.bot-message .english-response div');
        const dominicanElement = document.querySelector('.bot-message .dominican-response div');
        
        expect(englishElement.textContent).toBe(response.englishResponse);
        expect(dominicanElement.textContent).toBe(response.dominicanResponse);
    });
    
    test('addTypingIndicator() should add a typing indicator to the chat', () => {
        chatUIController.addTypingIndicator();
        const indicator = document.querySelector('.typing-indicator-container');
        expect(indicator).toBeTruthy();
    });
    
    test('removeTypingIndicator() should remove all typing indicators', () => {
        // Add multiple indicators
        chatUIController.addTypingIndicator();
        chatUIController.addTypingIndicator();
        
        // Remove them
        chatUIController.removeTypingIndicator();
        
        const indicators = document.querySelectorAll('.typing-indicator-container');
        expect(indicators.length).toBe(0);
    });
    
    test('toggleSendButtonLoading() should add loading class when true', () => {
        chatUIController.toggleSendButtonLoading(true);
        expect(chatUIController.elements.sendButton.classList.contains('loading')).toBe(true);
        expect(chatUIController.elements.sendButton.disabled).toBe(true);
    });
    
    test('toggleSendButtonLoading() should remove loading class when false', () => {
        chatUIController.elements.sendButton.classList.add('loading');
        chatUIController.elements.sendButton.disabled = true;
        
        chatUIController.toggleSendButtonLoading(false);
        
        expect(chatUIController.elements.sendButton.classList.contains('loading')).toBe(false);
        expect(chatUIController.elements.sendButton.disabled).toBe(false);
    });
});

describe('MessageService', () => {
    let messageService;
    
    beforeEach(() => {
        document.body.innerHTML = `
            <form id="antiforgery-form">
                <input name="__RequestVerificationToken" value="test-token" />
            </form>
        `;
        
        messageService = new MessageService();
        
        // Mock fetch
        global.fetch = jest.fn();
    });
    
    afterEach(() => {
        document.body.innerHTML = '';
        jest.clearAllMocks();
    });
    
    test('getAntiForgeryToken() should return the token value', () => {
        const token = messageService.getAntiForgeryToken();
        expect(token).toBe('test-token');
    });
    
    test('sendMessage() should call fetch with correct parameters', async () => {
        const testMessage = 'Hello';
        const mockResponse = { ok: true, json: () => Promise.resolve({ success: true }) };
        global.fetch.mockResolvedValueOnce(mockResponse);
        
        await messageService.sendMessage(testMessage);
        
        expect(fetch).toHaveBeenCalledWith(
            expect.any(String),
            expect.objectContaining({
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': 'test-token'
                },
                body: JSON.stringify({ question: testMessage })
            })
        );
    });
    
    test('sendMessage() should throw error when response is not OK', async () => {
        const mockResponse = { ok: false, status: 400 };
        global.fetch.mockResolvedValueOnce(mockResponse);
        
        await expect(messageService.sendMessage('test')).rejects.toThrow();
    });
});

describe('Message Display', () => {
    let chatUIController;
    let messageService;
    let chatEventHandler;
    
    beforeEach(() => {
        document.body.innerHTML = `
            <div id="chatMessages"></div>
            <template id="user-message-template">
                <div class="chat-message user-message mb-3 message-animate-in">
                    <div class="message-content">
                        <div class="message-bubble">
                            <div class="question"></div>
                        </div>
                    </div>
                </div>
            </template>
            <template id="bot-message-template">
                <div class="chat-message bot-message mb-3 message-animate-in">
                    <div class="message-content">
                        <div class="bot-avatar">
                            <i class="fas fa-robot"></i>
                        </div>
                        <div class="message-bubble">
                            <div class="english-response">
                                <small>English:</small>
                                <div></div>
                            </div>
                            <div class="dominican-response">
                                <small>Dominican:</small>
                                <div></div>
                            </div>
                        </div>
                    </div>
                </div>
            </template>
        `;
        
        chatUIController = new ChatUIController();
        chatUIController.initialize();
        
        messageService = new MessageService();
        chatEventHandler = new ChatEventHandler(chatUIController, messageService);
    });
    
    afterEach(() => {
        document.body.innerHTML = '';
        jest.clearAllMocks();
    });
    
    test('addUserMessage should create message with correct content', () => {
        const question = 'Test question';
        chatUIController.addUserMessage(question);
        
        const userMessage = document.querySelector('.user-message .question');
        expect(userMessage).not.toBeNull();
        expect(userMessage.textContent).toBe(question);
    });
    
    test('addBotMessage should create message with English and Dominican responses', () => {
        const response = {
            englishResponse: 'English test',
            dominicanResponse: 'Dominican test'
        };
        
        chatUIController.addBotMessage(response);
        
        const englishResponse = document.querySelector('.bot-message .english-response div');
        const dominicanResponse = document.querySelector('.bot-message .dominican-response div');
        
        expect(englishResponse).not.toBeNull();
        expect(dominicanResponse).not.toBeNull();
        expect(englishResponse.textContent).toBe(response.englishResponse);
        expect(dominicanResponse.textContent).toBe(response.dominicanResponse);
    });
    
    test('Messages should have correct styling classes', () => {
        const question = 'Test question';
        chatUIController.addUserMessage(question);
        
        const userMessage = document.querySelector('.user-message');
        expect(userMessage.querySelector('.message-bubble')).not.toBeNull();
        expect(userMessage.classList.contains('message-animate-in')).toBe(true);
    });
    
    test('Bot message should include avatar and correct structure', () => {
        const response = {
            englishResponse: 'English test',
            dominicanResponse: 'Dominican test'
        };
        
        chatUIController.addBotMessage(response);
        
        const botMessage = document.querySelector('.bot-message');
        expect(botMessage.querySelector('.bot-avatar')).not.toBeNull();
        expect(botMessage.querySelector('.bot-avatar i.fas.fa-robot')).not.toBeNull();
        expect(botMessage.querySelector('.message-bubble')).not.toBeNull();
    });
    
    test('Messages should be added in sequence', () => {
        const question = 'Test question';
        const response = {
            englishResponse: 'English test',
            dominicanResponse: 'Dominican test'
        };
        
        chatUIController.addUserMessage(question);
        chatUIController.addBotMessage(response);
        
        const messages = document.querySelectorAll('.chat-message');
        expect(messages.length).toBe(2);
        expect(messages[0].classList.contains('user-message')).toBe(true);
        expect(messages[1].classList.contains('bot-message')).toBe(true);
    });
}); 
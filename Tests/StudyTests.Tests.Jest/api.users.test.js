const https = require('https');
const axios = require('axios');

const BASE_URL = 'http://localhost:5000'; 

const httpsAgent = new https.Agent({
  rejectUnauthorized: false,
});

const api = axios.create({
  baseURL: BASE_URL,
  httpsAgent: httpsAgent,
});

describe('Users API E2E Tests', () => {
    
    let newUserId;
    const testUser = {
        login: `testuser_${Date.now()}`,
        fullName: 'Test User',
        email: `test_${Date.now()}@example.com`,
        password: 'Password123!!!',
        role: 'Student',
        phoneNumber: '+380963173322'
    };

    // Test 1: Create a new user (POST)
    it('should create a new user', async () => {
        try {
            const response = await api.post('/api/users', testUser); 

            expect(response.status).toBe(201);
            expect(response.data.id).toBeGreaterThan(0);
            newUserId = response.data.id;
        } catch (error) {
            console.error('Create user failed:', error.response?.data);
            throw error;
        }
    });

    // Test 2: Get the new user by ID (GET)
    it('should get the created user by ID', async () => {
        expect(newUserId).toBeDefined();

        const response = await api.get(`/api/users/${newUserId}`);
        
        expect(response.status).toBe(200);
        expect(response.data.id).toBe(newUserId);
    });

    // Test 3: Update the user (PUT)
    it('should update the user', async () => {
        const updatedUserData = {
            login: testUser.login,
            fullName: 'Test User Updated',
            email: testUser.email,
            phoneNumber: testUser.phoneNumber,
            role: 'Teacher'
        };

        const response = await api.put(`/api/users/${newUserId}`, updatedUserData);

        expect(response.status).toBe(204);

        const verifyResponse = await api.get(`/api/users/${newUserId}`);
        
        expect(verifyResponse.status).toBe(200);
        expect(verifyResponse.data.fullName).toBe('Test User Updated');
        expect(verifyResponse.data.roles).toContain('Teacher');
    });

    // Test 4: Delete the user (DELETE)
    it('should delete the user', async () => {
        const response = await api.delete(`/api/users/${newUserId}`);
        
        expect(response.status).toBe(204);

        // Test 5: Verify deletion (GET)
        try {
            await api.get(`/api/users/${newUserId}`);
        } catch (error) {
            expect(error.response).toBeDefined();
            expect(error.response.status).toBe(404);
        }
    });
});
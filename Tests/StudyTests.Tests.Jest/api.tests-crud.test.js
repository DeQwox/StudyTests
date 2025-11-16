const axios = require('axios');
const https = require('https');

const BASE_URL = 'http://localhost:5000'; 

const httpsAgent = new https.Agent({
  rejectUnauthorized: false,
});

const api = axios.create({
  baseURL: BASE_URL,
  httpsAgent: httpsAgent,
});

describe('Tests CRUD API E2E Tests', () => {

    let teacherId;
    let newTestId;
    
    const testData = {
        name: 'Initial E2E Test',
        description: 'A test created by Jest',
        teacherID: 0,
        validUntil: '2099-12-31T23:59:59Z',
        password: 'Password123!'
    };

    // 1. SETUP: Create a Teacher user before any tests run
    beforeAll(async () => {
        const teacherUser = {
            login: `teacher_${Date.now()}`,
            fullName: 'Test Teacher',
            email: `teacher_${Date.now()}@example.com`,
            password: 'Password123!',
            role: 'Teacher',
            phoneNumber: '+380963173322'
        };
        
        try {
            const response = await api.post('/api/users', teacherUser);
            teacherId = response.data.id;
            testData.teacherID = teacherId;
        } catch (error) {
            console.error("Failed to create teacher in beforeAll:", error.response?.data);
            throw error;
        }
    });

    // 4. CLEANUP: Delete the Teacher user after all tests run
    afterAll(async () => {
        if (teacherId) {
            try {
                await api.delete(`/api/users/${teacherId}`);
            } catch (error) {
                console.error("Failed to delete teacher in afterAll:", error.response?.data);
            }
        }
    });

    // --- TEST SUITE ---

    it('should create a new test', async () => {
        try {
            const response = await api.post('/api/tests', testData);

            expect(response.status).toBe(201); // Created
            expect(response.data.id).toBeGreaterThan(0);
            expect(response.data.name).toBe(testData.name);
            expect(response.data.teacherID).toBe(teacherId);

            newTestId = response.data.id;
        } catch (error) {
            console.error('Create test failed:', error.response?.data);
            throw error;
        }
    });

    it('should get the created test by ID', async () => {
        expect(newTestId).toBeDefined();
        const response = await api.get(`/api/tests/${newTestId}`);
        
        expect(response.status).toBe(200);
        expect(response.data.id).toBe(newTestId);
    });
    
    it('should get all tests (and find ours)', async () => {
        const response = await api.get('/api/tests');
        expect(response.status).toBe(200);
        expect(Array.isArray(response.data)).toBe(true);
        expect(response.data.some(t => t.id === newTestId)).toBe(true);
    });

    it('should update the test', async () => {
        const updatedData = {
            ...testData,
            name: 'Updated E2E Test'
        };

        const response = await api.put(`/api/tests/${newTestId}`, updatedData);
        expect(response.status).toBe(204);

        const verify = await api.get(`/api/tests/${newTestId}`);
        expect(verify.data.name).toBe('Updated E2E Test');
    });

    it('should delete the test', async () => {
        const response = await api.delete(`/api/tests/${newTestId}`);
        expect(response.status).toBe(204);

        try {
            await api.get(`/api/tests/${newTestId}`);
        } catch (error) {
            expect(error.response).toBeDefined();
            expect(error.response.status).toBe(404);
        }
    });
});
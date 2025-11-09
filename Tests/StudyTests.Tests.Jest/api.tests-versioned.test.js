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

describe('Versioned Tests API E2E Tests', () => {

    let teacherId;
    let testId;
    let teacherFullName;

    // 1. SETUP: Create a Teacher and a Test
    beforeAll(async () => {
        const teacherUser = {
            login: `v_teacher_${Date.now()}`,
            fullName: 'Versioned Teacher',
            email: `v_teacher_${Date.now()}@example.com`,
            password: 'Password123!',
            role: 'Teacher',
            phoneNumber: '+380963173322'
        };
        const userResponse = await api.post('/api/users', teacherUser);
        teacherId = userResponse.data.id;
        teacherFullName = userResponse.data.fullName;

        const testData = {
            name: 'V1/V2 Test',
            description: 'A test for versioned endpoints',
            teacherID: teacherId,
            validUntil: '2099-12-31T23:59:59Z',
            password: 'Password123!'
        };
        const testResponse = await api.post('/api/tests', testData);
        testId = testResponse.data.id;
    });

    // 2. CLEANUP: Delete the Test and the Teacher
    afterAll(async () => {
        if (testId) {
            await api.delete(`/api/tests/${testId}`);
        }
        if (teacherId) {
            await api.delete(`/api/users/${teacherId}`);
        }
    });

    // --- TEST SUITE ---

    it('v1 should return the minimal contract', async () => {
        expect(testId).toBeDefined();
        
        const response = await api.get(`/api/v1/tests/${testId}`);
        
        expect(response.status).toBe(200);
        
        // Check that it HAS v1 fields
        expect(response.data.id).toBe(testId);
        expect(response.data.name).toBe('V1/V2 Test');
        expect(response.data.description).toBe('A test for versioned endpoints');

        // Check that it DOES NOT HAVE v2 fields
        expect(response.data.teacherId).toBeUndefined();
        expect(response.data.teacherName).toBeUndefined();
        expect(response.data.questionCount).toBeUndefined();
    });

    it('v2 should return the superset contract', async () => {
        expect(testId).toBeDefined();
        
        const response = await api.get(`/api/v2/tests/${testId}`);
        
        expect(response.status).toBe(200);

        // Check that it HAS v2 fields
        expect(response.data.id).toBe(testId);
        expect(response.data.name).toBe('V1/V2 Test');
        expect(response.data.teacherId).toBe(teacherId);
        expect(response.data.teacherName).toBe(teacherFullName);
        expect(response.data.questionCount).toBe(0);
        expect(response.data.totalMaxScore).toBe(0);
        expect(response.data.createdAt).toBeDefined();
    });

    it('v1 should return 404 for a non-existent test', async () => {
        try {
            await api.get('/api/v1/tests/999999');
        } catch (error) {
            expect(error.response).toBeDefined();
            expect(error.response.status).toBe(404);
        }
    });
    
    it('v2 should return 404 for a non-existent test', async () => {
        try {
            await api.get('/api/v2/tests/999999');
        } catch (error) {
            expect(error.response).toBeDefined();
            expect(error.response.status).toBe(404);
        }
    });
});
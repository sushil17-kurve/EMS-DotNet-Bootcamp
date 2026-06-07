import axiosInstance from './axiosInstance';

export const authApi = {
    login: (data) =>
        axiosInstance.post('/auth/login', data),

    register: (data) =>
        axiosInstance.post('/auth/register', data),

    logout: () =>
        axiosInstance.post('/auth/logout'),

    refreshToken: (data) =>
        axiosInstance.post('/auth/refresh-token', data),

    getMe: () =>
        axiosInstance.get('/auth/me'),
};
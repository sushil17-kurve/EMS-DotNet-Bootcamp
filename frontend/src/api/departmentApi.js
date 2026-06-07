import axiosInstance from './axiosInstance';

export const departmentApi = {
    getAll: () =>
        axiosInstance.get('/departments'),

    getById: (id) =>
        axiosInstance.get(`/departments/${id}`),

    create: (data) =>
        axiosInstance.post('/departments', data),

    update: (id, data) =>
        axiosInstance.put(`/departments/${id}`, data),

    delete: (id) =>
        axiosInstance.delete(`/departments/${id}`),
};
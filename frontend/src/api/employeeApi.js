import axiosInstance from './axiosInstance';

export const employeeApi = {
    getAll: (params) =>
        axiosInstance.get('/employees', { params }),

    getById: (id) =>
        axiosInstance.get(`/employees/${id}`),

    create: (data) =>
        axiosInstance.post('/employees', data),

    update: (id, data) =>
        axiosInstance.put(`/employees/${id}`, data),

    delete: (id) =>
        axiosInstance.delete(`/employees/${id}`),

    toggleStatus: (id) =>
        axiosInstance.patch(`/employees/${id}/toggle-status`),

    uploadPhoto: (id, file) => {
        const formData = new FormData();
        formData.append('file', file);
        return axiosInstance.post(`/employees/${id}/upload-photo`, formData, {
            headers: { 'Content-Type': 'multipart/form-data' },
        });
    },
};
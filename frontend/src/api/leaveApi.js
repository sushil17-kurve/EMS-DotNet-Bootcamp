import axiosInstance from './axiosInstance';

export const leaveApi = {
    getAll: () =>
        axiosInstance.get('/leaverequests'),

    getMyLeaves: (employeeId) =>
        axiosInstance.get(`/leaverequests/my-leaves/${employeeId}`),

    getById: (id) =>
        axiosInstance.get(`/leaverequests/${id}`),

    create: (data) =>
        axiosInstance.post('/leaverequests', data),

    review: (id, data) =>
        axiosInstance.patch(`/leaverequests/${id}/review`, data),

    cancel: (id) =>
        axiosInstance.patch(`/leaverequests/${id}/cancel`),

    getLeaveTypes: () =>
        axiosInstance.get('/leaverequests/leave-types'),

    getBalance: (employeeId) =>
        axiosInstance.get(`/leaverequests/balance/${employeeId}`),
};
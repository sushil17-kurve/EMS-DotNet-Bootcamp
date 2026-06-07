import axios from 'axios';

// Base instance — all API calls go through this
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

// ── Request Interceptor ────────────────────────────────────────────────────
// Automatically attach JWT token to every request
axiosInstance.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('accessToken');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// ── Response Interceptor ───────────────────────────────────────────────────
// Handle 401 — auto refresh token, then retry original request
axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    // If 401 and we haven't retried yet
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const refreshToken  = localStorage.getItem('refreshToken');
        const accessToken   = localStorage.getItem('accessToken');

        if (!refreshToken) {
          // No refresh token → force logout
          localStorage.clear();
          window.location.href = '/login';
          return Promise.reject(error);
        }

        // Call refresh endpoint
        const response = await axios.post(
          `${import.meta.env.VITE_API_BASE_URL}/auth/refresh-token`,
          { accessToken, refreshToken }
        );

        const { accessToken: newToken, refreshToken: newRefresh } =
          response.data.data;

        localStorage.setItem('accessToken',  newToken);
        localStorage.setItem('refreshToken', newRefresh);

        // Retry original request with new token
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return axiosInstance(originalRequest);

      } catch {
        // Refresh failed → force logout
        localStorage.clear();
        window.location.href = '/login';
        return Promise.reject(error);
      }
    }

    return Promise.reject(error);
  }
);

export default axiosInstance;
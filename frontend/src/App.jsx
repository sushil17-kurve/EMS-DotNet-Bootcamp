import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './context/AuthContext';
import { theme } from './theme/theme';
import ProtectedRoute from './routes/ProtectedRoute';
import MainLayout from './components/layout/MainLayout';
import LoginPage from './pages/auth/LoginPage';
import DashboardPage from './pages/dashboard/DashboardPage';
import EmployeesPage from './pages/employees/EmployeesPage';
import DepartmentsPage from './pages/departments/DepartmentsPage';
import LeavesPage from './pages/leaves/LeavesPage';

function App() {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Toaster position="top-right" toastOptions={{ duration: 3000 }} />
            <BrowserRouter>
                <AuthProvider>
                    <Routes>
                        <Route path="/login" element={<LoginPage />} />
                        <Route element={
                            <ProtectedRoute>
                                <MainLayout />
                            </ProtectedRoute>
                        }>
                            <Route path="/dashboard" element={<DashboardPage />} />
                            <Route path="/employees" element={<EmployeesPage />} />
                            <Route path="/departments" element={<DepartmentsPage />} />
                            <Route path="/leaves" element={<LeavesPage />} />
                        </Route>
                        <Route path="/" element={<Navigate to="/dashboard" replace />} />
                        <Route path="*" element={<Navigate to="/dashboard" replace />} />
                    </Routes>
                </AuthProvider>
            </BrowserRouter>
        </ThemeProvider>
    );
}

export default App;
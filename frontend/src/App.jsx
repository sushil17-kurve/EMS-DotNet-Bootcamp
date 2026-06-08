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
// Placeholder pages — built Day 10+
//const EmployeesPage = () => <div style={{ padding: 32 }}><h2>Employees — Coming Day 10</h2></div>;
const DepartmentsPage = () => <div style={{ padding: 32 }}><h2>Departments — Coming Day 10</h2></div>;
const LeavesPage = () => <div style={{ padding: 32 }}><h2>Leaves — Coming Day 11</h2></div>;

function App() {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Toaster position="top-right" toastOptions={{ duration: 3000 }} />
            <BrowserRouter>
                <AuthProvider>
                    <Routes>
                        {/* Public */}
                        <Route path="/login" element={<LoginPage />} />

                        {/* Protected — all inside MainLayout */}
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

                        {/* Redirects */}
                        <Route path="/" element={<Navigate to="/dashboard" replace />} />
                        <Route path="*" element={<Navigate to="/dashboard" replace />} />
                    </Routes>
                </AuthProvider>
            </BrowserRouter>
        </ThemeProvider>
    );
}

export default App;
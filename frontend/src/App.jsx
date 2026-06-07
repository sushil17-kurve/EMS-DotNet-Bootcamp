import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, CssBaseline } from '@mui/material';
import { Toaster } from 'react-hot-toast';
import { AuthProvider } from './context/AuthContext';
import { theme } from './theme/theme';
import ProtectedRoute from './routes/ProtectedRoute';
import LoginPage from './pages/auth/LoginPage';

function App() {
    return (
        <ThemeProvider theme={theme}>
            <CssBaseline />
            <Toaster position="top-right" />
            <BrowserRouter>
                <AuthProvider>
                    <Routes>
                        <Route path="/login" element={<LoginPage />} />
                        <Route path="/dashboard" element={
                            <ProtectedRoute>
                                <div style={{ padding: 32 }}>
                                    <h2>✅ Dashboard — Logged In!</h2>
                                </div>
                            </ProtectedRoute>
                        } />
                        <Route path="/" element={<Navigate to="/login" replace />} />
                        <Route path="*" element={<Navigate to="/login" replace />} />
                    </Routes>
                </AuthProvider>
            </BrowserRouter>
        </ThemeProvider>
    );
}

export default App;
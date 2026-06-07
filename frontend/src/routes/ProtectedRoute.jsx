import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { CircularProgress, Box } from '@mui/material';

// Wraps any route that requires authentication
const ProtectedRoute = ({ children, requiredRole }) => {
    const { isAuthenticated, user, loading } = useAuth();
    const location = useLocation();

    // Show spinner while checking auth state
    if (loading) {
        return (
            <Box display="flex" justifyContent="center"
                alignItems="center" minHeight="100vh">
                <CircularProgress />
            </Box>
        );
    }

    // Not logged in → redirect to login, remember where they were
    if (!isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    // Role check — e.g. only SuperAdmin can access user management
    if (requiredRole && user?.role !== requiredRole) {
        return <Navigate to="/dashboard" replace />;
    }

    return children;
};

export default ProtectedRoute;
import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
    Box, Card, CardContent, TextField, Button,
    Typography, InputAdornment, IconButton,
    CircularProgress, Divider, Alert
} from '@mui/material';
import {
    Email, Lock, Visibility, VisibilityOff,
    Business
} from '@mui/icons-material';
import toast from 'react-hot-toast';
import { useAuth } from '../../context/AuthContext';

// Validation schema
const schema = yup.object({
    email: yup.string().email('Invalid email').required('Email is required'),
    password: yup.string().min(6, 'Min 6 characters').required('Password is required'),
});

const LoginPage = () => {
    const [showPassword, setShowPassword] = useState(false);
    const [serverError, setServerError] = useState('');
    const { login } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();

    // Redirect to where they came from, or dashboard
    const from = location.state?.from?.pathname || '/dashboard';

    const {
        register,
        handleSubmit,
        formState: { errors, isSubmitting }
    } = useForm({ resolver: yupResolver(schema) });

    const onSubmit = async (data) => {
        setServerError('');
        try {
            const user = await login(data.email, data.password);
            toast.success(`Welcome back, ${user.fullName}!`);
            navigate(from, { replace: true });
        } catch (err) {
            const msg = err.response?.data?.message || 'Login failed. Please try again.';
            setServerError(msg);
        }
    };

    return (
        <Box
            sx={{
                minHeight: '100vh',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                background: 'linear-gradient(135deg, #1976d2 0%, #1565c0 50%, #0d47a1 100%)',
                p: 2,
            }}
        >
            <Card sx={{ width: '100%', maxWidth: 440, borderRadius: 3 }}>
                <CardContent sx={{ p: 4 }}>

                    {/* Logo / Header */}
                    <Box textAlign="center" mb={3}>
                        <Box
                            sx={{
                                display: 'inline-flex',
                                alignItems: 'center',
                                justifyContent: 'center',
                                width: 64,
                                height: 64,
                                borderRadius: '50%',
                                bgcolor: 'primary.main',
                                mb: 2,
                            }}
                        >
                            <Business sx={{ color: 'white', fontSize: 32 }} />
                        </Box>
                        <Typography variant="h5" fontWeight={700} color="text.primary">
                            Employee Management
                        </Typography>
                        <Typography variant="body2" color="text.secondary" mt={0.5}>
                            Sign in to your account
                        </Typography>
                    </Box>

                    <Divider sx={{ mb: 3 }} />

                    {/* Server Error */}
                    {serverError && (
                        <Alert severity="error" sx={{ mb: 2 }}>
                            {serverError}
                        </Alert>
                    )}

                    {/* Login Form */}
                    <Box
                        component="form"
                        onSubmit={handleSubmit(onSubmit)}
                        noValidate
                    >
                        <TextField
                            label="Email Address"
                            type="email"
                            autoComplete="email"
                            autoFocus
                            {...register('email')}
                            error={!!errors.email}
                            helperText={errors.email?.message}
                            sx={{ mb: 2 }}
                            InputProps={{
                                startAdornment: (
                                    <InputAdornment position="start">
                                        <Email color="action" />
                                    </InputAdornment>
                                ),
                            }}
                        />

                        <TextField
                            label="Password"
                            type={showPassword ? 'text' : 'password'}
                            autoComplete="current-password"
                            {...register('password')}
                            error={!!errors.password}
                            helperText={errors.password?.message}
                            sx={{ mb: 3 }}
                            InputProps={{
                                startAdornment: (
                                    <InputAdornment position="start">
                                        <Lock color="action" />
                                    </InputAdornment>
                                ),
                                endAdornment: (
                                    <InputAdornment position="end">
                                        <IconButton
                                            onClick={() => setShowPassword(!showPassword)}
                                            edge="end"
                                        >
                                            {showPassword ? <VisibilityOff /> : <Visibility />}
                                        </IconButton>
                                    </InputAdornment>
                                ),
                            }}
                        />

                        <Button
                            type="submit"
                            variant="contained"
                            fullWidth
                            size="large"
                            disabled={isSubmitting}
                            sx={{ py: 1.5, fontSize: '1rem' }}
                        >
                            {isSubmitting
                                ? <CircularProgress size={24} color="inherit" />
                                : 'Sign In'
                            }
                        </Button>
                    </Box>

                    {/* Demo credentials hint */}
                    <Box
                        mt={3}
                        p={2}
                        bgcolor="grey.50"
                        borderRadius={2}
                        border="1px solid"
                        borderColor="grey.200"
                    >
                        <Typography variant="caption" color="text.secondary" display="block">
                            <strong>Demo Credentials:</strong>
                        </Typography>
                        <Typography variant="caption" color="text.secondary" display="block">
                            Email: superadmin@ems.com
                        </Typography>
                        <Typography variant="caption" color="text.secondary" display="block">
                            Password: Admin@123
                        </Typography>
                    </Box>

                </CardContent>
            </Card>
        </Box>
    );
};

export default LoginPage;
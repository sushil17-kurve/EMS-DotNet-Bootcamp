import { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import {
    Dialog, DialogTitle, DialogContent, DialogActions,
    Button, Grid, TextField, MenuItem,
    CircularProgress, Typography, Divider, IconButton
} from '@mui/material';
import { Close } from '@mui/icons-material';
import { departmentApi } from '../../api/departmentApi';
import toast from 'react-hot-toast';

const EMPLOYMENT_TYPES = ['FullTime', 'PartTime', 'Contract', 'Intern'];

// Validation — create mode needs password, edit mode doesn't
const createSchema = yup.object({
    firstName: yup.string().required('First name is required'),
    lastName: yup.string().required('Last name is required'),
    email: yup.string().email('Invalid email').required('Email is required'),
    password: yup.string().min(8).required('Password is required')
        .matches(/[A-Z]/, 'Must contain uppercase')
        .matches(/\d/, 'Must contain number')
        .matches(/[!@#$%^&*]/, 'Must contain special character'),
    designation: yup.string().required('Designation is required'),
    departmentId: yup.number().min(1, 'Select a department').required(),
    employmentType: yup.string().required('Employment type is required'),
    salary: yup.number().min(1, 'Salary must be > 0').required(),
    dateOfJoining: yup.string().required('Date of joining is required'),
});

const editSchema = yup.object({
    firstName: yup.string().required('First name is required'),
    lastName: yup.string().required('Last name is required'),
    designation: yup.string().required('Designation is required'),
    departmentId: yup.number().min(1, 'Select a department').required(),
    employmentType: yup.string().required('Employment type is required'),
    salary: yup.number().min(1, 'Salary must be > 0').required(),
});

const EmployeeFormDialog = ({ open, onClose, onSuccess, employee }) => {
    const isEdit = !!employee;
    const [depts, setDepts] = useState([]);

    const {
        register, handleSubmit, control, reset,
        formState: { errors, isSubmitting }
    } = useForm({
        resolver: yupResolver(isEdit ? editSchema : createSchema),
    });

    // Load departments for dropdown
    useEffect(() => {
        departmentApi.getAll().then(res => {
            setDepts(res.data.data ?? []);
        }).catch(() => toast.error('Failed to load departments'));
    }, []);

    // Pre-fill form when editing
    useEffect(() => {
        if (employee) {
            reset({
                firstName: employee.fullName?.split(' ')[0] ?? '',
                lastName: employee.fullName?.split(' ').slice(1).join(' ') ?? '',
                designation: employee.designation,
                departmentId: employee.departmentId,
                employmentType: employee.employmentType,
                salary: employee.salary,
                phoneNumber: employee.phoneNumber ?? '',
                address: employee.address ?? '',
            });
        } else {
            reset({
                firstName: '', lastName: '', email: '',
                password: '', designation: '', departmentId: '',
                employmentType: 'FullTime', salary: '',
                dateOfJoining: new Date().toISOString().split('T')[0],
            });
        }
    }, [employee, reset]);

    const onSubmit = async (data) => {
        try {
            await onSuccess(data);
            onClose();
        } catch (err) {
            const msg = err.response?.data?.message || 'Operation failed';
            toast.error(msg);
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
            <DialogTitle>
                <Typography variant="h6" fontWeight={600}>
                    {isEdit ? 'Edit Employee' : 'Add New Employee'}
                </Typography>
                <IconButton
                    onClick={onClose}
                    sx={{ position: 'absolute', right: 8, top: 8 }}
                >
                    <Close />
                </IconButton>
            </DialogTitle>

            <Divider />

            <DialogContent sx={{ pt: 3 }}>
                <Grid container spacing={2.5}>

                    {/* Personal Info Section */}
                    <Grid item xs={12}>
                        <Typography variant="subtitle2" color="text.secondary" fontWeight={600}>
                            PERSONAL INFORMATION
                        </Typography>
                    </Grid>

                    <Grid item xs={12} sm={6}>
                        <TextField
                            label="First Name"
                            {...register('firstName')}
                            error={!!errors.firstName}
                            helperText={errors.firstName?.message}
                            fullWidth
                        />
                    </Grid>

                    <Grid item xs={12} sm={6}>
                        <TextField
                            label="Last Name"
                            {...register('lastName')}
                            error={!!errors.lastName}
                            helperText={errors.lastName?.message}
                            fullWidth
                        />
                    </Grid>

                    {/* Email & Password — only on create */}
                    {!isEdit && (
                        <>
                            <Grid item xs={12} sm={6}>
                                <TextField
                                    label="Email Address"
                                    type="email"
                                    {...register('email')}
                                    error={!!errors.email}
                                    helperText={errors.email?.message}
                                    fullWidth
                                />
                            </Grid>
                            <Grid item xs={12} sm={6}>
                                <TextField
                                    label="Password"
                                    type="password"
                                    {...register('password')}
                                    error={!!errors.password}
                                    helperText={errors.password?.message}
                                    fullWidth
                                />
                            </Grid>
                        </>
                    )}

                    <Grid item xs={12} sm={6}>
                        <TextField
                            label="Phone Number"
                            {...register('phoneNumber')}
                            error={!!errors.phoneNumber}
                            helperText={errors.phoneNumber?.message}
                            fullWidth
                        />
                    </Grid>

                    <Grid item xs={12} sm={6}>
                        <TextField
                            label="Address"
                            {...register('address')}
                            fullWidth
                        />
                    </Grid>

                    {/* Employment Info Section */}
                    <Grid item xs={12} sx={{ mt: 1 }}>
                        <Typography variant="subtitle2" color="text.secondary" fontWeight={600}>
                            EMPLOYMENT INFORMATION
                        </Typography>
                    </Grid>

                    <Grid item xs={12} sm={6}>
                        <TextField
                            label="Designation"
                            {...register('designation')}
                            error={!!errors.designation}
                            helperText={errors.designation?.message}
                            fullWidth
                        />
                    </Grid>

                    <Grid item xs={12} sm={6}>
                        <Controller
                            name="departmentId"
                            control={control}
                            render={({ field }) => (
                                <TextField
                                    select
                                    label="Department"
                                    {...field}
                                    error={!!errors.departmentId}
                                    helperText={errors.departmentId?.message}
                                    fullWidth
                                >
                                    {depts.map(d => (
                                        <MenuItem key={d.id} value={d.id}>
                                            {d.name}
                                        </MenuItem>
                                    ))}
                                </TextField>
                            )}
                        />
                    </Grid>

                    <Grid item xs={12} sm={6}>
                        <Controller
                            name="employmentType"
                            control={control}
                            render={({ field }) => (
                                <TextField
                                    select
                                    label="Employment Type"
                                    {...field}
                                    error={!!errors.employmentType}
                                    helperText={errors.employmentType?.message}
                                    fullWidth
                                >
                                    {EMPLOYMENT_TYPES.map(t => (
                                        <MenuItem key={t} value={t}>{t}</MenuItem>
                                    ))}
                                </TextField>
                            )}
                        />
                    </Grid>

                    <Grid item xs={12} sm={6}>
                        <TextField
                            label="Salary"
                            type="number"
                            {...register('salary')}
                            error={!!errors.salary}
                            helperText={errors.salary?.message}
                            fullWidth
                        />
                    </Grid>

                    {!isEdit && (
                        <Grid item xs={12} sm={6}>
                            <TextField
                                label="Date of Joining"
                                type="date"
                                {...register('dateOfJoining')}
                                error={!!errors.dateOfJoining}
                                helperText={errors.dateOfJoining?.message}
                                InputLabelProps={{ shrink: true }}
                                fullWidth
                            />
                        </Grid>
                    )}

                </Grid>
            </DialogContent>

            <Divider />

            <DialogActions sx={{ px: 3, py: 2 }}>
                <Button onClick={onClose} variant="outlined">
                    Cancel
                </Button>
                <Button
                    onClick={handleSubmit(onSubmit)}
                    variant="contained"
                    disabled={isSubmitting}
                    startIcon={isSubmitting ? <CircularProgress size={16} /> : null}
                >
                    {isEdit ? 'Save Changes' : 'Create Employee'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default EmployeeFormDialog;
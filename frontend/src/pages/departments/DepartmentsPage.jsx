import { useEffect, useState, useCallback } from 'react';
import {
    Box, Grid, Card, CardContent, CardActions,
    Typography, Button, TextField, Chip,
    IconButton, Tooltip, CircularProgress,
    Alert, InputAdornment, Dialog, DialogTitle,
    DialogContent, DialogActions, Divider
} from '@mui/material';
import {
    Edit, Delete, People, Add,
    Search, Business, Close
} from '@mui/icons-material';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import toast from 'react-hot-toast';
import { departmentApi } from '../../api/departmentApi';
import PageHeader from '../../components/common/PageHeader';
import ConfirmDialog from '../../components/common/ConfirmDialog';

// ── Validation Schema ──────────────────────────────────────────────────────
const schema = yup.object({
    name: yup.string().required('Department name is required').min(2).max(150),
    description: yup.string().max(500).optional(),
});

// ── Department Form Dialog ─────────────────────────────────────────────────
const DepartmentFormDialog = ({ open, onClose, onSuccess, department }) => {
    const isEdit = !!department;

    const {
        register, handleSubmit, reset,
        formState: { errors, isSubmitting }
    } = useForm({ resolver: yupResolver(schema) });

    useEffect(() => {
        if (department) {
            reset({ name: department.name, description: department.description ?? '' });
        } else {
            reset({ name: '', description: '' });
        }
    }, [department, reset, open]);

    const onSubmit = async (data) => {
        try {
            await onSuccess(data);
            onClose();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Operation failed');
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                <Typography variant="h6" fontWeight={600}>
                    {isEdit ? 'Edit Department' : 'Add New Department'}
                </Typography>
                <IconButton onClick={onClose} sx={{ position: 'absolute', right: 8, top: 8 }}>
                    <Close />
                </IconButton>
            </DialogTitle>

            <Divider />

            <DialogContent sx={{ pt: 3 }}>
                <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2.5 }}>
                    <TextField
                        label="Department Name"
                        {...register('name')}
                        error={!!errors.name}
                        helperText={errors.name?.message}
                        autoFocus
                        fullWidth
                    />
                    <TextField
                        label="Description (optional)"
                        {...register('description')}
                        error={!!errors.description}
                        helperText={errors.description?.message}
                        multiline
                        rows={3}
                        fullWidth
                    />
                </Box>
            </DialogContent>

            <Divider />

            <DialogActions sx={{ px: 3, py: 2 }}>
                <Button onClick={onClose} variant="outlined">Cancel</Button>
                <Button
                    onClick={handleSubmit(onSubmit)}
                    variant="contained"
                    disabled={isSubmitting}
                >
                    {isEdit ? 'Save Changes' : 'Create Department'}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

// ── Main Departments Page ──────────────────────────────────────────────────
const DepartmentsPage = () => {
    const [departments, setDepartments] = useState([]);
    const [filtered, setFiltered] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [searchTerm, setSearchTerm] = useState('');
    const [formOpen, setFormOpen] = useState(false);
    const [editDept, setEditDept] = useState(null);
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [deleteDept, setDeleteDept] = useState(null);

    const fetchDepartments = useCallback(async () => {
        setLoading(true);
        try {
            const res = await departmentApi.getAll();
            const data = res.data.data ?? [];
            setDepartments(data);
            setFiltered(data);
        } catch {
            setError('Failed to load departments.');
        } finally {
            setLoading(false);
        }
    }, []);

    useEffect(() => { fetchDepartments(); }, [fetchDepartments]);

    // Client-side search filter
    useEffect(() => {
        const term = searchTerm.toLowerCase();
        setFiltered(
            departments.filter(d =>
                d.name.toLowerCase().includes(term) ||
                (d.description ?? '').toLowerCase().includes(term)
            )
        );
    }, [searchTerm, departments]);

    const handleCreate = async (data) => {
        await departmentApi.create(data);
        toast.success('Department created successfully!');
        fetchDepartments();
    };

    const handleUpdate = async (data) => {
        await departmentApi.update(editDept.id, { ...data, isActive: true });
        toast.success('Department updated successfully!');
        fetchDepartments();
    };

    const handleDelete = async () => {
        setConfirmOpen(false);
        try {
            await departmentApi.delete(deleteDept.id);
            toast.success('Department deleted successfully!');
            fetchDepartments();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Cannot delete department');
        }
    };

    // Generate a color per department card
    const COLORS = ['#1976d2', '#9c27b0', '#00bcd4', '#4caf50', '#ff9800', '#f44336', '#607d8b'];
    const getColor = (idx) => COLORS[idx % COLORS.length];

    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
                <CircularProgress />
            </Box>
        );
    }

    return (
        <Box>
            <PageHeader
                title="Departments"
                subtitle={`${departments.length} departments total`}
                buttonLabel="Add Department"
                onButtonClick={() => { setEditDept(null); setFormOpen(true); }}
            />

            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            {/* Search */}
            <Box sx={{ mb: 3, maxWidth: 400 }}>
                <TextField
                    placeholder="Search departments..."
                    value={searchTerm}
                    onChange={e => setSearchTerm(e.target.value)}
                    size="small"
                    fullWidth
                    InputProps={{
                        startAdornment: (
                            <InputAdornment position="start">
                                <Search fontSize="small" />
                            </InputAdornment>
                        ),
                    }}
                />
            </Box>

            {/* Department Cards Grid */}
            {filtered.length === 0 ? (
                <Box textAlign="center" py={8}>
                    <Business sx={{ fontSize: 64, color: 'text.disabled' }} />
                    <Typography variant="h6" color="text.secondary" mt={2}>
                        No departments found
                    </Typography>
                </Box>
            ) : (
                <Grid container spacing={3}>
                    {filtered.map((dept, idx) => (
                        <Grid item xs={12} sm={6} md={4} lg={3} key={dept.id}>
                            <Card sx={{
                                height: '100%',
                                display: 'flex',
                                flexDirection: 'column',
                                borderTop: `4px solid ${getColor(idx)}`,
                                transition: 'transform 0.2s, box-shadow 0.2s',
                                '&:hover': { transform: 'translateY(-4px)', boxShadow: 6 },
                            }}>
                                <CardContent sx={{ flexGrow: 1 }}>
                                    {/* Icon + Name */}
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5, mb: 1.5 }}>
                                        <Box sx={{
                                            width: 44, height: 44,
                                            borderRadius: '10px',
                                            bgcolor: `${getColor(idx)}20`,
                                            display: 'flex',
                                            alignItems: 'center',
                                            justifyContent: 'center',
                                        }}>
                                            <Business sx={{ color: getColor(idx) }} />
                                        </Box>
                                        <Box>
                                            <Typography variant="subtitle1" fontWeight={700}>
                                                {dept.name}
                                            </Typography>
                                            <Chip
                                                label={dept.isActive ? 'Active' : 'Inactive'}
                                                size="small"
                                                color={dept.isActive ? 'success' : 'default'}
                                                sx={{ height: 18, fontSize: '0.65rem' }}
                                            />
                                        </Box>
                                    </Box>

                                    {/* Description */}
                                    <Typography
                                        variant="body2"
                                        color="text.secondary"
                                        sx={{
                                            minHeight: 40,
                                            display: '-webkit-box',
                                            WebkitLineClamp: 2,
                                            WebkitBoxOrient: 'vertical',
                                            overflow: 'hidden',
                                        }}
                                    >
                                        {dept.description || 'No description provided'}
                                    </Typography>

                                    {/* Employee Count */}
                                    <Box sx={{
                                        display: 'flex',
                                        alignItems: 'center',
                                        gap: 0.5,
                                        mt: 2,
                                        pt: 2,
                                        borderTop: '1px solid',
                                        borderColor: 'divider',
                                    }}>
                                        <People sx={{ fontSize: 18, color: 'text.secondary' }} />
                                        <Typography variant="body2" color="text.secondary">
                                            <strong>{dept.employeeCount ?? 0}</strong> employees
                                        </Typography>
                                    </Box>
                                </CardContent>

                                <CardActions sx={{ px: 2, pb: 2, pt: 0 }}>
                                    <Button
                                        size="small"
                                        startIcon={<Edit />}
                                        onClick={() => { setEditDept(dept); setFormOpen(true); }}
                                    >
                                        Edit
                                    </Button>
                                    <Tooltip title={
                                        dept.employeeCount > 0
                                            ? 'Cannot delete — has employees'
                                            : 'Delete department'
                                    }>
                                        <span>
                                            <Button
                                                size="small"
                                                color="error"
                                                startIcon={<Delete />}
                                                disabled={dept.employeeCount > 0}
                                                onClick={() => { setDeleteDept(dept); setConfirmOpen(true); }}
                                            >
                                                Delete
                                            </Button>
                                        </span>
                                    </Tooltip>
                                </CardActions>
                            </Card>
                        </Grid>
                    ))}
                </Grid>
            )}

            {/* Form Dialog */}
            <DepartmentFormDialog
                open={formOpen}
                onClose={() => { setFormOpen(false); setEditDept(null); }}
                onSuccess={editDept ? handleUpdate : handleCreate}
                department={editDept}
            />

            {/* Confirm Delete Dialog */}
            <ConfirmDialog
                open={confirmOpen}
                title="Delete Department"
                message={`Delete "${deleteDept?.name}"? This cannot be undone.`}
                severity="error"
                confirmText="Delete"
                onConfirm={handleDelete}
                onCancel={() => setConfirmOpen(false)}
            />
        </Box>
    );
};

export default DepartmentsPage;
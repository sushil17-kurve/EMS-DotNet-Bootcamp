import { useEffect, useState, useCallback } from 'react';
import {
    Box, Card, Table, TableBody, TableCell,
    TableContainer, TableHead, TableRow,
    Typography, Chip, IconButton, Tooltip,
    CircularProgress, Alert, Button, TextField,
    MenuItem, Dialog, DialogTitle, DialogContent,
    DialogActions, Grid, Divider, Tab, Tabs
} from '@mui/material';
import {
    CheckCircle, Cancel, Visibility,
    Add, Close, EventNote
} from '@mui/icons-material';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import toast from 'react-hot-toast';
import dayjs from 'dayjs';
import { leaveApi } from '../../api/leaveApi';
import { useAuth } from '../../context/AuthContext';
import PageHeader from '../../components/common/PageHeader';

// ── Status color helper ────────────────────────────────────────────────────
const statusColor = (status) => ({
    Pending: 'warning',
    Approved: 'success',
    Rejected: 'error',
    Cancelled: 'default',
}[status] ?? 'default');

// ── Apply Leave Schema ─────────────────────────────────────────────────────
const applySchema = yup.object({
    leaveTypeId: yup.number().min(1, 'Select a leave type').required(),
    startDate: yup.string().required('Start date is required'),
    endDate: yup.string().required('End date is required'),
    reason: yup.string().min(10, 'Min 10 characters').required('Reason is required'),
});

// ── Apply Leave Dialog ─────────────────────────────────────────────────────
const ApplyLeaveDialog = ({ open, onClose, onSuccess, leaveTypes }) => {
    const {
        register, handleSubmit, control, reset, watch,
        formState: { errors, isSubmitting }
    } = useForm({ resolver: yupResolver(applySchema) });

    const startDate = watch('startDate');

    useEffect(() => {
        if (open) reset({
            leaveTypeId: '',
            startDate: dayjs().add(1, 'day').format('YYYY-MM-DD'),
            endDate: dayjs().add(1, 'day').format('YYYY-MM-DD'),
            reason: '',
        });
    }, [open, reset]);

    const onSubmit = async (data) => {
        try {
            await onSuccess(data);
            onClose();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Failed to submit leave request');
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                <Typography variant="h6" fontWeight={600}>Apply for Leave</Typography>
                <IconButton onClick={onClose} sx={{ position: 'absolute', right: 8, top: 8 }}>
                    <Close />
                </IconButton>
            </DialogTitle>
            <Divider />

            <DialogContent sx={{ pt: 3 }}>
                <Grid container spacing={2.5}>
                    <Grid item xs={12}>
                        <Controller
                            name="leaveTypeId"
                            control={control}
                            render={({ field }) => (
                                <TextField
                                    select label="Leave Type"
                                    {...field} fullWidth
                                    error={!!errors.leaveTypeId}
                                    helperText={errors.leaveTypeId?.message}
                                >
                                    {leaveTypes.map(lt => (
                                        <MenuItem key={lt.id} value={lt.id}>
                                            {lt.name} (max {lt.maxDaysAllowed} days/year)
                                        </MenuItem>
                                    ))}
                                </TextField>
                            )}
                        />
                    </Grid>

                    <Grid item xs={6}>
                        <TextField
                            label="Start Date" type="date"
                            {...register('startDate')}
                            error={!!errors.startDate}
                            helperText={errors.startDate?.message}
                            InputLabelProps={{ shrink: true }}
                            fullWidth
                        />
                    </Grid>

                    <Grid item xs={6}>
                        <TextField
                            label="End Date" type="date"
                            {...register('endDate')}
                            inputProps={{ min: startDate }}
                            error={!!errors.endDate}
                            helperText={errors.endDate?.message}
                            InputLabelProps={{ shrink: true }}
                            fullWidth
                        />
                    </Grid>

                    <Grid item xs={12}>
                        <TextField
                            label="Reason"
                            {...register('reason')}
                            error={!!errors.reason}
                            helperText={errors.reason?.message}
                            multiline rows={4}
                            placeholder="Please provide a detailed reason for your leave request..."
                            fullWidth
                        />
                    </Grid>
                </Grid>
            </DialogContent>

            <Divider />
            <DialogActions sx={{ px: 3, py: 2 }}>
                <Button onClick={onClose} variant="outlined">Cancel</Button>
                <Button
                    onClick={handleSubmit(onSubmit)}
                    variant="contained"
                    disabled={isSubmitting}
                >
                    Submit Request
                </Button>
            </DialogActions>
        </Dialog>
    );
};

// ── Review Dialog (Admin) ──────────────────────────────────────────────────
const ReviewDialog = ({ open, onClose, onSuccess, leave }) => {
    const [action, setAction] = useState('Approved');
    const [note, setNote] = useState('');
    const [loading, setLoading] = useState(false);

    const handleSubmit = async () => {
        setLoading(true);
        try {
            await onSuccess({ action, reviewNote: note });
            onClose();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Review failed');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>
                <Typography variant="h6" fontWeight={600}>Review Leave Request</Typography>
                <IconButton onClick={onClose} sx={{ position: 'absolute', right: 8, top: 8 }}>
                    <Close />
                </IconButton>
            </DialogTitle>
            <Divider />

            <DialogContent sx={{ pt: 3 }}>
                {leave && (
                    <Box sx={{ mb: 3, p: 2, bgcolor: 'grey.50', borderRadius: 2 }}>
                        <Typography variant="body2">
                            <strong>Employee:</strong> {leave.employeeName}
                        </Typography>
                        <Typography variant="body2">
                            <strong>Leave Type:</strong> {leave.leaveTypeName}
                        </Typography>
                        <Typography variant="body2">
                            <strong>Dates:</strong> {dayjs(leave.startDate).format('DD MMM YYYY')} –{' '}
                            {dayjs(leave.endDate).format('DD MMM YYYY')} ({leave.totalDays} days)
                        </Typography>
                        <Typography variant="body2">
                            <strong>Reason:</strong> {leave.reason}
                        </Typography>
                    </Box>
                )}

                <Box sx={{ display: 'flex', gap: 2, mb: 2.5 }}>
                    <Button
                        fullWidth
                        variant={action === 'Approved' ? 'contained' : 'outlined'}
                        color="success"
                        startIcon={<CheckCircle />}
                        onClick={() => setAction('Approved')}
                    >
                        Approve
                    </Button>
                    <Button
                        fullWidth
                        variant={action === 'Rejected' ? 'contained' : 'outlined'}
                        color="error"
                        startIcon={<Cancel />}
                        onClick={() => setAction('Rejected')}
                    >
                        Reject
                    </Button>
                </Box>

                <TextField
                    label="Review Note (optional)"
                    value={note}
                    onChange={e => setNote(e.target.value)}
                    multiline rows={3}
                    fullWidth
                    placeholder="Add a note for the employee..."
                />
            </DialogContent>

            <Divider />
            <DialogActions sx={{ px: 3, py: 2 }}>
                <Button onClick={onClose} variant="outlined">Cancel</Button>
                <Button
                    onClick={handleSubmit}
                    variant="contained"
                    color={action === 'Approved' ? 'success' : 'error'}
                    disabled={loading}
                >
                    {action === 'Approved' ? 'Approve' : 'Reject'} Request
                </Button>
            </DialogActions>
        </Dialog>
    );
};

// ── Main Leaves Page ───────────────────────────────────────────────────────
const LeavesPage = () => {
    const { isAdmin, user } = useAuth();
    const [leaves, setLeaves] = useState([]);
    const [leaveTypes, setLeaveTypes] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [tabValue, setTabValue] = useState(0);
    const [applyOpen, setApplyOpen] = useState(false);
    const [reviewOpen, setReviewOpen] = useState(false);
    const [activeLeave, setActiveLeave] = useState(null);

    const fetchLeaves = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            const res = isAdmin
                ? await leaveApi.getAll()
                : await leaveApi.getMyLeaves(user?.id);
            setLeaves(res.data.data ?? []);
        } catch {
            setError('Failed to load leave requests.');
        } finally {
            setLoading(false);
        }
    }, [isAdmin, user]);

    useEffect(() => { fetchLeaves(); }, [fetchLeaves]);

    useEffect(() => {
        leaveApi.getLeaveTypes()
            .then(res => setLeaveTypes(res.data.data ?? []))
            .catch(() => { });
    }, []);

    // Filter by tab (All / Pending / Approved / Rejected)
    const TAB_FILTERS = ['All', 'Pending', 'Approved', 'Rejected'];
    const filteredLeaves = leaves.filter(l =>
        tabValue === 0 ? true : l.status === TAB_FILTERS[tabValue]
    );

    const handleApply = async (data) => {
        await leaveApi.create({
            ...data,
            leaveTypeId: Number(data.leaveTypeId),
        });
        toast.success('Leave request submitted successfully!');
        fetchLeaves();
    };

    const handleReview = async (data) => {
        await leaveApi.review(activeLeave.id, data);
        toast.success(`Leave request ${data.action.toLowerCase()}d!`);
        fetchLeaves();
    };

    const handleCancel = async (id) => {
        try {
            await leaveApi.cancel(id);
            toast.success('Leave request cancelled.');
            fetchLeaves();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Cannot cancel request');
        }
    };

    return (
        <Box>
            <PageHeader
                title="Leave Requests"
                subtitle={`${leaves.length} total requests`}
                buttonLabel="Apply for Leave"
                onButtonClick={() => setApplyOpen(true)}
                showButton={!isAdmin}
            />

            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            {/* Status Tabs */}
            <Card sx={{ mb: 2 }}>
                <Tabs
                    value={tabValue}
                    onChange={(_, v) => setTabValue(v)}
                    sx={{ px: 2 }}
                >
                    {TAB_FILTERS.map((label, i) => {
                        const count = i === 0
                            ? leaves.length
                            : leaves.filter(l => l.status === label).length;
                        return (
                            <Tab
                                key={label}
                                label={
                                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                        {label}
                                        <Chip label={count} size="small" sx={{ height: 18, fontSize: '0.65rem' }} />
                                    </Box>
                                }
                            />
                        );
                    })}
                </Tabs>
            </Card>

            {/* Table */}
            <Card>
                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow sx={{ bgcolor: 'grey.50' }}>
                                {isAdmin && <TableCell sx={{ fontWeight: 600 }}>Employee</TableCell>}
                                <TableCell sx={{ fontWeight: 600 }}>Leave Type</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Start Date</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>End Date</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Days</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Reason</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Applied On</TableCell>
                                <TableCell sx={{ fontWeight: 600 }} align="center">Actions</TableCell>
                            </TableRow>
                        </TableHead>

                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={9} align="center" sx={{ py: 6 }}>
                                        <CircularProgress />
                                    </TableCell>
                                </TableRow>
                            ) : filteredLeaves.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={9} align="center" sx={{ py: 6 }}>
                                        <EventNote sx={{ fontSize: 48, color: 'text.disabled', mb: 1 }} />
                                        <Typography color="text.secondary">
                                            No leave requests found
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            ) : (
                                filteredLeaves.map(leave => (
                                    <TableRow key={leave.id} hover>
                                        {isAdmin && (
                                            <TableCell>
                                                <Typography variant="body2" fontWeight={600}>
                                                    {leave.employeeName}
                                                </Typography>
                                                <Typography variant="caption" color="text.secondary">
                                                    {leave.employeeCode}
                                                </Typography>
                                            </TableCell>
                                        )}
                                        <TableCell>
                                            <Chip label={leave.leaveTypeName} size="small" variant="outlined" />
                                        </TableCell>
                                        <TableCell>
                                            {dayjs(leave.startDate).format('DD MMM YYYY')}
                                        </TableCell>
                                        <TableCell>
                                            {dayjs(leave.endDate).format('DD MMM YYYY')}
                                        </TableCell>
                                        <TableCell>
                                            <Chip
                                                label={`${leave.totalDays}d`}
                                                size="small"
                                                color="primary"
                                                variant="outlined"
                                            />
                                        </TableCell>
                                        <TableCell sx={{ maxWidth: 200 }}>
                                            <Typography
                                                variant="body2"
                                                sx={{
                                                    overflow: 'hidden',
                                                    textOverflow: 'ellipsis',
                                                    whiteSpace: 'nowrap',
                                                    maxWidth: 180,
                                                }}
                                                title={leave.reason}
                                            >
                                                {leave.reason}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Chip
                                                label={leave.status}
                                                size="small"
                                                color={statusColor(leave.status)}
                                            />
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="caption" color="text.secondary">
                                                {dayjs(leave.appliedOn).format('DD MMM YYYY')}
                                            </Typography>
                                        </TableCell>
                                        <TableCell align="center">
                                            <Box sx={{ display: 'flex', justifyContent: 'center', gap: 0.5 }}>
                                                {/* Admin: approve/reject pending */}
                                                {isAdmin && leave.status === 'Pending' && (
                                                    <Tooltip title="Review">
                                                        <IconButton
                                                            size="small"
                                                            color="primary"
                                                            onClick={() => { setActiveLeave(leave); setReviewOpen(true); }}
                                                        >
                                                            <Visibility fontSize="small" />
                                                        </IconButton>
                                                    </Tooltip>
                                                )}
                                                {/* Employee: cancel pending */}
                                                {!isAdmin && leave.status === 'Pending' && (
                                                    <Tooltip title="Cancel Request">
                                                        <IconButton
                                                            size="small"
                                                            color="error"
                                                            onClick={() => handleCancel(leave.id)}
                                                        >
                                                            <Cancel fontSize="small" />
                                                        </IconButton>
                                                    </Tooltip>
                                                )}
                                            </Box>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            </Card>

            {/* Apply Dialog */}
            <ApplyLeaveDialog
                open={applyOpen}
                onClose={() => setApplyOpen(false)}
                onSuccess={handleApply}
                leaveTypes={leaveTypes}
            />

            {/* Review Dialog */}
            <ReviewDialog
                open={reviewOpen}
                onClose={() => { setReviewOpen(false); setActiveLeave(null); }}
                onSuccess={handleReview}
                leave={activeLeave}
            />
        </Box>
    );
};

export default LeavesPage;
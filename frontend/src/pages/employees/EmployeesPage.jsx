import { useEffect, useState, useCallback } from 'react';
import {
    Box, Card, Table, TableBody, TableCell,
    TableContainer, TableHead, TableRow, TablePagination,
    TextField, InputAdornment, MenuItem, Chip,
    IconButton, Avatar, Tooltip, CircularProgress,
    Alert, Grid, Button
} from '@mui/material';
import {
    Search, Edit, Delete, ToggleOn,
    ToggleOff, Refresh, FilterList
} from '@mui/icons-material';
import toast from 'react-hot-toast';
import { employeeApi } from '../../api/employeeApi';
import { departmentApi } from '../../api/departmentApi';
import PageHeader from '../../components/common/PageHeader';
import ConfirmDialog from '../../components/common/ConfirmDialog';
import EmployeeFormDialog from './EmployeeFormDialog';

const EmployeesPage = () => {
    // ── Data state ─────────────────────────────────────────────────────────
    const [employees, setEmployees] = useState([]);
    const [departments, setDepartments] = useState([]);
    const [totalCount, setTotalCount] = useState(0);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    // ── Filter state ────────────────────────────────────────────────────────
    const [page, setPage] = useState(0);  // MUI is 0-based
    const [pageSize, setPageSize] = useState(10);
    const [searchTerm, setSearchTerm] = useState('');
    const [deptFilter, setDeptFilter] = useState('');
    const [statusFilter, setStatusFilter] = useState('true');

    // ── Dialog state ────────────────────────────────────────────────────────
    const [formOpen, setFormOpen] = useState(false);
    const [editEmployee, setEditEmployee] = useState(null);
    const [confirmOpen, setConfirmOpen] = useState(false);
    const [actionTarget, setActionTarget] = useState(null);
    const [actionType, setActionType] = useState('');

    // ── Fetch employees ─────────────────────────────────────────────────────
    const fetchEmployees = useCallback(async () => {
        setLoading(true);
        setError('');
        try {
            const res = await employeeApi.getAll({
                page: page + 1,       // API is 1-based
                pageSize,
                searchTerm: searchTerm || undefined,
                departmentId: deptFilter || undefined,
                isActive: statusFilter,
            });
            const data = res.data.data;
            setEmployees(data.items ?? []);
            setTotalCount(data.totalCount ?? 0);
        } catch {
            setError('Failed to load employees.');
        } finally {
            setLoading(false);
        }
    }, [page, pageSize, searchTerm, deptFilter, statusFilter]);

    useEffect(() => { fetchEmployees(); }, [fetchEmployees]);

    // ── Load departments for filter dropdown ────────────────────────────────
    useEffect(() => {
        departmentApi.getAll()
            .then(res => setDepartments(res.data.data ?? []))
            .catch(() => { });
    }, []);

    // ── Search with debounce ────────────────────────────────────────────────
    const [searchInput, setSearchInput] = useState('');
    useEffect(() => {
        const timer = setTimeout(() => {
            setSearchTerm(searchInput);
            setPage(0);
        }, 500);
        return () => clearTimeout(timer);
    }, [searchInput]);

    // ── Create employee ─────────────────────────────────────────────────────
    const handleCreate = async (data) => {
        await employeeApi.create(data);
        toast.success('Employee created successfully!');
        fetchEmployees();
    };

    // ── Update employee ─────────────────────────────────────────────────────
    const handleUpdate = async (data) => {
        await employeeApi.update(editEmployee.id, {
            ...data,
            isActive: editEmployee.isActive,
        });
        toast.success('Employee updated successfully!');
        fetchEmployees();
    };

    // ── Confirm dialog actions ──────────────────────────────────────────────
    const openConfirm = (employee, type) => {
        setActionTarget(employee);
        setActionType(type);
        setConfirmOpen(true);
    };

    const handleConfirm = async () => {
        setConfirmOpen(false);
        try {
            if (actionType === 'delete') {
                await employeeApi.delete(actionTarget.id);
                toast.success('Employee deactivated successfully!');
            } else if (actionType === 'toggle') {
                await employeeApi.toggleStatus(actionTarget.id);
                toast.success(`Employee ${actionTarget.isActive ? 'deactivated' : 'activated'}!`);
            }
            fetchEmployees();
        } catch (err) {
            toast.error(err.response?.data?.message || 'Action failed');
        }
    };

    // ── Status chip color ───────────────────────────────────────────────────
    const getStatusChip = (isActive) => (
        <Chip
            label={isActive ? 'Active' : 'Inactive'}
            color={isActive ? 'success' : 'default'}
            size="small"
        />
    );

    return (
        <Box>
            <PageHeader
                title="Employees"
                subtitle={`${totalCount} total employees`}
                buttonLabel="Add Employee"
                onButtonClick={() => { setEditEmployee(null); setFormOpen(true); }}
            />

            {/* ── Filters ──────────────────────────────────────────────────── */}
            <Card sx={{ mb: 2, p: 2 }}>
                <Grid container spacing={2} alignItems="center">
                    <Grid item xs={12} sm={4}>
                        <TextField
                            placeholder="Search by name, email, code..."
                            value={searchInput}
                            onChange={e => setSearchInput(e.target.value)}
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
                    </Grid>

                    <Grid item xs={6} sm={3}>
                        <TextField
                            select
                            label="Department"
                            value={deptFilter}
                            onChange={e => { setDeptFilter(e.target.value); setPage(0); }}
                            size="small"
                            fullWidth
                        >
                            <MenuItem value="">All Departments</MenuItem>
                            {departments.map(d => (
                                <MenuItem key={d.id} value={d.id}>{d.name}</MenuItem>
                            ))}
                        </TextField>
                    </Grid>

                    <Grid item xs={6} sm={3}>
                        <TextField
                            select
                            label="Status"
                            value={statusFilter}
                            onChange={e => { setStatusFilter(e.target.value); setPage(0); }}
                            size="small"
                            fullWidth
                        >
                            <MenuItem value="">All</MenuItem>
                            <MenuItem value="true">Active</MenuItem>
                            <MenuItem value="false">Inactive</MenuItem>
                        </TextField>
                    </Grid>

                    <Grid item xs={12} sm={2}>
                        <Button
                            startIcon={<Refresh />}
                            onClick={fetchEmployees}
                            variant="outlined"
                            size="small"
                            fullWidth
                        >
                            Refresh
                        </Button>
                    </Grid>
                </Grid>
            </Card>

            {/* ── Table ─────────────────────────────────────────────────────── */}
            {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

            <Card>
                <TableContainer>
                    <Table>
                        <TableHead>
                            <TableRow sx={{ bgcolor: 'grey.50' }}>
                                <TableCell sx={{ fontWeight: 600 }}>Employee</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Code</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Designation</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Department</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Salary</TableCell>
                                <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                                <TableCell sx={{ fontWeight: 600 }} align="center">
                                    Actions
                                </TableCell>
                            </TableRow>
                        </TableHead>

                        <TableBody>
                            {loading ? (
                                <TableRow>
                                    <TableCell colSpan={8} align="center" sx={{ py: 6 }}>
                                        <CircularProgress />
                                    </TableCell>
                                </TableRow>
                            ) : employees.length === 0 ? (
                                <TableRow>
                                    <TableCell colSpan={8} align="center"
                                        sx={{ py: 6, color: 'text.secondary' }}>
                                        No employees found
                                    </TableCell>
                                </TableRow>
                            ) : (
                                employees.map(emp => (
                                    <TableRow key={emp.id} hover>
                                        {/* Employee Name + Avatar */}
                                        <TableCell>
                                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                                                <Avatar
                                                    src={emp.profilePhotoPath
                                                        ? `${import.meta.env.VITE_API_BASE_URL?.replace('/api', '')}${emp.profilePhotoPath}`
                                                        : undefined}
                                                    sx={{ width: 36, height: 36, bgcolor: 'primary.main' }}
                                                >
                                                    {emp.fullName?.charAt(0)}
                                                </Avatar>
                                                <Box>
                                                    <Box sx={{ fontWeight: 600, fontSize: '0.875rem' }}>
                                                        {emp.fullName}
                                                    </Box>
                                                    <Box sx={{ fontSize: '0.75rem', color: 'text.secondary' }}>
                                                        {emp.email}
                                                    </Box>
                                                </Box>
                                            </Box>
                                        </TableCell>

                                        <TableCell>
                                            <Chip
                                                label={emp.employeeCode}
                                                size="small"
                                                variant="outlined"
                                                color="primary"
                                            />
                                        </TableCell>

                                        <TableCell sx={{ fontSize: '0.875rem' }}>
                                            {emp.designation}
                                        </TableCell>

                                        <TableCell sx={{ fontSize: '0.875rem' }}>
                                            {emp.departmentName}
                                        </TableCell>

                                        <TableCell>
                                            <Chip
                                                label={emp.employmentType}
                                                size="small"
                                                variant="outlined"
                                            />
                                        </TableCell>

                                        <TableCell sx={{ fontSize: '0.875rem' }}>
                                            ₹{emp.salary?.toLocaleString('en-IN')}
                                        </TableCell>

                                        <TableCell>{getStatusChip(emp.isActive)}</TableCell>

                                        {/* Actions */}
                                        <TableCell align="center">
                                            <Box sx={{ display: 'flex', justifyContent: 'center' }}>
                                                <Tooltip title="Edit">
                                                    <IconButton
                                                        size="small"
                                                        color="primary"
                                                        onClick={() => {
                                                            setEditEmployee(emp);
                                                            setFormOpen(true);
                                                        }}
                                                    >
                                                        <Edit fontSize="small" />
                                                    </IconButton>
                                                </Tooltip>

                                                <Tooltip title={emp.isActive ? 'Deactivate' : 'Activate'}>
                                                    <IconButton
                                                        size="small"
                                                        color={emp.isActive ? 'warning' : 'success'}
                                                        onClick={() => openConfirm(emp, 'toggle')}
                                                    >
                                                        {emp.isActive
                                                            ? <ToggleOff fontSize="small" />
                                                            : <ToggleOn fontSize="small" />
                                                        }
                                                    </IconButton>
                                                </Tooltip>

                                                <Tooltip title="Delete">
                                                    <IconButton
                                                        size="small"
                                                        color="error"
                                                        onClick={() => openConfirm(emp, 'delete')}
                                                    >
                                                        <Delete fontSize="small" />
                                                    </IconButton>
                                                </Tooltip>
                                            </Box>
                                        </TableCell>
                                    </TableRow>
                                ))
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>

                {/* Pagination */}
                <TablePagination
                    component="div"
                    count={totalCount}
                    page={page}
                    rowsPerPage={pageSize}
                    onPageChange={(_, newPage) => setPage(newPage)}
                    onRowsPerPageChange={e => {
                        setPageSize(parseInt(e.target.value));
                        setPage(0);
                    }}
                    rowsPerPageOptions={[5, 10, 25, 50]}
                />
            </Card>

            {/* ── Form Dialog ──────────────────────────────────────────────── */}
            <EmployeeFormDialog
                open={formOpen}
                onClose={() => { setFormOpen(false); setEditEmployee(null); }}
                onSuccess={editEmployee ? handleUpdate : handleCreate}
                employee={editEmployee}
            />

            {/* ── Confirm Dialog ────────────────────────────────────────────── */}
            <ConfirmDialog
                open={confirmOpen}
                title={actionType === 'delete' ? 'Deactivate Employee' : 'Toggle Status'}
                message={
                    actionType === 'delete'
                        ? `Deactivate ${actionTarget?.fullName}? They will lose system access.`
                        : `${actionTarget?.isActive ? 'Deactivate' : 'Activate'} ${actionTarget?.fullName}?`
                }
                severity={actionType === 'delete' ? 'error' : 'warning'}
                confirmText={actionType === 'delete' ? 'Deactivate' : 'Confirm'}
                onConfirm={handleConfirm}
                onCancel={() => setConfirmOpen(false)}
            />
        </Box>
    );
};

export default EmployeesPage;
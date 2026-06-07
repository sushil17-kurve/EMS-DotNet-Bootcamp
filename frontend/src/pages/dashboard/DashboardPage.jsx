import { useEffect, useState } from 'react';
import {
    Grid, Typography, Box, Card, CardContent,
    CardHeader, CircularProgress, Alert,
    Table, TableBody, TableCell, TableContainer,
    TableHead, TableRow, Chip, Paper
} from '@mui/material';
import {
    People, Business, EventNote,
    PersonAdd, HourglassEmpty
} from '@mui/icons-material';
import {
    BarChart, Bar, XAxis, YAxis, CartesianGrid,
    Tooltip, ResponsiveContainer, PieChart, Pie,
    Cell, Legend
} from 'recharts';
import { dashboardApi } from '../../api/dashboardApi';
import StatCard from '../../components/common/StatCard';
import dayjs from 'dayjs';

// Colors for pie chart slices
const STATUS_COLORS = {
    Pending: '#ff9800',
    Approved: '#4caf50',
    Rejected: '#f44336',
    Cancelled: '#9e9e9e',
};

const DashboardPage = () => {
    const [stats, setStats] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const fetchStats = async () => {
            try {
                const res = await dashboardApi.getStats();
                setStats(res.data.data);
            } catch (err) {
                setError('Failed to load dashboard data.');
            } finally {
                setLoading(false);
            }
        };
        fetchStats();
    }, []);

    if (loading) {
        return (
            <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
                <CircularProgress size={48} />
            </Box>
        );
    }

    if (error) {
        return <Alert severity="error">{error}</Alert>;
    }

    const getStatusColor = (status) => {
        const map = {
            Pending: 'warning',
            Approved: 'success',
            Rejected: 'error',
            Cancelled: 'default',
        };
        return map[status] ?? 'default';
    };

    return (
        <Box>
            {/* Page Header */}
            <Box mb={3}>
                <Typography variant="h5" fontWeight={700}>
                    Dashboard
                </Typography>
                <Typography variant="body2" color="text.secondary">
                    Welcome back! Here's what's happening today.
                </Typography>
            </Box>

            {/* ── Stat Cards ─────────────────────────────────────────── */}
            <Grid container spacing={3} mb={3}>
                <Grid item xs={12} sm={6} md={4} lg={2.4}>
                    <StatCard
                        title="Total Employees"
                        value={stats?.totalEmployees}
                        icon={<People />}
                        color="#1976d2"
                        subtitle="All time"
                    />
                </Grid>
                <Grid item xs={12} sm={6} md={4} lg={2.4}>
                    <StatCard
                        title="Active Employees"
                        value={stats?.activeEmployees}
                        icon={<People />}
                        color="#4caf50"
                        subtitle="Currently active"
                    />
                </Grid>
                <Grid item xs={12} sm={6} md={4} lg={2.4}>
                    <StatCard
                        title="Departments"
                        value={stats?.totalDepartments}
                        icon={<Business />}
                        color="#9c27b0"
                        subtitle="Total departments"
                    />
                </Grid>
                <Grid item xs={12} sm={6} md={4} lg={2.4}>
                    <StatCard
                        title="Pending Leaves"
                        value={stats?.pendingLeaveRequests}
                        icon={<HourglassEmpty />}
                        color="#ff9800"
                        subtitle="Awaiting review"
                    />
                </Grid>
                <Grid item xs={12} sm={6} md={4} lg={2.4}>
                    <StatCard
                        title="New This Month"
                        value={stats?.newJoineesThisMonth}
                        icon={<PersonAdd />}
                        color="#00bcd4"
                        subtitle="New joiners"
                    />
                </Grid>
            </Grid>

            {/* ── Charts Row ──────────────────────────────────────────── */}
            <Grid container spacing={3} mb={3}>

                {/* Bar Chart — Monthly Joinings */}
                <Grid item xs={12} md={8}>
                    <Card>
                        <CardHeader
                            title="Monthly Joinings"
                            subheader="New employees over last 6 months"
                            titleTypographyProps={{ variant: 'h6', fontWeight: 600 }}
                        />
                        <CardContent>
                            <ResponsiveContainer width="100%" height={280}>
                                <BarChart data={stats?.monthlyJoinings ?? []}>
                                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                                    <XAxis
                                        dataKey="month"
                                        tick={{ fontSize: 12 }}
                                        axisLine={false}
                                        tickLine={false}
                                    />
                                    <YAxis
                                        allowDecimals={false}
                                        tick={{ fontSize: 12 }}
                                        axisLine={false}
                                        tickLine={false}
                                    />
                                    <Tooltip
                                        contentStyle={{
                                            borderRadius: 8,
                                            border: '1px solid #e0e0e0',
                                        }}
                                    />
                                    <Bar
                                        dataKey="count"
                                        fill="#1976d2"
                                        radius={[6, 6, 0, 0]}
                                        name="New Employees"
                                    />
                                </BarChart>
                            </ResponsiveContainer>
                        </CardContent>
                    </Card>
                </Grid>

                {/* Pie Chart — Leave Status */}
                <Grid item xs={12} md={4}>
                    <Card sx={{ height: '100%' }}>
                        <CardHeader
                            title="Leave Status"
                            subheader="Distribution by status"
                            titleTypographyProps={{ variant: 'h6', fontWeight: 600 }}
                        />
                        <CardContent>
                            <ResponsiveContainer width="100%" height={280}>
                                <PieChart>
                                    <Pie
                                        data={stats?.leaveStatusSummary ?? []}
                                        cx="50%"
                                        cy="50%"
                                        innerRadius={60}
                                        outerRadius={90}
                                        dataKey="count"
                                        nameKey="status"
                                        paddingAngle={3}
                                    >
                                        {(stats?.leaveStatusSummary ?? []).map((entry) => (
                                            <Cell
                                                key={entry.status}
                                                fill={STATUS_COLORS[entry.status] ?? '#9e9e9e'}
                                            />
                                        ))}
                                    </Pie>
                                    <Tooltip />
                                    <Legend />
                                </PieChart>
                            </ResponsiveContainer>
                        </CardContent>
                    </Card>
                </Grid>

            </Grid>

            {/* ── Bottom Row ──────────────────────────────────────────── */}
            <Grid container spacing={3}>

                {/* Department Headcount */}
                <Grid item xs={12} md={5}>
                    <Card>
                        <CardHeader
                            title="Department Headcount"
                            titleTypographyProps={{ variant: 'h6', fontWeight: 600 }}
                        />
                        <CardContent sx={{ pt: 0 }}>
                            <ResponsiveContainer width="100%" height={220}>
                                <BarChart
                                    data={stats?.departmentHeadcounts ?? []}
                                    layout="vertical"
                                >
                                    <CartesianGrid strokeDasharray="3 3" stroke="#f0f0f0" />
                                    <XAxis type="number" tick={{ fontSize: 11 }} />
                                    <YAxis
                                        dataKey="departmentName"
                                        type="category"
                                        tick={{ fontSize: 11 }}
                                        width={90}
                                    />
                                    <Tooltip />
                                    <Bar
                                        dataKey="employeeCount"
                                        fill="#9c27b0"
                                        radius={[0, 6, 6, 0]}
                                        name="Employees"
                                    />
                                </BarChart>
                            </ResponsiveContainer>
                        </CardContent>
                    </Card>
                </Grid>

                {/* Recent Leave Requests */}
                <Grid item xs={12} md={7}>
                    <Card>
                        <CardHeader
                            title="Recent Leave Requests"
                            titleTypographyProps={{ variant: 'h6', fontWeight: 600 }}
                        />
                        <TableContainer component={Paper} elevation={0}>
                            <Table size="small">
                                <TableHead>
                                    <TableRow sx={{ bgcolor: 'grey.50' }}>
                                        <TableCell sx={{ fontWeight: 600 }}>Employee</TableCell>
                                        <TableCell sx={{ fontWeight: 600 }}>Type</TableCell>
                                        <TableCell sx={{ fontWeight: 600 }}>Dates</TableCell>
                                        <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                                    </TableRow>
                                </TableHead>
                                <TableBody>
                                    {(stats?.recentLeaveRequests ?? []).length === 0 ? (
                                        <TableRow>
                                            <TableCell colSpan={4} align="center" sx={{ py: 4, color: 'text.secondary' }}>
                                                No leave requests yet
                                            </TableCell>
                                        </TableRow>
                                    ) : (
                                        (stats?.recentLeaveRequests ?? []).map((leave, idx) => (
                                            <TableRow key={idx} hover>
                                                <TableCell>
                                                    <Typography variant="body2" fontWeight={500}>
                                                        {leave.employeeName}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell>
                                                    <Typography variant="body2" color="text.secondary">
                                                        {leave.leaveType}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell>
                                                    <Typography variant="caption" color="text.secondary">
                                                        {dayjs(leave.startDate).format('DD MMM')} –{' '}
                                                        {dayjs(leave.endDate).format('DD MMM YYYY')}
                                                    </Typography>
                                                </TableCell>
                                                <TableCell>
                                                    <Chip
                                                        label={leave.status}
                                                        size="small"
                                                        color={getStatusColor(leave.status)}
                                                    />
                                                </TableCell>
                                            </TableRow>
                                        ))
                                    )}
                                </TableBody>
                            </Table>
                        </TableContainer>
                    </Card>
                </Grid>

            </Grid>
        </Box>
    );
};

export default DashboardPage;
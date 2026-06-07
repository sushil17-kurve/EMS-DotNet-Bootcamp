import { useLocation, useNavigate } from 'react-router-dom';
import {
    Box, Drawer, List, ListItem, ListItemButton,
    ListItemIcon, ListItemText, Typography,
    Divider, Avatar, Chip
} from '@mui/material';
import {
    Dashboard, People, Business,
    EventNote, ExitToApp, AdminPanelSettings
} from '@mui/icons-material';
import { useAuth } from '../../context/AuthContext';
import toast from 'react-hot-toast';

const NAV_ITEMS = [
    {
        label: 'Dashboard',
        icon: <Dashboard />,
        path: '/dashboard',
        roles: ['SuperAdmin', 'Admin', 'Employee'],
    },
    {
        label: 'Employees',
        icon: <People />,
        path: '/employees',
        roles: ['SuperAdmin', 'Admin'],
    },
    {
        label: 'Departments',
        icon: <Business />,
        path: '/departments',
        roles: ['SuperAdmin', 'Admin'],
    },
    {
        label: 'Leave Requests',
        icon: <EventNote />,
        path: '/leaves',
        roles: ['SuperAdmin', 'Admin', 'Employee'],
    },
];

const Sidebar = ({ open, onClose, drawerWidth, isMobile }) => {
    const { user, logout, isAdmin } = useAuth();
    const location = useLocation();
    const navigate = useNavigate();

    const handleLogout = async () => {
        await logout();
        toast.success('Logged out successfully');
        navigate('/login');
    };

    const handleNav = (path) => {
        navigate(path);
        if (isMobile) onClose();
    };

    // Filter nav items by user role
    const visibleItems = NAV_ITEMS.filter(item =>
        item.roles.includes(user?.role)
    );

    const drawerContent = (
        <Box sx={{
            display: 'flex',
            flexDirection: 'column',
            height: '100%',
            bgcolor: '#1a1f36',
            color: 'white',
        }}>

            {/* Logo */}
            <Box sx={{ p: 3, display: 'flex', alignItems: 'center', gap: 1.5 }}>
                <Box sx={{
                    width: 40,
                    height: 40,
                    borderRadius: '10px',
                    bgcolor: 'primary.main',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                }}>
                    <AdminPanelSettings sx={{ color: 'white', fontSize: 22 }} />
                </Box>
                <Box>
                    <Typography variant="subtitle1" fontWeight={700} color="white">
                        EMS Portal
                    </Typography>
                    <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.5)' }}>
                        v1.0.0
                    </Typography>
                </Box>
            </Box>

            <Divider sx={{ borderColor: 'rgba(255,255,255,0.1)' }} />

            {/* User Info */}
            <Box sx={{ p: 2.5 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
                    <Avatar sx={{ bgcolor: 'primary.main', width: 40, height: 40 }}>
                        {user?.fullName?.charAt(0) ?? 'U'}
                    </Avatar>
                    <Box sx={{ minWidth: 0 }}>
                        <Typography
                            variant="body2"
                            fontWeight={600}
                            color="white"
                            noWrap
                        >
                            {user?.fullName}
                        </Typography>
                        <Chip
                            label={user?.role}
                            size="small"
                            sx={{
                                height: 18,
                                fontSize: '0.65rem',
                                bgcolor: 'rgba(255,255,255,0.15)',
                                color: 'white',
                                mt: 0.3,
                            }}
                        />
                    </Box>
                </Box>
            </Box>

            <Divider sx={{ borderColor: 'rgba(255,255,255,0.1)' }} />

            {/* Navigation */}
            <Box sx={{ flexGrow: 1, py: 1 }}>
                <Typography
                    variant="caption"
                    sx={{ px: 3, color: 'rgba(255,255,255,0.4)', fontWeight: 600 }}
                >
                    MAIN MENU
                </Typography>

                <List dense sx={{ mt: 0.5 }}>
                    {visibleItems.map((item) => {
                        const isActive = location.pathname.startsWith(item.path);
                        return (
                            <ListItem key={item.path} disablePadding sx={{ px: 1.5, mb: 0.5 }}>
                                <ListItemButton
                                    onClick={() => handleNav(item.path)}
                                    sx={{
                                        borderRadius: 2,
                                        bgcolor: isActive ? 'primary.main' : 'transparent',
                                        '&:hover': {
                                            bgcolor: isActive
                                                ? 'primary.dark'
                                                : 'rgba(255,255,255,0.08)',
                                        },
                                    }}
                                >
                                    <ListItemIcon sx={{
                                        color: isActive ? 'white' : 'rgba(255,255,255,0.5)',
                                        minWidth: 36,
                                    }}>
                                        {item.icon}
                                    </ListItemIcon>
                                    <ListItemText
                                        primary={item.label}
                                        primaryTypographyProps={{
                                            fontSize: '0.875rem',
                                            fontWeight: isActive ? 600 : 400,
                                            color: isActive ? 'white' : 'rgba(255,255,255,0.7)',
                                        }}
                                    />
                                </ListItemButton>
                            </ListItem>
                        );
                    })}
                </List>
            </Box>

            <Divider sx={{ borderColor: 'rgba(255,255,255,0.1)' }} />

            {/* Logout */}
            <List sx={{ p: 1.5 }}>
                <ListItem disablePadding>
                    <ListItemButton
                        onClick={handleLogout}
                        sx={{
                            borderRadius: 2,
                            '&:hover': { bgcolor: 'rgba(255,0,0,0.15)' },
                        }}
                    >
                        <ListItemIcon sx={{ color: '#ff6b6b', minWidth: 36 }}>
                            <ExitToApp />
                        </ListItemIcon>
                        <ListItemText
                            primary="Logout"
                            primaryTypographyProps={{
                                fontSize: '0.875rem',
                                color: '#ff6b6b',
                            }}
                        />
                    </ListItemButton>
                </ListItem>
            </List>

        </Box>
    );

    return (
        <Drawer
            variant={isMobile ? 'temporary' : 'persistent'}
            open={open}
            onClose={onClose}
            sx={{
                width: drawerWidth,
                flexShrink: 0,
                '& .MuiDrawer-paper': {
                    width: drawerWidth,
                    boxSizing: 'border-box',
                    border: 'none',
                },
            }}
        >
            {drawerContent}
        </Drawer>
    );
};

export default Sidebar;
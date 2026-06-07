import {
    AppBar, Toolbar, IconButton, Typography,
    Box, Avatar, Tooltip, Badge
} from '@mui/material';
import { Menu, Notifications } from '@mui/icons-material';
import { useAuth } from '../../context/AuthContext';

const TopBar = ({ onMenuClick }) => {
    const { user } = useAuth();

    return (
        <AppBar
            position="fixed"
            elevation={0}
            sx={{
                bgcolor: 'white',
                borderBottom: '1px solid',
                borderColor: 'divider',
                zIndex: (theme) => theme.zIndex.drawer - 1,
            }}
        >
            <Toolbar>
                {/* Hamburger menu */}
                <IconButton onClick={onMenuClick} edge="start" sx={{ mr: 2, color: 'text.primary' }}>
                    <Menu />
                </IconButton>

                {/* Page title area */}
                <Typography variant="h6" fontWeight={600} color="text.primary" sx={{ flexGrow: 1 }}>
                    Employee Management System
                </Typography>

                {/* Right side */}
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>

                    {/* Notifications */}
                    <Tooltip title="Notifications">
                        <IconButton sx={{ color: 'text.secondary' }}>
                            <Badge badgeContent={3} color="error">
                                <Notifications />
                            </Badge>
                        </IconButton>
                    </Tooltip>

                    {/* User avatar */}
                    <Tooltip title={user?.fullName ?? ''}>
                        <Avatar
                            sx={{
                                width: 36,
                                height: 36,
                                bgcolor: 'primary.main',
                                cursor: 'pointer',
                                fontSize: '0.9rem',
                            }}
                        >
                            {user?.fullName?.charAt(0) ?? 'U'}
                        </Avatar>
                    </Tooltip>

                </Box>
            </Toolbar>
        </AppBar>
    );
};

export default TopBar;
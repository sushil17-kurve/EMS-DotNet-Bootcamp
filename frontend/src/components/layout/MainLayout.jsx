import { useState } from 'react';
import { Outlet } from 'react-router-dom';
import { Box, useMediaQuery, useTheme } from '@mui/material';
import Sidebar from './Sidebar';
import TopBar from './TopBar';

const DRAWER_WIDTH = 260;

const MainLayout = () => {
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));
    const [sidebarOpen, setSidebarOpen] = useState(!isMobile);

    const toggleSidebar = () => setSidebarOpen(prev => !prev);

    return (
        <Box sx={{ display: 'flex', minHeight: '100vh', bgcolor: 'background.default' }}>

            {/* Sidebar */}
            <Sidebar
                open={sidebarOpen}
                onClose={() => setSidebarOpen(false)}
                drawerWidth={DRAWER_WIDTH}
                isMobile={isMobile}
            />

            {/* Main content area */}
            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    display: 'flex',
                    flexDirection: 'column',
                    minWidth: 0,
                    ml: { md: sidebarOpen ? `${DRAWER_WIDTH}px` : 0 },
                    transition: theme.transitions.create('margin', {
                        easing: theme.transitions.easing.sharp,
                        duration: theme.transitions.duration.leavingScreen,
                    }),
                }}
            >
                {/* Top Bar */}
                <TopBar onMenuClick={toggleSidebar} />

                {/* Page Content */}
                <Box sx={{ flexGrow: 1, p: 3, mt: '64px' }}>
                    <Outlet />
                </Box>
            </Box>

        </Box>
    );
};

export default MainLayout;
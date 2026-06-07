import { Card, CardContent, Box, Typography, Avatar } from '@mui/material';
import { TrendingUp } from '@mui/icons-material';

const StatCard = ({ title, value, icon, color, subtitle }) => {
    const cardColor = color || '#1976d2';

    return (
        <Card sx={{ height: '100%' }}>
            <CardContent>
                <Box
                    sx={{
                        display: 'flex',
                        justifyContent: 'space-between',
                        alignItems: 'flex-start',
                    }}
                >
                    <Box>
                        <Typography variant="body2" color="text.secondary" fontWeight={500}>
                            {title}
                        </Typography>

                        <Typography variant="h4" fontWeight={700} mt={0.5} color="text.primary">
                            {value !== undefined && value !== null ? value : '—'}
                        </Typography>

                        {subtitle && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <TrendingUp sx={{ fontSize: 14, color: 'success.main' }} />
                                <Typography variant="caption" color="text.secondary">
                                    {subtitle}
                                </Typography>
                            </Box>
                        )}
                    </Box>

                    <Avatar
                        sx={{
                            bgcolor: `${cardColor}20`,
                            width: 52,
                            height: 52,
                            borderRadius: 2,
                        }}
                    >
                        <Box sx={{ color: cardColor, display: 'flex' }}>
                            {icon}
                        </Box>
                    </Avatar>
                </Box>
            </CardContent>
        </Card>
    );
};

export default StatCard;
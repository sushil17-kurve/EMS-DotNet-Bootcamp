import { Box, Typography, Button } from '@mui/material';
import { Add } from '@mui/icons-material';

const PageHeader = ({ title, subtitle, buttonLabel, onButtonClick, showButton = true }) => {
    return (
        <Box
            sx={{
                display: 'flex',
                justifyContent: 'space-between',
                alignItems: 'flex-start',
                mb: 3,
            }}
        >
            <Box>
                <Typography variant="h5" fontWeight={700}>
                    {title}
                </Typography>
                {subtitle && (
                    <Typography variant="body2" color="text.secondary" mt={0.5}>
                        {subtitle}
                    </Typography>
                )}
            </Box>

            {showButton && buttonLabel && (
                <Button
                    variant="contained"
                    startIcon={<Add />}
                    onClick={onButtonClick}
                    sx={{ whiteSpace: 'nowrap' }}
                >
                    {buttonLabel}
                </Button>
            )}
        </Box>
    );
};

export default PageHeader;
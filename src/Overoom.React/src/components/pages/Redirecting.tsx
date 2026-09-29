import { Box, Paper, Typography, CircularProgress } from '@mui/material';
import { styled } from '@mui/system';
import { ReactElement } from 'react';

/** Иллюстрация с лёгкой анимацией */
const StyledImage = styled('img')(({ theme }) => ({
  maxWidth: '420px',
  marginBottom: theme.spacing(4),
  filter: 'drop-shadow(0 4px 12px rgba(0,0,0,0.2))',
  animation: 'float 3s ease-in-out infinite',
  '@keyframes float': {
    '0%, 100%': { transform: 'translateY(0)' },
    '50%': { transform: 'translateY(-8px)' },
  },
}));

/**
 * Страница информирования о перенаправлении пользователя.
 * Отображается во время перехода между сервисами или авторизации.
 * @returns {ReactElement} JSX элемент страницы перенаправления
 */
const Redirecting = (): ReactElement => {
  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '70vh',
        textAlign: 'center',
        px: 2,
      }}
    >
      <Paper
        elevation={4}
        sx={{
          p: { xs: 3, md: 6 },
          maxWidth: 680,
          width: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <StyledImage src="/img/redirect.svg" alt="Перенаправление" />

        <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
          Выполняется перенаправление
        </Typography>

        <Typography
          variant="body1"
          sx={{
            mb: 4,
            color: (theme) => theme.palette.text.secondary,
            lineHeight: 1.6,
          }}
        >
          Пожалуйста, подождите — происходит перенаправление. <br />
          Обычно это занимает всего несколько секунд.
        </Typography>

        <CircularProgress
          size={38}
          thickness={4}
          sx={{ color: (theme) => theme.palette.primary.main }}
        />
      </Paper>
    </Box>
  );
};

export default Redirecting;

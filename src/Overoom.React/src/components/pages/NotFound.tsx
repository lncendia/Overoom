import HomeIcon from '@mui/icons-material/Home';
import { Box, Paper, Typography } from '@mui/material';
import { styled } from '@mui/system';
import { ReactElement } from 'react';

import WildButton from '../../ui/buttons/WildButton.tsx';

/** Стили для иллюстрации страницы 404 */
const StyledImage = styled('img')(({ theme }) => ({
  maxWidth: '420px',
  marginBottom: theme.spacing(4),
  transition: 'transform 0.3s ease',
  filter: 'drop-shadow(0 4px 12px rgba(0,0,0,0.2))',
  '&:hover': {
    transform: 'scale(1.03)',
  },
}));

/**
 * Компонент страницы 404 - отображается при переходе по несуществующему маршруту
 * @param props - Пропсы компонента
 * @param props.action - Функция-обработчик для возврата на главную страницу
 * @returns {ReactElement} JSX элемент страницы 404
 */
const NotFound = ({ action }: { action: () => void }): ReactElement => {
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
          maxWidth: 700,
          width: '100%',
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
        }}
      >
        <StyledImage src="/img/not-found.svg" alt="Страница не найдена" />

        <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
          Страница не найдена
        </Typography>

        <Typography
          variant="body1"
          sx={{
            mb: 4,
            color: (theme) => theme.palette.text.secondary,
          }}
        >
          Похоже, вы попали не туда 🤔 <br />
          Проверьте адрес или вернитесь на главную страницу.
        </Typography>

        <WildButton
          buttonText="На главную"
          onClick={action}
          icon={<HomeIcon />}
          sx={{ width: '100%', maxWidth: '300px' }}
        />
      </Paper>
    </Box>
  );
};

export default NotFound;

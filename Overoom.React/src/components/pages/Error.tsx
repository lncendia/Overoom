import RefreshIcon from '@mui/icons-material/Refresh';
import { Box, Paper, Typography } from '@mui/material';
import { styled } from '@mui/system';
import { ReactElement } from 'react';

import WildButton from '../../ui/buttons/WildButton.tsx';

/**
 * Стили для иллюстрации ошибки.
 * Добавляет тень и анимацию увеличения при наведении.
 */
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
 * Компонент страницы отображения ошибок (например, 500 или других непредвиденных).
 * Показывает иллюстрацию, сообщение и кнопку для обновления страницы.
 * @param props - Свойства компонента
 * @param props.action - Обработчик действия при нажатии кнопки «Обновить»
 * @returns {ReactElement} JSX элемент страницы ошибки
 */
const Error = ({ action }: { action: () => void }): ReactElement => {
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
        {/* Иллюстрация ошибки */}
        <StyledImage src="/img/error.svg" alt="Произошла ошибка" />

        {/* Заголовок ошибки */}
        <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
          Что-то пошло не так
        </Typography>

        {/* Описание проблемы */}
        <Typography
          variant="body1"
          sx={{
            mb: 4,
            color: (theme) => theme.palette.text.secondary,
            whiteSpace: 'pre-line',
          }}
        >
          Возникла ошибка 😕 <br />
          Попробуйте обновить страницу.
        </Typography>

        {/* Кнопка для обновления страницы */}
        <WildButton
          buttonText="Обновить"
          onClick={action}
          icon={<RefreshIcon />}
          sx={{ width: '100%', maxWidth: '300px' }}
        />
      </Paper>
    </Box>
  );
};

export default Error;

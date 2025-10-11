import LoginIcon from '@mui/icons-material/Login';
import { Box, Paper, Typography } from '@mui/material';
import { styled } from '@mui/system';
import { ReactElement } from 'react';

import WildButton from '../../ui/buttons/WildButton.tsx';

/**
 * Стили для иллюстрации страницы ограничения доступа.
 * Добавляет плавное масштабирование при наведении и тень под изображением.
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
 * Компонент, отображаемый при недостаточных правах доступа пользователя.
 * Содержит иллюстрацию, сообщение и кнопку для перехода к авторизации.
 * @param props - Свойства компонента
 * @param props.action - Обработчик нажатия кнопки "Войти"
 * @returns {ReactElement} JSX элемент страницы ограничения доступа
 */
const NotEnoughRights = ({ action }: { action: () => void }): ReactElement => {
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
        {/* Иллюстрация ограничения доступа */}
        <StyledImage src="/img/login.svg" alt="Доступ ограничен" />

        {/* Заголовок страницы */}
        <Typography variant="h6" sx={{ mb: 2, fontWeight: 600 }}>
          Доступ ограничен
        </Typography>

        {/* Сообщение о необходимости авторизации */}
        <Typography
          variant="body1"
          sx={{
            mb: 4,
            color: (theme) => theme.palette.text.secondary,
          }}
        >
          Для просмотра этой области требуется авторизация <br />
          или у вас недостаточно прав доступа.
        </Typography>

        {/* Кнопка перехода к авторизации */}
        <WildButton
          buttonText="Войти"
          onClick={action}
          icon={<LoginIcon />}
          sx={{ width: '100%', maxWidth: '300px' }}
        />
      </Paper>
    </Box>
  );
};

export default NotEnoughRights;

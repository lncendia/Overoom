import { keyframes } from '@emotion/react';
import ChatBubbleIcon from '@mui/icons-material/ChatBubble';
import { Box } from '@mui/material';
import Typography from '@mui/material/Typography';
import { ReactElement } from 'react';

/** Анимация "бегущих точек" для индикатора печати */
const dotPulse = keyframes`
  0% {
    opacity: 0.2;
    transform: translateY(0);
  }
  20% {
    opacity: 1;
    transform: translateY(-2px);
  }
  40% {
    opacity: 0.2;
    transform: translateY(0);
  }
  100% {
    opacity: 0.2;
    transform: translateY(0);
  }
`;

/** Свойства для компонента TypingIndicator */
interface TypingIndicatorProps {
  /** Текст сообщения о печатающих пользователях */
  message: string | null;
}

/**
 * Компонент индикатора печати, отображающий анимированные точки, когда пользователи печатают
 * @param props - Свойства компонента
 * @param props.message - Текст сообщения о печатающих пользователях
 * @returns {ReactElement} JSX элемент индикатора печати
 */
const TypingIndicator = ({ message }: TypingIndicatorProps): ReactElement => {
  const isVisible = !!message;

  return (
    <Box
      sx={{
        display: 'flex',
        alignItems: 'center',
        gap: 1,
        mt: 1,
        opacity: isVisible ? 1 : 0,
        transition: 'opacity 0.2s',
      }}
    >
      {/* Иконка пузыря чата */}
      <ChatBubbleIcon fontSize="small" sx={{ color: (theme) => theme.palette.text.disabled }} />

      {/* Текст сообщения с анимированными точками */}
      <Typography
        component="div"
        variant="body2"
        color="text.secondary"
        sx={{ fontStyle: 'italic', display: 'flex', alignItems: 'center', gap: 0.5 }}
      >
        {message}
        <Box sx={{ display: 'flex', ml: 0.5 }}>
          {/* Три анимированные точки */}
          <Box
            sx={{
              width: 4,
              height: 4,
              borderRadius: '50%',
              bgcolor: 'gray',
              animation: `${dotPulse} 1.4s infinite`,
            }}
          />
          <Box
            sx={{
              width: 4,
              height: 4,
              borderRadius: '50%',
              bgcolor: 'gray',
              animation: `${dotPulse} 1.4s infinite`,
              animationDelay: '0.2s',
              ml: 0.5,
            }}
          />
          <Box
            sx={{
              width: 4,
              height: 4,
              borderRadius: '50%',
              bgcolor: 'gray',
              animation: `${dotPulse} 1.4s infinite`,
              animationDelay: '0.4s',
              ml: 0.5,
            }}
          />
        </Box>
      </Typography>
    </Box>
  );
};

export default TypingIndicator;

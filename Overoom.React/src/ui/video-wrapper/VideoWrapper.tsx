import { Box } from '@mui/material';
import { ReactNode, ReactElement } from 'react';

/** Свойства компонента VideoWrapper */
export interface VideoWrapperProps {
  /** Дочерние элементы, которые будут отображаться поверх видео */
  children: ReactNode;
  /** URL видео для воспроизведения */
  src: string;
}

/**
 * Компонент обертки для видео с затемнением и возможностью размещения поверх него дочерних элементов
 * @param props - Свойства компонента
 * @param props.children - Дочерние элементы, отображаемые поверх видео
 * @param props.src - URL видео для воспроизведения
 * @returns {ReactElement} JSX элемент видеоконтейнера
 */
const VideoWrapper = ({ children, src }: VideoWrapperProps): ReactElement => {
  return (
    <Box
      sx={{
        mt: '50px',
        width: '100%',
        height: 'calc(100vh - 50px)',
        position: 'relative',
        overflow: 'hidden',
        backgroundColor: 'black',
      }}
    >
      {/* Фоновое видео с полупрозрачным эффектом */}
      <Box
        component="video"
        playsInline
        autoPlay
        muted
        loop
        sx={{
          objectFit: 'cover',
          position: 'absolute',
          top: 0,
          left: 0,
          width: '100%',
          height: '100%',
          opacity: 0.5,
        }}
      >
        <source src={src} type="video/mp4" />
        Ваш браузер не поддерживает видео.
      </Box>

      {/* Контент поверх видео */}
      <Box sx={{ position: 'relative' }}>{children}</Box>
    </Box>
  );
};

export default VideoWrapper;

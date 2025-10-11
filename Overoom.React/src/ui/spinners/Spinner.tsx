import { Box, keyframes, SxProps, Theme, useTheme } from '@mui/material';
import { ReactElement } from 'react';

/** Свойства для компонента Spinner */
export interface SpinnerProps {
  /** Размер спиннера в пикселях (по умолчанию 40) */
  size?: number;
  /** Цвет активной части спиннера (по умолчанию берется из темы) */
  color?: string;
  /** Дополнительные стили для контейнера спиннера */
  sx?: SxProps<Theme>;
}

/** Анимация вращения для спиннера */
const spin = keyframes`
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
`;

/**
 * Компонент спиннера для отображения загрузки
 * @param props - Свойства компонента
 * @param props.size - Размер спиннера
 * @param props.color - Цвет активной части спиннера
 * @param props.sx - Дополнительные стили контейнера спиннера
 * @returns {ReactElement} JSX элемент спиннера
 */
const Spinner = ({ size = 40, color, sx }: SpinnerProps): ReactElement => {
  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();

  // Определяем цвет спиннера: переданный или из темы
  const spinnerColor = color || theme.palette.primary.light;

  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        my: 3,
        ...sx,
      }}
    >
      {/* Вращающийся круг */}
      <Box
        sx={{
          width: size,
          height: size,
          border: `${Math.round(size / 8)}px solid ${spinnerColor}33`,
          borderTop: `${Math.round(size / 8)}px solid ${spinnerColor}`,
          borderRadius: '50%',
          animation: `${spin} 1s linear infinite`,
        }}
      />
    </Box>
  );
};

export default Spinner;

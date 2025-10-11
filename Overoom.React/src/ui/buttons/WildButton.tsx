import { Box, Typography, Theme, useMediaQuery, SxProps } from '@mui/material';
import { ReactNode, ReactElement } from 'react';

/** Пропсы компонента WildButton */
interface WildButtonProps {
  /** Обработчик клика по кнопке */
  onClick: () => void;
  /** Текст кнопки */
  buttonText: string;
  /** Иконка, отображаемая при наведении (только для десктопа) */
  icon: ReactNode;
  /** Дополнительные стили для кнопки */
  sx?: SxProps<Theme>;
}

/**
 * Анимированная широкая кнопка с иконкой
 * @param props - Пропсы компонента
 * @param props.onClick - Обработчик клика
 * @param props.buttonText - Текст кнопки
 * @param props.icon - Иконка для десктопа
 * @param props.sx - Дополнительные стили
 * @returns {ReactElement} JSX элемент кнопки
 */
const WildButton = ({ onClick, buttonText, icon, sx }: WildButtonProps): ReactElement => {
  const isMobile = useMediaQuery((theme: Theme) => theme.breakpoints.down('md'));

  return (
    <Box
      onClick={onClick}
      sx={{
        position: 'relative',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        height: 44,
        borderRadius: '50px',
        border: (theme) => `2px solid ${theme.palette.divider}`,
        cursor: 'pointer',
        overflow: 'hidden',
        textTransform: 'uppercase',
        fontWeight: 600,
        fontSize: '.85rem',
        letterSpacing: '1.5px',
        background: (theme) => theme.palette.primary.main,
        color: (theme) => theme.palette.primary.contrastText,
        boxShadow: (theme) => theme.shadows[2],
        transition: 'transform 0.2s ease, box-shadow 0.3s ease',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: (theme) => theme.shadows[3],
          '& .arrow': { left: 0 },
          '& .text': { left: '120%' },
        },
        ...sx,
      }}
    >
      {!isMobile && (
        <Box
          className="arrow"
          sx={{
            position: 'absolute',
            left: '-120%',
            top: 0,
            width: '100%',
            height: '100%',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            background: (theme) => theme.palette.secondary.main,
            transition: 'left .4s ease',
            zIndex: 1,
            '& svg': {
              color: (theme) => theme.palette.secondary.contrastText,
            },
          }}
        >
          {icon}
        </Box>
      )}

      <Typography
        className="text"
        sx={{
          position: 'relative',
          left: 0,
          zIndex: 2,
          transition: 'left .4s ease',
          pointerEvents: 'none',
        }}
      >
        {buttonText}
      </Typography>
    </Box>
  );
};

export default WildButton;

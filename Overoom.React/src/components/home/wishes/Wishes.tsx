import { Box, Typography, SxProps, Theme } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement } from 'react';

/** Пропсы компонента текстового пожелания */
interface WishesProps {
  /** Текст пожелания */
  text: string;
  /** Дополнительные стили MUI */
  sx?: SxProps<Theme>;
}

/** Стилизованный текст пожелания */
const WishText = styled(Typography)(({ theme }) => ({
  fontFamily: '"Morice-Bejar", "Arial", sans-serif',
  fontSize: '80px',
  textAlign: 'center',
  padding: '20px 0',
  color: theme.palette.text.primary,
  [theme.breakpoints.down('xl')]: {
    fontSize: '60px',
  },
  [theme.breakpoints.down('md')]: {
    fontSize: '40px',
  },
}));

/**
 * Компонент декоративного текстового пожелания
 * @param props - Пропсы компонента
 * @param props.text - Текст пожелания
 * @param props.sx - Дополнительные стили MUI
 * @returns {ReactElement} JSX элемент текстового пожелания
 */
const Wishes = ({ text, sx }: WishesProps): ReactElement => {
  return (
    <Box sx={{ textAlign: 'center', ...sx }}>
      {/* Текст пожелания */}
      <WishText>{text}</WishText>
    </Box>
  );
};

export default Wishes;

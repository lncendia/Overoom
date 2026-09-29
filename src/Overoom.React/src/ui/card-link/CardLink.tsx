import { styled } from '@mui/material';
import { Link as RouterLink } from 'react-router-dom';

/**
 * Ссылка-обертка для кликабельных карточек.
 * Не добавляет собственного оформления текста, но позволяет открывать страницу
 * в новой вкладке (средняя кнопка мыши) и переходить с клавиатуры.
 */
const CardLink = styled(RouterLink)(({ theme }) => ({
  display: 'block',
  color: 'inherit',
  textDecoration: 'none',
  borderRadius: theme.shape.borderRadius,
  '&:focus-visible': {
    outline: `2px solid ${theme.palette.primary.main}`,
    outlineOffset: 2,
  },
}));

export default CardLink;

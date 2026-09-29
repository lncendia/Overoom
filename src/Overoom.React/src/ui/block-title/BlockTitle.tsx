import { Typography, styled, SxProps, Theme } from '@mui/material';
import { ReactElement } from 'react';

/** Пропсы компонента BlockTitle */
interface BlockTitleProps {
  /** Текст заголовка */
  title: string;
  /** Дополнительные стили MUI Sx */
  sx?: SxProps<Theme>;
}

/** Стилизованный компонент заголовка блока */
const StyledTitle = styled(Typography)(({ theme }) => ({
  color: theme.palette.text.primary,
  marginLeft: theme.spacing(1.25), // ~10px
  fontFamily: '"Hero", "Arial", sans-serif',
  fontWeight: 600,
  lineHeight: 1.2,
}));

/**
 * Компонент заголовка блока
 * @param props - Пропсы компонента
 * @param props.title - Текст блока
 * @param props.sx - Дополнительные стили MUI
 * @returns {ReactElement} JSX элемент заголовка
 */
const BlockTitle = ({ title, sx }: BlockTitleProps): ReactElement => {
  return (
    <StyledTitle variant="h5" sx={sx}>
      {title}
    </StyledTitle>
  );
};

export default BlockTitle;

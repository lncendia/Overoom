import { Chip, Typography } from '@mui/material';
import { styled } from '@mui/material/styles';
import { memo, ReactElement } from 'react';

/** Пропсы компонента элемента слайдера жанров */
interface GenreSliderItemProps {
  /** Название жанра */
  genre: string;
  /** Флаг выбранного состояния */
  selected: boolean;
  /** Обработчик выбора жанра */
  onSelect: () => void;
}

/** Стилизованный чип жанра с анимацией */
const GenreChip = styled(Chip, {
  shouldForwardProp: (prop) => prop !== 'selected',
})<{ selected: boolean }>(({ theme, selected }) => ({
  height: '100%',
  width: '100%',
  padding: '5px 15px',
  borderRadius: theme.shape.borderRadius,
  fontFamily: '"Morice-Bejar", "Arial", sans-serif',
  transition: 'all 0.2s ease',
  cursor: 'pointer',
  backgroundColor: selected ? theme.palette.primary.main : theme.palette.background.paper,
  color: selected ? theme.palette.primary.contrastText : theme.palette.text.primary,
  boxShadow: selected ? theme.shadows[3] : 'none',
  '&:hover': {
    backgroundColor: selected ? theme.palette.primary.dark : theme.palette.action.hover,
    boxShadow: theme.shadows[1],
    transform: 'scale(1.03)',
  },
  [theme.breakpoints.up('xs')]: {
    fontSize: '25px',
  },
  [theme.breakpoints.up('md')]: {
    fontSize: '30px',
  },
  [theme.breakpoints.up('lg')]: {
    fontSize: '40px',
  },
  [theme.breakpoints.up('xl')]: {
    fontSize: '45px',
  },
}));

/**
 * Компонент элемента жанра в слайдере. Выбранный жанр выделяется основным цветом темы
 * @param props - Пропсы компонента
 * @param props.genre - Название жанра
 * @param props.selected - Флаг выбранного состояния
 * @param props.onSelect - Обработчик выбора жанра
 * @returns {ReactElement} JSX элемент жанра
 */
const GenreSliderItem = memo(
  ({ genre, selected, onSelect }: GenreSliderItemProps): ReactElement => {
    return (
      <GenreChip
        label={<Typography variant="inherit">{genre}</Typography>}
        selected={selected}
        onClick={onSelect}
        aria-pressed={selected}
        clickable
      />
    );
  }
);

export default GenreSliderItem;

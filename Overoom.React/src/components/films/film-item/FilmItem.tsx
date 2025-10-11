import { Typography, Chip, Box } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement } from 'react';

import { FilmItemDto } from './film-item.dto.ts';
import FilmCard from '../../../ui/film-card/FilmCard.tsx';
import GenresList from '../../../ui/genres-list/GenresList.tsx';

/** Пропсы компонента элемента фильма/сериала */
interface FilmItemProps {
  /** Данные о фильме или сериале */
  film: FilmItemDto;
  /** Выбранный жанр для подсветки (необязательный) */
  selectedGenre?: string;
  /** Флаг, указывающий на выбранный тип контента */
  typeSelected: boolean;
  /** Коллбэк для обработки клика по карточке */
  onClick: () => void;
}

/**
 * Стилизованный Chip для отображения типа контента (Фильм или Сериал)
 */
const TypeChip = styled(Chip)(({ theme }) => ({
  position: 'absolute',
  right: theme.spacing(2),
  bottom: theme.spacing(2),
  fontWeight: 'bold',
  '&.active': {
    backgroundColor: theme.palette.secondary.main,
    color: theme.palette.secondary.contrastText,
  },
}));

/**
 * Компонент карточки фильма или сериала в каталоге
 * @param props - объект с пропсами FilmItemProps
 * @param props.film - данные о фильме или сериале
 * @param props.selectedGenre - жанр для подсветки
 * @param props.typeSelected - флаг выбранного типа контента
 * @param props.onClick - функция для обработки клика по карточке
 * @returns {ReactElement} JSX элемент карточки фильма/сериала
 */
const FilmItem = ({ film, selectedGenre, typeSelected, onClick }: FilmItemProps): ReactElement => {
  return (
    <FilmCard {...film} onClick={onClick} header={film.title}>
      {/* Описание фильма/сериала с ограничением по строкам */}
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{
          mb: 3,
          display: '-webkit-box',
          WebkitLineClamp: 3,
          WebkitBoxOrient: 'vertical',
          overflow: 'hidden',
        }}
      >
        {film.description}
      </Typography>

      {/* Список жанров с подсветкой выбранного */}
      <Box>
        <GenresList genres={film.genres} selected={selectedGenre} />
      </Box>

      {/* Индикатор типа контента (Фильм или Сериал) */}
      <TypeChip
        label={film.isSerial ? 'Сериал' : 'Фильм'}
        size="small"
        color={typeSelected ? 'secondary' : 'default'}
      />
    </FilmCard>
  );
};

export default FilmItem;

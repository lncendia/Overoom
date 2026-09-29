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
  /** Коллбэк для обработки клика по карточке (используется, если не задан href) */
  onClick?: () => void;
  /** Адрес страницы фильма (карточка становится ссылкой) */
  href?: string;
}

/**
 * Стилизованный Chip для отображения типа контента (Фильм или Сериал)
 */
const TypeChip = styled(Chip)(({ theme }) => ({
  position: 'absolute',
  right: theme.spacing(2),
  bottom: theme.spacing(2),
  fontWeight: 'bold',
}));

/**
 * Компонент карточки фильма или сериала в каталоге
 * @param props - объект с пропсами FilmItemProps
 * @param props.film - данные о фильме или сериале
 * @param props.selectedGenre - жанр для подсветки
 * @param props.typeSelected - флаг выбранного типа контента
 * @param props.onClick - функция для обработки клика по карточке
 * @param props.href - адрес страницы фильма
 * @returns {ReactElement} JSX элемент карточки фильма/сериала
 */
const FilmItem = ({
  film,
  selectedGenre,
  typeSelected,
  onClick,
  href,
}: FilmItemProps): ReactElement => {
  return (
    <FilmCard {...film} onClick={onClick} href={href} header={film.title}>
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

      <Box>
        <GenresList genres={film.genres} selected={selectedGenre} />
      </Box>

      <TypeChip
        label={film.isSerial ? 'Сериал' : 'Фильм'}
        size="small"
        color={typeSelected ? 'secondary' : 'default'}
      />
    </FilmCard>
  );
};

export default FilmItem;

import { Box, Typography, styled } from '@mui/material';
import { ReactElement } from 'react';

import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';

/** Интерфейс свойств элемента поиска фильма */
interface FilmSearchElementProps {
  /** Данные фильма */
  film: FilmShortResponse;
  /** Обработчик клика по элементу */
  onClick: () => void;
}

/** Стилизованный элемент списка поиска */
const SearchElement = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'center',
  padding: theme.spacing(1),
  cursor: 'pointer',
  '&:hover': {
    backgroundColor: theme.palette.action.hover,
  },
  transition: theme.transitions.create('background-color', {
    duration: theme.transitions.duration.shortest,
  }),
}));

/** Стилизованное изображение постера */
const PosterImage = styled('img')({
  width: 50,
  height: 75,
  objectFit: 'cover',
  borderRadius: 4,
  marginRight: 16,
});

/**
 * Компонент списка результатов поиска фильмов
 * @param props - Пропсы компонента
 * @param props.film - Данные фильма
 * @param props.onClick - Обработчик клика по элементу
 * @returns {ReactElement} JSX элемент результата поиска
 */
const FilmSearchElement = ({ film, onClick }: FilmSearchElementProps): ReactElement => {
  return (
    <SearchElement onClick={onClick}>
      {/* Изображение постера фильма */}
      <PosterImage
        src={film.posterUrl}
        alt={`Постер ${film.title}`}
        onError={(e) => {
          (e.target as HTMLImageElement).src = '/placeholder-poster.jpg';
        }}
      />
      <Box sx={{ overflow: 'hidden' }}>
        {/* Заголовок фильма */}
        <Typography
          variant="subtitle1"
          noWrap
          sx={{
            fontWeight: 500,
            lineHeight: 1.2,
            mb: 0.5,
          }}
        >
          {film.title}
        </Typography>
        {/* Описание фильма (если есть) */}
        {film.description && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              lineHeight: 1.4,
            }}
          >
            {film.description}
          </Typography>
        )}
      </Box>
    </SearchElement>
  );
};

export default FilmSearchElement;

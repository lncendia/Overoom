import { Box, Typography, styled } from '@mui/material';
import { ReactElement } from 'react';
import { Link as RouterLink } from 'react-router-dom';

import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import { handlePosterError } from '../../../ui/poster-fallback/posterFallback.ts';

/** Интерфейс свойств элемента поиска фильма */
interface FilmSearchElementProps {
  /** Данные фильма */
  film: FilmShortResponse;
  /** Адрес страницы фильма */
  href: string;
  /** Обработчик клика по элементу (например, для закрытия списка) */
  onClick?: () => void;
}

/** Стилизованный элемент списка поиска */
const SearchElement = styled(RouterLink)(({ theme }) => ({
  display: 'flex',
  color: 'inherit',
  textDecoration: 'none',
  borderRadius: theme.shape.borderRadius,
  alignItems: 'center',
  padding: theme.spacing(1),
  cursor: 'pointer',
  '&:hover, &:focus-visible': {
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
  flexShrink: 0,
});

/**
 * Компонент списка результатов поиска фильмов
 * @param props - Пропсы компонента
 * @param props.film - Данные фильма
 * @param props.href - Адрес страницы фильма
 * @param props.onClick - Обработчик клика по элементу
 * @returns {ReactElement} JSX элемент результата поиска
 */
const FilmSearchElement = ({ film, href, onClick }: FilmSearchElementProps): ReactElement => {
  return (
    <SearchElement to={href} onClick={onClick}>
      <PosterImage
        src={film.posterUrl}
        alt={`Постер: ${film.title}`}
        loading="lazy"
        decoding="async"
        onError={handlePosterError}
      />
      <Box sx={{ overflow: 'hidden' }}>
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

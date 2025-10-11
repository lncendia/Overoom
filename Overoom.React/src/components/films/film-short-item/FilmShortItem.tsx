import { Box, Typography, styled } from '@mui/material';
import { ReactElement } from 'react';

import { FilmShortDto } from './film-short.dto.ts';
import FilmRatingStack from '../../../ui/film-rating-stack/FilmRatingStack.tsx';

/** Интерфейс свойств компонента FilmShortItem */
interface FilmShortItemProps {
  /** Данные о фильме */
  film: FilmShortDto;
  /** Обработчик клика по элементу */
  onClick: () => void;
}

/** Стилизованный контейнер для постера с наложением */
const PosterContainer = styled(Box)(({ theme }) => ({
  position: 'relative',
  borderRadius: theme.shape.borderRadius,
  overflow: 'hidden',
  cursor: 'pointer',
  transition: 'transform 0.3s ease, box-shadow 0.3s ease',
  boxShadow: theme.shadows[1],
  '&:hover': {
    transform: 'scale(1.03)',
  },
}));

/** Стилизованный компонент постера */
const PosterImage = styled('img')(({ theme }) => ({
  width: '100%',
  objectFit: 'cover',
  height: '160px',
  display: 'block',
  [theme.breakpoints.up('sm')]: {
    height: '190px',
  },
  [theme.breakpoints.up('md')]: {
    height: '220px',
  },
}));

/** Наложение для постера */
const PosterOverlay = styled(Box, {
  shouldForwardProp: (prop) => prop !== 'fullCover',
})<{ fullCover?: boolean }>(({ theme, fullCover }) => ({
  position: 'absolute',
  bottom: 0,
  left: 0,
  right: 0,
  height: fullCover ? '100%' : '40%',
  background: fullCover
    ? 'linear-gradient(to top, rgba(0,0,0,0.9) 0%, rgba(0,0,0,0.5) 100%)'
    : 'linear-gradient(to top, rgba(0,0,0,0.9) 0%, transparent 100%)',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'flex-end',
  padding: theme.spacing(1),
}));

/**
 * Компонент для отображения краткой информации о фильме
 * @param props - Пропсы компонента
 * @param props.film - Данные о фильме
 * @param props.onClick - Обработчик клика по элементу
 * @returns {ReactElement} JSX элемент краткой информации о фильме
 */
const FilmShortItem = ({ film, onClick }: FilmShortItemProps): ReactElement => {
  return (
    <PosterContainer onClick={onClick}>
      {/* Изображение постера фильма */}
      <PosterImage alt={`Постер ${film.title}`} src={film.posterUrl} />

      {/* Наложение с информацией о фильме */}
      <PosterOverlay fullCover={!!film.score}>
        {/* Центральный рейтинг со звездой */}
        {film.score && (
          <Box
            sx={{
              position: 'absolute',
              top: '50%',
              left: '50%',
              transform: 'translate(-50%, -50%)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              width: 60,
              height: 60,
              zIndex: 2,
            }}
          >
            <Typography
              variant="h6"
              color="common.white"
              sx={{
                fontWeight: 'bold',
                display: 'flex',
                alignItems: 'center',
                lineHeight: 1,
              }}
            >
              {film.score}★
            </Typography>
          </Box>
        )}

        {/* Название фильма */}
        <Typography
          variant="subtitle2"
          color="common.white"
          sx={{
            fontWeight: 'bold',
            textShadow: '0 1px 3px rgba(0,0,0,0.8)',
            lineHeight: 1.2,
            mt: 1,
          }}
        >
          {film.title}
        </Typography>
      </PosterOverlay>

      {/* Чипы с рейтингами КиноПоиск и IMDb */}
      {!film.score && (
        <FilmRatingStack sx={{ top: 8, right: 8 }} kp={film.ratingKp} imdb={film.ratingImdb} />
      )}
    </PosterContainer>
  );
};

export default FilmShortItem;

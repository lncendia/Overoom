import { Box, Grid, styled } from '@mui/material';
import { ReactElement, ReactNode } from 'react';

import FilmRatingStack from '../film-rating-stack/FilmRatingStack.tsx';

/** Свойства для компонента карточки информации о фильме */
export interface FilmInfoCardProps {
  /** URL постера фильма */
  posterUrl: string;
  /** Рейтинг Кинопоиска, может быть null */
  ratingKp: number | null;
  /** Рейтинг IMDB, может быть null */
  ratingImdb: number | null;
  /** Дочерние элементы карточки (информация о фильме) */
  children: ReactNode;
}

/** Контейнер для постера с эффектом overflow и скруглением углов */
const PosterContainer = styled(Box)(({ theme }) => ({
  position: 'relative',
  borderRadius: theme.shape.borderRadius,
  overflow: 'hidden',
  display: 'inline-block',
}));

/** Стилизованное изображение постера с эффектом масштабирования при наведении */
const ScalingPoster = styled('img')({
  width: '100%',
  display: 'block',
  objectFit: 'contain',
  transition: 'transform 0.3s ease',
  '&:hover': {
    transform: 'scale(1.05)',
  },
  maxHeight: '400px',
});

/**
 * Компонент карточки информации о фильме с постером и рейтингами
 * @param props - Свойства компонента
 * @param props.posterUrl - URL постера фильма
 * @param props.ratingKp - Рейтинг Кинопоиска
 * @param props.ratingImdb - Рейтинг IMDB
 * @param props.children - Дочерние элементы (информация о фильме)
 * @returns {ReactElement} JSX элемент карточки информации о фильме
 */
const FilmInfoCard = ({
  posterUrl,
  ratingKp,
  ratingImdb,
  children,
}: FilmInfoCardProps): ReactElement => {
  return (
    <Grid container spacing={3}>
      {/* Блок с постером */}
      <Grid size={{ xs: 12, md: 4, lg: 3, xl: 2.5 }} sx={{ textAlign: 'center' }}>
        <PosterContainer>
          {/* Основное изображение постера */}
          <ScalingPoster src={posterUrl} alt="Постер фильма" />

          {/* Чипы с рейтингами */}
          <FilmRatingStack sx={{ top: 8, right: 8 }} kp={ratingKp} imdb={ratingImdb} />
        </PosterContainer>
      </Grid>

      {/* Блок с информацией о фильме */}
      <Grid size={{ xs: 12, md: 8, lg: 9, xl: 9.5 }}>{children}</Grid>
    </Grid>
  );
};

export default FilmInfoCard;

import { Card, CardHeader, CardContent, Box, styled } from '@mui/material';
import { ReactElement, ReactNode } from 'react';

import FilmRatingStack from '../film-rating-stack/FilmRatingStack.tsx';

/** Свойства для компонента карточки фильма */
export interface FilmCardProps {
  /** URL постера фильма */
  posterUrl: string;
  /** Рейтинг Кинопоиска, может быть null */
  ratingKp: number | null;
  /** Рейтинг IMDB, может быть null */
  ratingImdb: number | null;
  /** Заголовок карточки */
  header: string;
  /** Дочерние элементы карточки */
  children: ReactNode;
  /** Обработчик клика по карточке */
  onClick: () => void;
}

/** Контейнер для постера с позиционированием и overflow hidden */
const PosterContainer = styled(Box)({
  position: 'relative',
  overflow: 'hidden',
  textAlign: 'center',
  flexShrink: '0',
});

/** Стилизация изображения постера с адаптивной высотой и анимацией при наведении */
const PosterImage = styled('img')(({ theme }) => ({
  borderRadius: theme.shape.borderRadius,
  display: 'block',
  transition: 'transform 0.3s ease, box-shadow 0.3s ease',
  objectFit: 'contain',
  '&:hover': {
    transform: 'scale(1.03)',
  },
  [theme.breakpoints.up('xs')]: {
    maxHeight: '330px',
  },
  [theme.breakpoints.up('sm')]: {
    maxHeight: '320px',
  },
  [theme.breakpoints.up('md')]: {
    maxHeight: '360px',
  },
  [theme.breakpoints.up('lg')]: {
    maxHeight: '330px',
  },
  [theme.breakpoints.up('xl')]: {
    maxHeight: '330px',
  },
}));

/** Размытый фон постера, создающий эффект глубины */
const BlurredBackground = styled(Box)(() => ({
  position: 'absolute',
  top: 0,
  left: 0,
  right: 0,
  bottom: 0,
  backgroundSize: 'cover',
  backgroundPosition: 'center',
  filter: 'blur(15px)',
  zIndex: 0,
}));

/**
 * Компонент карточки фильма с постером, рейтингами и контентом
 * @param props - Свойства компонента
 * @param props.posterUrl - URL постера фильма
 * @param props.ratingKp - Рейтинг Кинопоиска
 * @param props.ratingImdb - Рейтинг IMDB
 * @param props.header - Заголовок карточки
 * @param props.children - Дочерние элементы карточки
 * @param props.onClick - Обработчик клика по карточке
 * @returns {ReactElement} JSX элемент карточки фильма
 */
const FilmCard = ({
  posterUrl,
  ratingKp,
  ratingImdb,
  header,
  children,
  onClick,
}: FilmCardProps): ReactElement => {
  return (
    <Card
      sx={{
        position: 'relative',
        display: 'flex',
        flexDirection: { xs: 'column', lg: 'row' },
        cursor: 'pointer',
      }}
      onClick={onClick}
    >
      {/* Блок с постером и рейтингами */}
      <PosterContainer>
        {/* Размытый фон */}
        <BlurredBackground sx={{ backgroundImage: `url(${posterUrl})` }} />

        <Box sx={{ position: 'relative', display: 'inline-block', verticalAlign: 'middle' }}>
          {/* Основное изображение */}
          <PosterImage src={posterUrl} alt="Постер фильма" />

          {/* Чипы с рейтингами */}
          <FilmRatingStack
            sx={{
              position: 'absolute',
              bottom: 8,
              left: 8,
              zIndex: 2,
            }}
            kp={ratingKp}
            imdb={ratingImdb}
          />
        </Box>
      </PosterContainer>

      {/* Блок с контентом */}
      <Box>
        <CardHeader title={header} />
        <CardContent>{children}</CardContent>
      </Box>
    </Card>
  );
};

export default FilmCard;

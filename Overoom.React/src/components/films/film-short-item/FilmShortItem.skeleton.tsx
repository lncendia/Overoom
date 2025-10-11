import { Box, Skeleton, styled } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Стилизованный контейнер для скелетона постера
 */
const PosterContainer = styled(Box)(({ theme }) => ({
  position: 'relative',
  borderRadius: theme.shape.borderRadius,
  overflow: 'hidden',
  boxShadow: theme.shadows[1],
  width: '154px',
  height: '180px',
  display: 'block',
  [theme.breakpoints.up('sm')]: {
    height: '190px',
  },
  [theme.breakpoints.up('md')]: {
    height: '220px',
  },
  cursor: 'default',
}));

/**
 * Наложение для имитации текста поверх постера
 */
const PosterOverlay = styled(Box)(({ theme }) => ({
  position: 'absolute',
  bottom: 0,
  left: 0,
  right: 0,
  height: '40%',
  background: 'linear-gradient(to top, rgba(0,0,0,0.7) 0%, transparent 100%)',
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'flex-end',
  padding: theme.spacing(1),
}));

/**
 * Скелетон для карточки FilmShortItem
 * @returns {ReactElement} JSX элемент скелетона карточки фильма
 */
const FilmShortItemSkeleton = (): ReactElement => {
  return (
    <PosterContainer>
      {/* Заглушка для постера */}
      <Skeleton variant="rectangular" width="100%" height="100%" />

      {/* Наложение с имитацией текста */}
      <PosterOverlay>
        {/* Заглушка под название фильма */}
        <Skeleton variant="text" width="80%" height={20} sx={{ bgcolor: 'grey.700' }} />
      </PosterOverlay>

      {/* Заглушки под чипы с рейтингами */}
      <Box
        sx={{
          position: 'absolute',
          top: 8,
          right: 8,
          display: 'flex',
          gap: 1,
        }}
      >
        <Skeleton variant="rounded" width={40} height={24} />
        <Skeleton variant="rounded" width={40} height={24} />
      </Box>
    </PosterContainer>
  );
};

export default FilmShortItemSkeleton;

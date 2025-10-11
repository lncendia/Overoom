import { Paper, Skeleton, styled } from '@mui/material';
import { ReactElement } from 'react';

/** Стилизованный контейнер для скелетона плеера */
const PlayerSkeletonContainer = styled('div')(({ theme }) => ({
  width: '100%',
  [theme.breakpoints.up('xs')]: {
    height: '40vh',
  },
  [theme.breakpoints.up('md')]: {
    height: '50vh',
  },
  [theme.breakpoints.up('xl')]: {
    height: '75vh',
  },
  borderRadius: theme.shape.borderRadius,
  boxShadow: theme.shadows[1],
  border: 'none',
  transition: 'box-shadow 0.3s ease',
}));

/**
 * Компонент скелетона видеоплеера
 * @returns {ReactElement} JSX элемент скелетона плеера
 */
const FilmPlayerSkeleton = (): ReactElement => {
  return (
    <Paper>
      <PlayerSkeletonContainer>
        {/* Скелетон области видеоплеера */}
        <Skeleton
          variant="rectangular"
          width="100%"
          height="100%"
          animation="wave"
          sx={{ borderRadius: 1 }}
        />
      </PlayerSkeletonContainer>
    </Paper>
  );
};

export default FilmPlayerSkeleton;

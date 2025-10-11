import { Paper, Skeleton, Stack } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Скелетон компонента рейтинга
 * @returns {ReactElement} JSX элемент скелетона компонента рейтинга
 */
const FilmRatingSkeleton = (): ReactElement => {
  return (
    <Paper>
      {/* Заголовок */}
      <Skeleton variant="text" width="20%" height={28} />

      {/* Рейтинг (имитация звёзд) */}
      <Stack direction="row" spacing={1} sx={{ mt: 2 }}>
        {[...Array(10)].map((_, i) => (
          <Skeleton key={i} variant="circular" width={28} height={28} />
        ))}
      </Stack>
    </Paper>
  );
};

export default FilmRatingSkeleton;

import { Paper, Grid } from '@mui/material';
import { ReactElement } from 'react';

import FilmShortItemSkeleton from '../film-short-item/FilmShortItem.skeleton.tsx';

/**
 * Компонент скелетона компактного списка фильмов с бесконечной прокруткой
 * @returns {ReactElement} JSX элемент скелетона списка фильмов
 */
const FilmsListSkeleton = (): ReactElement => {
  // Массив для генерации заглушек элементов фильмов
  const items = Array.from({ length: 5 }, (_, i) => i);

  return (
    <Paper sx={{ overflow: 'hidden' }}>
      <Grid container spacing={2}>
        {items.map((i) => (
          <Grid size="auto" key={i} sx={{ display: 'flex', justifyContent: 'center' }}>
            {/* Скелетон компактного элемента фильма */}
            <FilmShortItemSkeleton />
          </Grid>
        ))}
      </Grid>
    </Paper>
  );
};

export default FilmsListSkeleton;

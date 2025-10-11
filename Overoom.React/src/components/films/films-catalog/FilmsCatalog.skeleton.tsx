import { Grid } from '@mui/material';
import { ReactElement } from 'react';

import FilmItemSkeleton from '../film-item/FilmItem.skeleton.tsx';

/**
 * Компонент скелетона каталога фильмов
 * @returns {ReactElement} JSX элемент скелетона каталога фильмов
 */
const FilmsCatalogSkeleton = (): ReactElement => {
  // Массив для генерации заглушек элементов фильмов
  const items = Array.from({ length: 6 }, (_, i) => i);

  return (
    <Grid container spacing={5} sx={{ mt: 4 }}>
      {items.map((i) => (
        <Grid
          size={{ xs: 12, sm: 6, md: 4, lg: 6, xl: 4 }}
          key={i}
          sx={{ display: 'flex', justifyContent: 'center' }}
        >
          {/* Скелетон элемента фильма */}
          <FilmItemSkeleton />
        </Grid>
      ))}
    </Grid>
  );
};

export default FilmsCatalogSkeleton;

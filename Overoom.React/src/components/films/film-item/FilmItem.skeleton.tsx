import { Card, CardHeader, CardContent, Box, Skeleton } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Скелетон-карточка фильма
 * @returns {ReactElement} JSX элемент скелетона карточки фильма
 */
const FilmItemSkeleton = (): ReactElement => {
  return (
    <Card
      sx={{
        display: 'flex',
        flexDirection: { xs: 'column', lg: 'row' },
        width: '100%',
        position: 'relative',
      }}
    >
      {/* Левая часть с постером */}
      <Box sx={{ textAlign: 'center' }}>
        <Box sx={{ position: 'relative', display: 'inline-block', verticalAlign: 'middle' }}>
          {/* Основное изображение */}
          <Skeleton
            variant="rectangular"
            sx={(theme) => ({
              [theme.breakpoints.up('xs')]: {
                height: '390px',
                width: '290px',
              },
              [theme.breakpoints.up('sm')]: {
                height: '370px',
                width: '280px',
              },
              [theme.breakpoints.up('md')]: {
                height: '360px',
                width: '270px',
              },
              [theme.breakpoints.up('lg')]: {
                height: '330px',
                width: '230px',
              },
              [theme.breakpoints.up('xl')]: {
                height: '330px',
                width: '230px',
              },
            })}
          />

          {/* Скелетоны под рейтинги */}
          <Box
            sx={{
              position: 'absolute',
              bottom: 8,
              left: 8,
              zIndex: 2,
              display: 'flex',
              gap: 1,
            }}
          >
            <Skeleton variant="rounded" width={48} height={28} />
            <Skeleton variant="rounded" width={48} height={28} />
          </Box>
        </Box>
      </Box>

      {/* Правая часть с текстом */}
      <Box sx={{ width: '100%' }}>
        {/* Заголовок */}
        <CardHeader title={<Skeleton variant="text" width="60%" height={28} sx={{ mb: 1 }} />} />

        <CardContent sx={{ flexGrow: 1 }}>
          {/* Описание */}
          <Box sx={{ mb: 3 }}>
            <Skeleton variant="text" width="100%" height={18} />
            <Skeleton variant="text" width="90%" height={18} />
            <Skeleton variant="text" width="95%" height={18} />
          </Box>

          {/* Список жанров */}
          <Box sx={{ display: 'flex', gap: 1, mb: 4 }}>
            <Skeleton variant="rounded" width={60} height={24} />
            <Skeleton variant="rounded" width={50} height={24} />
            <Skeleton variant="rounded" width={70} height={24} />
          </Box>

          {/* Индикатор типа */}
          <Skeleton
            variant="rounded"
            width={50}
            height={24}
            sx={{
              position: 'absolute',
              right: 16,
              bottom: 16,
              height: 24,
              bgcolor: 'grey.700',
            }}
          />
        </CardContent>
      </Box>
    </Card>
  );
};

export default FilmItemSkeleton;

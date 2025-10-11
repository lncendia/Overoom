import { Box, Divider, Grid, Paper, Skeleton, Stack, styled } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Стилизованный контейнер постера
 */
const PosterContainer = styled(Box)(({ theme }) => ({
  position: 'relative',
  borderRadius: theme.shape.borderRadius,
  overflow: 'hidden',
  height: 'clamp(400px, 35vw, 450px)',
}));

/**
 * Скелетон для информации о фильме
 * @returns {ReactElement} JSX элемент скелетона для информации о фильме
 */
const FilmInfoSkeleton = (): ReactElement => {
  return (
    <Paper>
      <Grid container spacing={3}>
        {/* Блок постера */}
        <Grid
          size={{ xs: 12, md: 4, lg: 3, xl: 2.5 }}
          sx={{ display: 'flex', justifyContent: 'center' }}
        >
          <PosterContainer>
            <Skeleton variant="rectangular" width={300} height="100%" />
          </PosterContainer>
        </Grid>

        {/* Блок с текстовой информацией */}
        <Grid size={{ xs: 12, md: 8, lg: 9, xl: 9.5 }}>
          {/* Заголовок и кнопка "Смотреть позже" */}
          <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
            <Skeleton variant="text" width="60%" height={36} />
            <Skeleton variant="circular" width={24} height={24} />
          </Stack>

          <Divider sx={{ my: 1, borderWidth: 1 }} />

          {/* Описание */}
          <Box sx={{ mb: 2 }}>
            <Skeleton variant="text" width="100%" height={18} />
            <Skeleton variant="text" width="90%" height={18} />
            <Skeleton variant="text" width="95%" height={18} />
          </Box>

          <Divider sx={{ my: 1, borderWidth: 1 }} />

          {/* Списки информации (KeyList) */}
          <Stack spacing={1}>
            {[...Array(6)].map((_, idx) => (
              <Stack key={idx} direction="row" spacing={1} alignItems="center">
                <Skeleton variant="text" width={80} height={20} />
                <Skeleton
                  variant="text"
                  width={`${30 + idx * (idx % 2 == 0 ? 2 : 1)}%`}
                  height={20}
                />
              </Stack>
            ))}
          </Stack>

          {/* Кнопка */}
          <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
            <Skeleton variant="rounded" width="100%" height={26} />
          </Box>
        </Grid>
      </Grid>
    </Paper>
  );
};

export default FilmInfoSkeleton;

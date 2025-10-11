import { Box, Stack, Skeleton, Paper } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Компонент скелетона зрителя в комнате просмотра
 * @returns {ReactElement} JSX элемент скелетона зрителя
 */
const ViewerSkeleton = (): ReactElement => {
  return (
    <Paper>
      <Stack
        direction="row"
        spacing={2}
        sx={{ mt: 1, alignItems: 'flex-start', justifyContent: 'space-between' }}
      >
        <Stack direction="row" spacing={2} sx={{ mt: 1, alignItems: 'flex-start' }}>
          {/* Скелетон аватара зрителя */}
          <Skeleton variant="circular" width={48} height={48} />

          {/* Основная информация о зрителе */}
          <Box flex={1}>
            {/* Скелетон имени и иконок состояния */}
            <Stack direction="row" spacing={1} alignItems="center" sx={{ mb: 0.5 }}>
              <Skeleton variant="text" width={120} height={20} />
              <Skeleton variant="circular" width={20} height={20} />
              <Skeleton variant="circular" width={20} height={20} />
              <Skeleton variant="circular" width={20} height={20} />
            </Stack>

            {/* Скелетон времени просмотра */}
            <Skeleton variant="text" width={80} height={16} sx={{ mb: 0.5 }} />

            {/* Скелетоны тегов зрителя */}
            <Stack direction="row" spacing={1} flexWrap="wrap">
              <Skeleton variant="rounded" width={40} height={20} />
              <Skeleton variant="rounded" width={50} height={20} />
              <Skeleton variant="rounded" width={30} height={20} />
            </Stack>
          </Box>
        </Stack>
        {/* Скелетон кнопки действия */}
        <Skeleton variant="rounded" width={40} height={40} />
      </Stack>
    </Paper>
  );
};

export default ViewerSkeleton;

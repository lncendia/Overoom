import { Paper, Box, Skeleton } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Компонент скелетона информации о пользователе
 * @returns {ReactElement} JSX элемент скелетона UserInfo
 */
const UserInfoSkeleton = (): ReactElement => {
  return (
    <Paper
      sx={{
        display: 'flex',
        alignItems: 'center',
        padding: 2,
      }}
    >
      <Skeleton variant="circular" width={64} height={64} />

      <Box sx={{ ml: 2, flex: 1 }}>
        <Skeleton variant="text" width="40%" height={28} sx={{ mb: 1 }} />

        <Box sx={{ display: 'flex', flexWrap: 'wrap' }}>
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton
              key={i}
              variant="rounded"
              width={60}
              height={24}
              sx={{ mr: 1, mb: 1, borderRadius: 1 }}
            />
          ))}
        </Box>
      </Box>
    </Paper>
  );
};

export default UserInfoSkeleton;

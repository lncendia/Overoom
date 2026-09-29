import { Paper, Skeleton, Stack } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Компонент скелетона формы добавления комментария
 * @returns {ReactElement} JSX элемент скелетона формы комментария
 */
const AddCommentFormSkeleton = (): ReactElement => {
  return (
    <Paper>
      <Stack spacing={2}>
        <Skeleton variant="text" width="30%" height={24} />

        <Skeleton variant="rounded" width="100%" height={96} />

        <Skeleton variant="rounded" width={120} height={36} />
      </Stack>
    </Paper>
  );
};

export default AddCommentFormSkeleton;

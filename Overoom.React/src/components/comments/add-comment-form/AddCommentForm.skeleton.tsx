import { Paper, Skeleton, Box, Stack } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement } from 'react';

/** Стилизованный контейнер для формы скелетона */
const FormSkeletonContainer = styled(Box)(({ theme }) => ({
  width: '100%',
  padding: theme.spacing(2),
}));

/**
 * Компонент скелетона формы добавления комментария
 * @returns {ReactElement} JSX элемент скелетона формы комментария
 */
const AddCommentFormSkeleton = (): ReactElement => {
  return (
    <Paper>
      <FormSkeletonContainer>
        <Stack spacing={2}>
          {/* Скелетон заголовка формы */}
          <Skeleton variant="text" width="30%" height={24} />

          {/* Скелетон текстового поля ввода */}
          <Skeleton variant="rounded" width="90%" height={96} />

          {/* Скелетон кнопки отправки */}
          <Skeleton variant="rounded" width={120} height={36} />
        </Stack>
      </FormSkeletonContainer>
    </Paper>
  );
};

export default AddCommentFormSkeleton;

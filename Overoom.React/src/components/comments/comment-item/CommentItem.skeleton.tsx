import { Card, CardHeader, CardContent, Skeleton, Box } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement } from 'react';

/** Стилизованная карточка скелетона комментария */
const StyledCardSkeleton = styled(Card)(({ theme }) => ({
  marginTop: theme.spacing(2),
  borderRadius: theme.shape.borderRadius,
  boxShadow: theme.shadows[1],
}));

/**
 * Скелетон элемента комментария.
 * @returns {ReactElement} JSX элемент скелетона комментария
 */
const CommentItemSkeleton = (): ReactElement => {
  return (
    <StyledCardSkeleton>
      <CardHeader
        avatar={<Skeleton variant="circular" width={40} height={40} />}
        title={<Skeleton variant="text" width="30%" />}
        subheader={<Skeleton variant="text" width="20%" />}
      />
      <CardContent sx={{ pt: 0 }}>
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          <Skeleton variant="text" width="100%" />
          <Skeleton variant="text" width="90%" />
          <Skeleton variant="text" width="95%" />
        </Box>
      </CardContent>
    </StyledCardSkeleton>
  );
};

export default CommentItemSkeleton;

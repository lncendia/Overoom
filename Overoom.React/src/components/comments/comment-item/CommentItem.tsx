import { Delete } from '@mui/icons-material';
import { Card, CardHeader, CardContent, Avatar, IconButton, Typography } from '@mui/material';
import { styled } from '@mui/material/styles';
import { ReactElement, useMemo } from 'react';

import { CommentItemDto } from './comment-item.dto.ts';

/** Интерфейс пропсов компонента CommentItem */
interface CommentParams {
  /** Данные комментария */
  comment: CommentItemDto;
  /** Функция для удаления комментария */
  removeComment: () => void;
}

/** Стилизованная карточка для комментария */
const StyledCard = styled(Card)(({ theme }) => ({
  marginTop: theme.spacing(2),
  borderRadius: theme.shape.borderRadius,
  boxShadow: theme.shadows[1],
  '&:hover': {
    boxShadow: theme.shadows[3],
  },
}));

/**
 * Компонент элемента комментария с аватаром, датой и текстом.
 * Показывает кнопку удаления, если комментарий принадлежит пользователю.
 * @param props - Свойства компонента
 * @param props.comment - Данные комментария
 * @param props.removeComment - Функция удаления комментария
 * @returns {ReactElement} JSX элемент карточки комментария
 */
const CommentItem = ({ comment, removeComment }: CommentParams): ReactElement => {
  /** Мемоизированное форматирование даты создания комментария */
  const formatedDate = useMemo(() => {
    return new Intl.DateTimeFormat('ru-RU', {
      day: 'numeric',
      month: 'long',
      year: 'numeric',
    }).format(comment.createdAt);
  }, [comment.createdAt]);

  // Рендерим карточку с аватаром, именем пользователя, датой и текстом комментария
  return (
    <StyledCard>
      <CardHeader
        avatar={
          <Avatar
            src={comment.photoUrl ?? undefined}
            alt={comment.userName}
            sx={{ width: 40, height: 40 }}
          />
        }
        action={
          comment.isUserComment && (
            <IconButton
              aria-label="delete comment"
              onClick={removeComment}
              size="small"
              color="error"
            >
              <Delete fontSize="small" />
            </IconButton>
          )
        }
        title={comment.userName}
        subheader={formatedDate}
        sx={{
          '& .MuiCardHeader-content': {
            overflow: 'hidden',
          },
        }}
      />
      <CardContent sx={{ pt: 0 }}>
        <Typography variant="body2" color="text.secondary">
          {comment.text}
        </Typography>
      </CardContent>
    </StyledCard>
  );
};

export default CommentItem;

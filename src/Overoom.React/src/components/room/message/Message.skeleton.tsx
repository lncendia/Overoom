import { Box, styled, Skeleton } from '@mui/material';
import { ReactElement } from 'react';

import { MessageBubble } from './Message.tsx';

/** Стилизованный контейнер для сообщения */
const MessageContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  marginBottom: theme.spacing(4),
}));

/**
 * Компонент скелетона сообщения в чате
 * @param props - Пропсы компонента
 * @param props.isOutgoing - Флаг исходящего сообщения
 * @returns {ReactElement} JSX элемент скелетона сообщения
 */
const MessageSkeleton = ({ isOutgoing = false }: { isOutgoing?: boolean }): ReactElement => {
  return (
    <MessageContainer sx={{ justifyContent: isOutgoing ? 'flex-end' : 'flex-start' }}>
      {!isOutgoing && <Skeleton variant="circular" width={48} height={48} />}

      <MessageBubble isOutgoing={isOutgoing}>
        <Skeleton variant="text" width={100} height={20} />

        <Skeleton variant="text" width="80%" height={16} />

        <Skeleton variant="text" width={40} height={12} sx={{ alignSelf: 'flex-end' }} />
      </MessageBubble>
    </MessageContainer>
  );
};

export default MessageSkeleton;

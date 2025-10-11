import { Box, Typography, styled } from '@mui/material';
import { ReactElement, useMemo } from 'react';

import { MessageDto } from './message.dto.ts';
import RoomAvatar from '../../../ui/room-avatar/RoomAvatar.tsx';

/** Пропсы компонента Message */
interface MessageProps {
  /** Данные одного сообщения */
  message: MessageDto;
}

/** Стилизованный контейнер для сообщения */
const MessageContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  marginBottom: theme.spacing(4),
}));

/**
 * Стилизованный блок сообщения с различной окраской для входящих и исходящих
 */
export const MessageBubble = styled(Box, {
  shouldForwardProp: (prop) => prop !== 'isOutgoing',
})<{ isOutgoing: boolean }>(({ theme, isOutgoing }) => ({
  minWidth: '100px',
  maxWidth: '500px',
  padding: theme.spacing(1, 1.5),
  borderRadius: theme.shape.borderRadius,
  position: 'relative',
  ...(isOutgoing
    ? {
        backgroundColor: theme.palette.primary.light,
        color: theme.palette.primary.contrastText,
        marginRight: theme.spacing(1),
        borderTopRightRadius: 0,
      }
    : {
        backgroundColor: theme.palette.secondary.light,
        color: theme.palette.secondary.contrastText,
        marginLeft: theme.spacing(1),
        borderTopLeftRadius: 0,
      }),
}));

/**
 * Компонент отображения сообщения в чате
 * @param props - Пропсы компонента
 * @param props.message - Данные сообщения для отображения
 * @returns {ReactElement} JSX элемент одного сообщения
 */
const Message = ({ message }: MessageProps): ReactElement => {
  /** Форматируем время отправки сообщения в формате HH:mm:ss */
  const formatedDate = useMemo(() => {
    return new Intl.DateTimeFormat('ru-RU', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false, // чтобы было 00–23
    }).format(message.sentAt);
  }, [message.sentAt]);

  return (
    <MessageContainer sx={{ justifyContent: message.isOutgoing ? 'flex-end' : 'flex-start' }}>
      {/* Если сообщение входящее, отображаем аватар */}
      {!message.isOutgoing && (
        <RoomAvatar owner={message.isOwner} src={message.photoUrl ?? undefined} />
      )}
      <MessageBubble isOutgoing={message.isOutgoing}>
        {/* Имя пользователя */}
        <Typography variant="subtitle2" fontWeight="bold">
          {message.userName}
        </Typography>
        {/* Текст сообщения */}
        <Typography variant="body1" sx={{ wordBreak: 'break-word' }}>
          {message.text}
        </Typography>
        {/* Время отправки */}
        <Typography variant="caption" display="block" textAlign="right">
          {formatedDate}
        </Typography>
      </MessageBubble>
    </MessageContainer>
  );
};

export default Message;

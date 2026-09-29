import AddReactionOutlinedIcon from '@mui/icons-material/AddReactionOutlined';
import { Box, Chip, IconButton, Popover, Stack, Tooltip, Typography, styled } from '@mui/material';
import { memo, MouseEvent, ReactElement, useMemo, useState } from 'react';

import { MessageDto } from './message.dto.ts';
import { REACTIONS } from '../../../ui/reactions/reactions.ts';
import RoomAvatar from '../../../ui/room-avatar/RoomAvatar.tsx';

/**
 * Возвращает название реакции для экранных дикторов
 * @param code - Код реакции
 * @returns {string} Название реакции или её код, если реакция неизвестна
 */
const reactionLabel = (code: string): string =>
  REACTIONS.find((r) => r.code === code)?.label ?? code;

/** Пропсы компонента Message */
interface MessageProps {
  /** Данные одного сообщения */
  message: MessageDto;
  /**
   * Обработчик установки или снятия реакции. Без него реакции только отображаются.
   * Общий для всех сообщений, чтобы мемоизация компонента не сбрасывалась
   */
  onReact?: (messageId: string, reaction: string) => void;
}

/** Стилизованный контейнер для сообщения */
const MessageContainer = styled(Box)(({ theme }) => ({
  display: 'flex',
  alignItems: 'flex-start',
  marginBottom: theme.spacing(4),
  '& .add-reaction': {
    transition: theme.transitions.create('opacity'),
  },
  // На устройствах с мышью кнопка реакции появляется при наведении, на сенсорных видна всегда
  '@media (hover: hover)': {
    '& .add-reaction': { opacity: 0 },
    '&:hover .add-reaction, & .add-reaction:focus-visible': { opacity: 1 },
  },
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
 * @param props.onReact - Обработчик установки или снятия реакции
 * @returns {ReactElement} JSX элемент одного сообщения
 */
const Message = ({ message, onReact }: MessageProps): ReactElement => {
  /** Элемент, к которому привязан выбор реакции */
  const [pickerAnchor, setPickerAnchor] = useState<HTMLElement | null>(null);

  /** Форматируем время отправки сообщения в формате HH:mm:ss */
  const formatedDate = useMemo(() => {
    return new Intl.DateTimeFormat('ru-RU', {
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
      hour12: false, // чтобы было 00–23
    }).format(message.sentAt);
  }, [message.sentAt]);

  /**
   * Ставит или снимает реакцию и закрывает выбор реакции
   * @param reaction - Код реакции
   */
  const react = (reaction: string) => {
    setPickerAnchor(null);
    onReact?.(message.id, reaction);
  };

  const addReactionButton = onReact && (
    <IconButton
      className="add-reaction"
      size="small"
      aria-label="Поставить реакцию"
      onClick={(e: MouseEvent<HTMLElement>) => setPickerAnchor(e.currentTarget)}
    >
      <AddReactionOutlinedIcon fontSize="small" />
    </IconButton>
  );

  return (
    <MessageContainer sx={{ justifyContent: message.isOutgoing ? 'flex-end' : 'flex-start' }}>
      {!message.isOutgoing && (
        <RoomAvatar owner={message.isOwner} src={message.photoUrl ?? undefined} />
      )}
      {message.isOutgoing && addReactionButton}
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          alignItems: message.isOutgoing ? 'flex-end' : 'flex-start',
          minWidth: 0,
        }}
      >
        <MessageBubble isOutgoing={message.isOutgoing}>
          <Typography variant="subtitle2" sx={{ fontWeight: 'bold' }}>
            {message.userName}
          </Typography>
          <Typography variant="body1" sx={{ wordBreak: 'break-word' }}>
            {message.text}
          </Typography>
          <Typography variant="caption" sx={{ display: 'block', textAlign: 'right' }}>
            {formatedDate}
          </Typography>
        </MessageBubble>
        {message.reactions.length > 0 && (
          <Stack
            direction="row"
            sx={{
              flexWrap: 'wrap',
              gap: 0.5,
              mt: 0.5,
              ...(message.isOutgoing ? { mr: 1 } : { ml: 1 }),
            }}
          >
            {message.reactions.map((r) => (
              <Tooltip key={r.code} title={r.userNames.join(', ')}>
                <Chip
                  size="small"
                  label={`${r.emoji} ${r.count}`}
                  color={r.reacted ? 'primary' : 'default'}
                  variant={r.reacted ? 'filled' : 'outlined'}
                  onClick={onReact ? () => react(r.code) : undefined}
                  aria-label={`${reactionLabel(r.code)}: ${r.count}`}
                  aria-pressed={onReact ? r.reacted : undefined}
                />
              </Tooltip>
            ))}
          </Stack>
        )}
      </Box>
      {!message.isOutgoing && addReactionButton}
      <Popover
        open={pickerAnchor !== null}
        anchorEl={pickerAnchor}
        onClose={() => setPickerAnchor(null)}
        anchorOrigin={{ vertical: 'top', horizontal: 'center' }}
        transformOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Stack direction="row" sx={{ p: 0.5 }}>
          {REACTIONS.map((r) => (
            <IconButton key={r.code} aria-label={r.label} onClick={() => react(r.code)}>
              <Typography sx={{ fontSize: '1.25rem', lineHeight: 1 }}>{r.emoji}</Typography>
            </IconButton>
          ))}
        </Stack>
      </Popover>
    </MessageContainer>
  );
};

export default memo(Message);

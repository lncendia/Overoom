import { Paper, Skeleton, Stack } from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement, useEffect, useMemo, useRef, useState } from 'react';

import ConnectLink from './ConnectLink.tsx';
import { MessageDto } from '../../../components/room/message/message.dto.ts';
import MessagesListSkeleton from '../../../components/room/messages-list/MessagesList.skeleton.tsx';
import MessagesList from '../../../components/room/messages-list/MessagesList.tsx';
import SendMessageForm from '../../../components/room/send-message-form/SendMessageForm.tsx';
import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import RoomDto from '../../../hooks/room-hook/room.dto.ts';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';
import MessageResponse from '../../../services/room-hub/responses/message.response.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';
import TypingIndicator from '../../../ui/typing-indicator/TypingIndicator.tsx';

/**
 * Основной компонент чата комнаты
 * @returns {ReactElement} JSX элемент чата комнаты
 */
const RoomChatModule = (): ReactElement => {
  /** Состояние сообщений */
  const [messages, setMessages] = useState<MessageResponse[]>([]);

  /** Флаг загрузки данных */
  const [isLoading, setIsLoading] = useState(true);

  /** Общее количество сообщений */
  const totalCount = useRef<number>(0);

  /** Контекст комнаты */
  const { room, currentViewerId, hub } = useRoom();

  /** Код подключения к комнате */
  const code = useRoomCode(room?.id);

  /** Флаг наличия дополнительных сообщений */
  const hasMore = useMemo(() => messages.length < totalCount.current, [messages.length]);

  /** ID последнего сообщения для пагинации */
  const lastMessageId = useMemo(() => messages[messages.length - 1]?.id, [messages]);

  /** Преобразует серверные сообщения в DTO для отображения */
  const mappedMessages = useMemo<MessageDto[]>(() => {
    return messages.map((m) => {
      const viewer = room?.viewers.get(m.userId);
      return {
        id: m.id,
        isOutgoing: m.userId === currentViewerId,
        isOwner: m.userId === room?.ownerId,
        sentAt: new Date(m.sentAt),
        photoUrl: viewer?.photoUrl ?? null,
        text: m.text,
        userName: viewer?.userName ?? 'Зритель',
      };
    });
  }, [currentViewerId, messages, room?.ownerId, room?.viewers]);

  /** Обрабатывает события хаба комнаты */
  useEffect(() => {
    if (!hub) return;

    const handler = (e: RoomEventContainer) => {
      if (e.messagesEvent) {
        setMessages((prev) => [...prev, ...e.messagesEvent!.messages.list]);
        totalCount.current = e.messagesEvent!.messages.totalCount;
        setIsLoading(false);
      } else if (e.messageEvent) {
        setMessages((prev) => [e.messageEvent!.message, ...prev]);
        totalCount.current += 1;
      }
    };

    hub.addHandler(handler);
    return () => hub.removeHandler(handler);
  }, [hub]);

  /** Загрузка сообщений из хаба */
  const get = useSafeCallback(
    async (fromId?: string) => {
      await hub?.getMessages(fromId, 10);
    },
    [hub]
  );

  /** Первичная загрузка сообщений */
  useEffect(() => {
    get().then();
    return () => {
      setMessages([]);
      setIsLoading(true);
      totalCount.current = 0;
    };
  }, [get]);

  /** Отправка сообщения */
  const sendMessage = useSafeCallback(
    async (text: string) => {
      await hub?.sendMessage(text);
    },
    [hub]
  );

  /** Отправка события печати */
  const onTyping = useSafeCallback(async () => {
    await hub?.type();
  }, [hub]);

  /** Сообщение о печатающих зрителях */
  const typingMessage = useTypingMessage(room, currentViewerId);

  if (!room || isLoading)
    return (
      <Paper>
        <MessagesListSkeleton />
        <Skeleton variant="rounded" width="100%" height={76} sx={{ mb: 1 }} />
        <Stack direction="row" alignItems="center">
          <Skeleton variant="circular" width={16} height={16} sx={{ mr: 1 }} />
          <Skeleton variant="text" width={120} height={20} />
        </Stack>
      </Paper>
    );

  return (
    <Paper>
      <MessagesList next={() => get(lastMessageId)} hasMore={hasMore} messages={mappedMessages} />
      <TypingIndicator message={typingMessage} />
      <SendMessageForm onTyping={onTyping} onSend={sendMessage} />
      <ConnectLink code={code} id={room.id} endpoint="room" />
    </Paper>
  );
};

export default RoomChatModule;

/**
 * Хук формирует сообщение о печатающих зрителях
 * @param room - текущее состояние комнаты
 * @param currentViewerId - id текущего пользователя (не включается в список)
 * @returns {string | null} Сообщение для отображения или null, если никто не печатает
 */
function useTypingMessage(room: RoomDto | null, currentViewerId: string): string | null {
  return useMemo(() => {
    if (!room) return null;

    const typingViewers = Array.from(room.viewerStates.entries())
      .filter(([id, state]) => state.typing && id !== currentViewerId)
      .map(([id]) => room.viewers.get(id)?.userName ?? 'Зритель');

    if (typingViewers.length === 0) return null;
    if (typingViewers.length === 1) return `${typingViewers[0]} печатает`;
    if (typingViewers.length === 2) return `${typingViewers[0]} и ${typingViewers[1]} печатают`;
    return `${typingViewers[0]} и ещё ${typingViewers.length - 1} печатают`;
  }, [room, currentViewerId]);
}

/**
 * Хук для загрузки кода комнаты по её ID
 * @param roomId - идентификатор комнаты
 * @returns {string | null} код комнаты или null
 */
const useRoomCode = (roomId?: string): string | null => {
  const roomsApi = useInjection<RoomsApi>('RoomsApi');
  const [code, setCode] = useState<string | null>(null);

  const loadCode = useSafeCallback(async (): Promise<void> => {
    if (!roomId) return;
    const result = await roomsApi.getCode(roomId);
    setCode(result);
  }, [roomsApi, roomId]);

  useEffect(() => {
    loadCode().then();
    return () => setCode(null);
  }, [loadCode]);

  return code;
};

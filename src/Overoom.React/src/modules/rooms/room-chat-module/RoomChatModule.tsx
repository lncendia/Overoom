import { Paper, Skeleton, Stack } from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useEffect, useMemo, useRef, useState } from 'react';

import ConnectLink from './ConnectLink.tsx';
import { MessageDto, MessageReactionDto } from '../../../components/room/message/message.dto.ts';
import MessagesListSkeleton from '../../../components/room/messages-list/MessagesList.skeleton.tsx';
import MessagesList from '../../../components/room/messages-list/MessagesList.tsx';
import SendMessageForm from '../../../components/room/send-message-form/SendMessageForm.tsx';
import { useRoom } from '../../../contexts/room-context/useRoomContext.tsx';
import RoomDto from '../../../hooks/room-hook/room.dto.ts';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import MessageReactionEvent from '../../../services/room-hub/events/messages/message-reaction.event.ts';
import { RoomEventContainer } from '../../../services/room-hub/events/room-event.container.ts';
import MessageResponse from '../../../services/room-hub/responses/message.response.ts';
import { RoomsApi } from '../../../services/rooms/rooms.api.ts';
import { REACTIONS } from '../../../ui/reactions/reactions.ts';
import TypingIndicator from '../../../ui/typing-indicator/TypingIndicator.tsx';

/** Количество сообщений, запрашиваемых за одну загрузку */
const PAGE_SIZE = 10;

/** Минимальный интервал между сигналами о наборе сообщения (мс) */
const TYPING_THROTTLE = 2500;

/** Данные автора сообщения, сохраняемые после его выхода из комнаты */
interface ChatUser {
  /** Имя пользователя */
  userName: string;
  /** URL фото пользователя */
  photoUrl: string | null;
}

/**
 * Основной компонент чата комнаты
 * @returns {ReactElement} JSX элемент чата комнаты
 */
const RoomChatModule = (): ReactElement => {
  /** Состояние сообщений (от новых к старым) */
  const [messages, setMessages] = useState<MessageResponse[]>([]);

  /** Флаг загрузки данных */
  const [isLoading, setIsLoading] = useState(true);

  /** Общее количество сообщений */
  const [totalCount, setTotalCount] = useState(0);

  /** Флаг: сервер вернул неполную страницу, более старых сообщений нет */
  const [isExhausted, setIsExhausted] = useState(false);

  /** Контекст комнаты */
  const { room, currentViewerId, hub, reconnectCount } = useRoom();

  /** Код подключения к комнате */
  const code = useRoomCode(room?.id);

  /** Имена и аватары авторов, в том числе покинувших комнату */
  const users = useChatUsers(room);

  /** Флаг наличия дополнительных сообщений */
  const hasMore = !isExhausted && messages.length < totalCount;

  /** ID последнего (самого старого) сообщения для пагинации */
  const lastMessageId = messages[messages.length - 1]?.id;

  /**
   * Кэш преобразованных сообщений. Пересоздается при изменении авторов,
   * иначе уже показанные сообщения сохраняют ссылку и не перерисовываются
   */
  const dtoCache = useMemo(
    () => new WeakMap<MessageResponse, MessageDto>(),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [users, currentViewerId, room?.ownerId]
  );

  /** Преобразует серверные сообщения в DTO для отображения */
  const mappedMessages = useMemo<MessageDto[]>(() => {
    return messages.map((m) => {
      const cached = dtoCache.get(m);
      if (cached) return cached;

      const user = users.get(m.userId);
      const dto: MessageDto = {
        id: m.id,
        isOutgoing: m.userId === currentViewerId,
        isOwner: m.userId === room?.ownerId,
        sentAt: new Date(m.sentAt),
        photoUrl: user?.photoUrl ?? null,
        text: m.text,
        userName: user?.userName ?? 'Зритель',
        reactions: summarizeReactions(m, users, currentViewerId),
      };
      dtoCache.set(m, dto);
      return dto;
    });
  }, [currentViewerId, dtoCache, messages, room?.ownerId, users]);

  /** Обрабатывает события хаба комнаты */
  useEffect(() => {
    if (!hub) return;

    const handler = (e: RoomEventContainer) => {
      if (e.messagesEvent) {
        const page = e.messagesEvent.messages;
        setMessages((prev) => mergeMessages(prev, page.list));
        setTotalCount(page.totalCount);
        if (page.list.length < PAGE_SIZE) setIsExhausted(true);
        setIsLoading(false);
      } else if (e.messageEvent) {
        const message = e.messageEvent.message;
        setMessages((prev) =>
          prev.some((m) => m.id === message.id) ? prev : mergeMessages(prev, [message])
        );
        setTotalCount((c) => c + 1);
      } else if (e.messageReactionEvent) {
        const reactionEvent = e.messageReactionEvent;
        setMessages((prev) => prev.map((m) => applyReaction(m, reactionEvent)));
      }
    };

    hub.addHandler(handler);
    return () => hub.removeHandler(handler);
  }, [hub]);

  /** Загрузка сообщений из хаба */
  const get = useSafeCallback(
    async (fromId?: string) => {
      try {
        await hub?.getMessages(fromId, PAGE_SIZE);
      } catch (e) {
        setIsLoading(false);
        throw e;
      }
    },
    [hub]
  );

  /** Первичная загрузка сообщений и повторная загрузка после переподключения */
  useEffect(() => {
    get().then();
    return () => {
      setMessages([]);
      setIsLoading(true);
      setTotalCount(0);
      setIsExhausted(false);
    };
  }, [get, reconnectCount]);

  /** Загрузка более старых сообщений */
  const next = useCallback(() => {
    get(lastMessageId).then();
  }, [get, lastMessageId]);

  /** Безопасная отправка сообщения */
  const safeSendMessage = useSafeCallback(
    async (text: string) => {
      if (!hub) return false;
      await hub.sendMessage(text);
      return true;
    },
    [hub]
  );

  /**
   * Отправка сообщения
   * @param text - текст сообщения
   * @returns {Promise<boolean>} true, если сообщение отправлено
   */
  const sendMessage = useCallback(
    async (text: string): Promise<boolean> => (await safeSendMessage(text)) === true,
    [safeSendMessage]
  );

  /** Установка или снятие реакции на сообщение */
  const toggleReaction = useSafeCallback(
    async (messageId: string, reaction: string) => {
      await hub?.toggleReaction(messageId, reaction);
    },
    [hub]
  );

  /** Отправка события печати */
  const sendTyping = useSafeCallback(async () => {
    await hub?.type();
  }, [hub]);

  /** Момент последней отправки события печати */
  const lastTypingRef = useRef(0);

  /** Отправка события печати не чаще одного раза в TYPING_THROTTLE мс */
  const onTyping = useCallback(() => {
    const now = Date.now();
    if (now - lastTypingRef.current < TYPING_THROTTLE) return;
    lastTypingRef.current = now;
    sendTyping().then();
  }, [sendTyping]);

  /** Сообщение о печатающих зрителях */
  const typingMessage = useTypingMessage(room, currentViewerId);

  if (!room || isLoading)
    return (
      <Paper>
        <MessagesListSkeleton />
        <Skeleton variant="rounded" width="100%" height={76} sx={{ mb: 1 }} />
        <Stack direction="row" sx={{ alignItems: 'center' }}>
          <Skeleton variant="circular" width={16} height={16} sx={{ mr: 1 }} />
          <Skeleton variant="text" width={120} height={20} />
        </Stack>
      </Paper>
    );

  return (
    <Paper>
      <MessagesList
        next={next}
        hasMore={hasMore}
        messages={mappedMessages}
        onReact={toggleReaction}
      />
      <TypingIndicator message={typingMessage} />
      <SendMessageForm onTyping={onTyping} onSend={sendMessage} />
      <ConnectLink code={code} id={room.id} />
    </Paper>
  );
};

export default RoomChatModule;

/**
 * Объединяет список сообщений с новыми сообщениями без дублей
 * @param prev - текущий список сообщений (от новых к старым)
 * @param incoming - полученные сообщения
 * @returns {MessageResponse[]} Объединенный список от новых к старым
 */
function mergeMessages(prev: MessageResponse[], incoming: MessageResponse[]): MessageResponse[] {
  const byId = new Map(prev.map((m) => [m.id, m]));
  for (const m of incoming) byId.set(m.id, m);
  return Array.from(byId.values()).sort((a, b) => Date.parse(b.sentAt) - Date.parse(a.sentAt));
}

/**
 * Хук хранит имена и аватары авторов сообщений.
 * Заполняется из зрителей комнаты и не очищается при выходе зрителя,
 * чтобы его сообщения не теряли имя и аватар
 * @param room - текущее состояние комнаты
 * @returns {Map<string, ChatUser>} Авторы по идентификатору пользователя
 */
function useChatUsers(room: RoomDto | null): Map<string, ChatUser> {
  /** Накопленные данные авторов */
  const cacheRef = useRef(new Map<string, ChatUser>());
  const viewers = room?.viewers;

  return useMemo(() => {
    const cache = cacheRef.current;
    viewers?.forEach((v, id) => cache.set(id, { userName: v.userName, photoUrl: v.photoUrl }));
    return new Map(cache);
  }, [viewers]);
}

/**
 * Собирает сводку реакций сообщения для отображения: по одной записи на поставленную реакцию
 * @param message - сообщение с сервера
 * @param users - авторы (для имён зрителей)
 * @param currentViewerId - id текущего пользователя
 * @returns {MessageReactionDto[]} Реакции в порядке {@link REACTIONS}, без реакций с нулевым количеством
 */
function summarizeReactions(
  message: MessageResponse,
  users: Map<string, ChatUser>,
  currentViewerId: string
): MessageReactionDto[] {
  return REACTIONS.map((option) => {
    const userIds = message.reactions
      .filter((r) => r.reaction === option.code)
      .map((r) => r.userId);
    return {
      code: option.code,
      emoji: option.emoji,
      count: userIds.length,
      reacted: userIds.includes(currentViewerId),
      userNames: userIds.map((id) => users.get(id)?.userName ?? 'Зритель'),
    };
  }).filter((r) => r.count > 0);
}

/**
 * Применяет к сообщению событие установки или снятия реакции
 * @param message - сообщение с сервера
 * @param event - событие реакции
 * @returns {MessageResponse} Сообщение с обновлёнными реакциями (тот же объект, если событие не про него)
 */
function applyReaction(message: MessageResponse, event: MessageReactionEvent): MessageResponse {
  if (message.id !== event.messageId) return message;

  const rest = message.reactions.filter(
    (r) => !(r.userId === event.viewerId && r.reaction === event.reaction)
  );

  return {
    ...message,
    reactions: event.added ? [...rest, { userId: event.viewerId, reaction: event.reaction }] : rest,
  };
}

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

import RoomDto, { ViewerDto, ViewerStateDto } from './room.dto.ts';
import MessageEvent from '../../services/room-hub/events/messages/message.event.ts';
import TypingEvent from '../../services/room-hub/events/messages/typing.event.ts';
import UpdateViewerPlayerEvent from '../../services/room-hub/events/player/update-viewer-player.event.ts';
import JoinEvent from '../../services/room-hub/events/room/join.event.ts';
import LeaveEvent from '../../services/room-hub/events/room/leave.event.ts';
import RoomEvent from '../../services/room-hub/events/room/room.event.ts';
import UpdateViewerEvent from '../../services/room-hub/events/room/update-viewer.event.ts';
import { ViewerResponse } from '../../services/room-hub/responses/viewer.response.ts';

/**
 * Разбивает ViewerResponse на ViewerDto и ViewerStateDto
 * @param v - Объект ответа сервера с данными зрителя
 * @returns Кортеж из ViewerDto и ViewerStateDto
 */
function splitViewer(v: ViewerResponse): [ViewerDto, ViewerStateDto] {
  return [
    {
      userName: v.userName,
      photoUrl: v.photoUrl,
      online: v.online,
      settings: v.settings,
      tags: v.tags,
    },
    {
      onPause: v.onPause,
      fullScreen: v.fullScreen,
      timeLine: v.timeLine,
      season: v.season,
      episode: v.episode,
      speed: v.speed,
      typing: false,
      typingSetTime: null,
    },
  ];
}

/**
 * Обработка события полной инициализации комнаты (RoomEvent)
 * @param event - Событие с полными данными комнаты
 * @returns Новое состояние RoomDto
 */
export const roomEventHandler = (event: RoomEvent): RoomDto => {
  const viewers = new Map<string, ViewerDto>();
  const players = new Map<string, ViewerStateDto>();

  for (const v of event.room.viewers) {
    const [viewerDto, playerDto] = splitViewer(v);
    viewers.set(v.id, viewerDto);
    players.set(v.id, playerDto);
  }

  return {
    id: event.room.id,
    ownerId: event.room.ownerId,
    title: event.room.title,
    isSerial: event.room.isSerial,
    viewers,
    viewerStates: players,
  };
};

/**
 * Обработка события подключения пользователя
 * @param room - Текущее состояние комнаты
 * @param event - Событие подключения нового зрителя
 * @returns Новое состояние RoomDto с добавленным зрителем
 */
export const connectEventHandler = (room: RoomDto, event: JoinEvent): RoomDto => {
  const [viewerDto, playerDto] = splitViewer(event.viewer);

  return {
    ...room,
    viewers: new Map(room.viewers).set(event.viewer.id, viewerDto),
    viewerStates: new Map(room.viewerStates).set(event.viewer.id, playerDto),
  };
};

/**
 * Обработка события выхода пользователя
 * @param room - Текущее состояние комнаты
 * @param event - Событие выхода зрителя
 * @returns Новое состояние RoomDto без ушедшего зрителя
 */
export const disconnectEventHandler = (room: RoomDto, event: LeaveEvent): RoomDto => {
  const newViewers = new Map(room.viewers);
  const newViewerStates = new Map(room.viewerStates);

  newViewers.delete(event.viewer);
  newViewerStates.delete(event.viewer);

  return {
    ...room,
    viewers: newViewers,
    viewerStates: newViewerStates,
  };
};

/**
 * Обработка события набора сообщения пользователем
 * @param room - Текущее состояние комнаты
 * @param event - Событие начала печати
 * @returns Новое состояние RoomDto с обновленным флагом typing
 */
export const typingEventHandler = (room: RoomDto, event: TypingEvent): RoomDto => {
  const exists = room.viewerStates.has(event.initiator);
  if (!exists) return room;

  const newViewerStates = new Map(room.viewerStates);
  const viewer = newViewerStates.get(event.initiator)!;
  viewer.typing = true;
  viewer.typingSetTime = new Date();

  return {
    ...room,
    viewerStates: newViewerStates,
  };
};

/**
 * Обработка события отправки сообщения пользователем
 * @param room - Текущее состояние комнаты
 * @param event - Событие отправки сообщения
 * @returns Новое состояние RoomDto с обновленным флагом typing
 */
export const messageEventHandler = (room: RoomDto, event: MessageEvent): RoomDto => {
  const exists = room.viewerStates.has(event.message.userId);
  if (!exists) return room;

  const newViewerStates = new Map(room.viewerStates);
  const viewer = newViewerStates.get(event.message.userId)!;

  if (!viewer.typing) return room;

  viewer.typing = false;
  viewer.typingSetTime = null;

  return {
    ...room,
    viewerStates: newViewerStates,
  };
};

/**
 * Обработка события обновления данных зрителя (viewer-часть)
 * @param room - Текущее состояние комнаты
 * @param event - Событие обновления зрителя
 * @returns Новое состояние RoomDto с обновленными данными зрителя
 */
export const updateViewerEventHandler = (room: RoomDto, event: UpdateViewerEvent): RoomDto => {
  const exists = room.viewers.has(event.id);
  if (!exists) return room;

  const newViewers = new Map(room.viewers);
  const oldViewer = room.viewers.get(event.id)!;

  const partialUpdate = Object.fromEntries(
    event.updatedFields.map((f) => [f, (event as never)[f]])
  ) as Partial<ViewerDto>;

  newViewers.set(event.id, { ...oldViewer, ...partialUpdate });

  return {
    ...room,
    viewers: newViewers,
  };
};

/**
 * Обработка события обновления данных зрителя (player-часть)
 * @param room - Текущее состояние комнаты
 * @param event - Событие обновления состояния проигрывателя зрителя
 * @returns Новое состояние RoomDto с обновленным состоянием player
 */
export const updateViewerPlayerEventHandler = (
  room: RoomDto,
  event: UpdateViewerPlayerEvent
): RoomDto => {
  const exists = room.viewerStates.has(event.id);
  if (!exists) return room;

  const newViewerStates = new Map(room.viewerStates);
  const oldPlayer = room.viewerStates.get(event.id)!;

  const partialUpdate = Object.fromEntries(
    event.updatedFields.map((f) => [f, (event as never)[f]])
  ) as Partial<ViewerStateDto>;

  newViewerStates.set(event.id, { ...oldPlayer, ...partialUpdate });

  return {
    ...room,
    viewerStates: newViewerStates,
  };
};

/**
 * Хендлер тика таймера.
 * Увеличивает timeLine у всех игроков, которые не на паузе
 * @param room - Текущее состояние комнаты
 * @returns Новое состояние RoomDto с обновленным timeLine
 */
export const tickViewerTimeLineHandler = (room: RoomDto): RoomDto => {
  const newViewerStates = new Map(room.viewerStates);
  let someModified = false;

  newViewerStates.forEach((player, id) => {
    if (!player.onPause) {
      someModified = true;
      newViewerStates.set(id, {
        ...player,
        timeLine: player.timeLine + 10_000_000 * player.speed,
      });
    }
  });

  return someModified ? { ...room, viewerStates: newViewerStates } : room;
};

/**
 * Хендлер очистки устаревшего состояния "печатает"
 * Если прошло более 5 секунд с момента typingSetTime, сбрасывает typing
 * @param room - Текущее состояние комнаты
 * @returns Новое состояние RoomDto с обновленными флагами typing
 */
export const clearStaleTypingHandler = (room: RoomDto): RoomDto => {
  const now = Date.now();
  const newViewerStates = new Map(room.viewerStates);
  let someModified = false;

  newViewerStates.forEach((state, id) => {
    if (state.typing && state.typingSetTime) {
      const elapsed = now - state.typingSetTime.getTime();
      if (elapsed > 5000) {
        someModified = true;
        newViewerStates.set(id, {
          ...state,
          typing: false,
          typingSetTime: null,
        });
      }
    }
  });

  return someModified ? { ...room, viewerStates: newViewerStates } : room;
};

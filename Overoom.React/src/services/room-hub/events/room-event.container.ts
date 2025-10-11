import MessageEvent from './messages/message.event.ts';
import MessagesEvent from './messages/messages.event.ts';
import TypingEvent from './messages/typing.event.ts';
import { BeepNotificationEvent } from './notifications/beep-notification.event.ts';
import { DeleteNotificationEvent } from './notifications/delete-notification.event.ts';
import { ErrorNotificationEvent } from './notifications/error-notification.event.ts';
import { JoinNotificationEvent } from './notifications/join-notification.event.ts';
import { KickNotificationEvent } from './notifications/kick-notification.event.ts';
import { LeaveNotificationEvent } from './notifications/leave-notification.event.ts';
import { ScreamNotificationEvent } from './notifications/scream-notification.event.ts';
import EpisodeEvent from './player/episode.event.ts';
import PauseEvent from './player/pause.event.ts';
import SpeedEvent from './player/speed.event.ts';
import TimeLineEvent from './player/time-line.event.ts';
import UpdateViewerPlayerEvent from './player/update-viewer-player.event.ts';
import { ConnectEvent } from './room/connect.event.ts';
import JoinEvent from './room/join.event.ts';
import LeaveEvent from './room/leave.event.ts';
import RoomEvent from './room/room.event.ts';
import UpdateViewerEvent from './room/update-viewer.event.ts';

/** Главный интерфейс, объединяющий все возможные события комнаты */
export interface RoomEventContainer {
  /** Событие установки связи с текущим пользователем */
  connectEvent?: ConnectEvent;

  /** Событие подключения пользователя */
  joinEvent?: JoinEvent;

  /** Событие данных комнаты */
  roomEvent?: RoomEvent;

  /** Событие нового сообщения */
  messageEvent?: MessageEvent;

  /** Событие истории сообщений */
  messagesEvent?: MessagesEvent;

  /** Событие набора сообщения */
  typingEvent?: TypingEvent;

  /** Событие обновления зрителя */
  updateViewerEvent?: UpdateViewerEvent;

  /** Событие обновления данных плеера зрителя */
  updateViewerPlayerEvent?: UpdateViewerPlayerEvent;

  /** Событие выхода пользователя */
  leaveEvent?: LeaveEvent;

  /** Событие установки паузы */
  pauseEvent?: PauseEvent;

  /** Событие смены скорости */
  speedEvent?: SpeedEvent;

  /** Событие установки таймлайна */
  timeLineEvent?: TimeLineEvent;

  /** Событие изменения номера сезона и серии */
  episodeEvent?: EpisodeEvent;

  /** Событие удаления комнаты */
  deleteNotificationEvent?: DeleteNotificationEvent;

  /** Событие уведомления о скримере для пользователя */
  screamNotificationEvent?: ScreamNotificationEvent;

  /** Событие уведомления о звуковом сигнале для пользователя */
  beepNotificationEvent?: BeepNotificationEvent;

  /** Событие уведомления о подключении пользователя к комнате */
  joinNotificationEvent?: JoinNotificationEvent;

  /** Событие уведомления о выходе пользователя из комнаты */
  leaveNotificationEvent?: LeaveNotificationEvent;

  /** Событие уведомления об исключении пользователя из комнаты */
  kickNotificationEvent?: KickNotificationEvent;

  /** Событие уведомления об ошибке в комнате */
  errorNotificationEvent?: ErrorNotificationEvent;
}

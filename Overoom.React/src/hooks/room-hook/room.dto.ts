import { RoomSettingsResponse } from '../../services/common/room-settings.response.ts';
import { ViewerTagResponse } from '../../services/room-hub/responses/viewer-tag.response.ts';

/** Данные комнаты */
export default interface RoomDto {
  /** Идентификатор комнаты */
  id: string;
  /** Идентификатор владельца комнаты */
  ownerId: string;
  /** Список зрителей в комнате, ключ — userId, значение — данные зрителя */
  viewers: Map<string, ViewerDto>;
  /** Состояния зрителей, ключ — userId, значение — состояние зрителя */
  viewerStates: Map<string, ViewerStateDto>;
  /** Название комнаты */
  title: string;
  /** Флаг, указывающий, является ли контент сериалом */
  isSerial: boolean;
}

/** Данные зрителя */
export interface ViewerDto {
  /** Имя пользователя */
  userName: string;
  /** URL фото пользователя, если есть */
  photoUrl: string | null;
  /** Флаг, указывающий, онлайн ли зритель */
  online: boolean;
  /** Настройки зрителя для комнаты */
  settings: RoomSettingsResponse;
  /** Список тегов зрителя */
  tags: ViewerTagResponse[];
}

/** Состояние зрителя в комнате */
export interface ViewerStateDto {
  /** Флаг, указывающий, поставил ли зритель паузу */
  onPause: boolean;
  /** Флаг, указывающий, включен ли полноэкранный режим */
  fullScreen: boolean;
  /** Текущее время воспроизведения видео */
  timeLine: number;
  /** Скорость воспроизведения видео */
  speed: number;
  /** Текущий сезон (если сериал) */
  season: number | null;
  /** Текущая серия (если сериал) */
  episode: number | null;
  /** Флаг, указывающий, печатает ли зритель сообщение */
  typing: boolean;
  /** Время установки флага typing */
  typingSetTime: Date | null;
}

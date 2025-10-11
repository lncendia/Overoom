import { ViewerTagResponse } from './viewer-tag.response.ts';
import { RoomSettingsResponse } from '../../common/room-settings.response.ts';

/** Данные зрителя в комнате */
export interface ViewerResponse {
  /** Идентификатор зрителя */
  id: string;
  /** Имя пользователя */
  userName: string;
  /** Ключ фотографии в хранилище */
  photoKey: string | null;
  /** URL фотографии пользователя */
  photoUrl: string | null;
  /** Флаг паузы воспроизведения */
  onPause: boolean;
  /** Флаг полноэкранного режима */
  fullScreen: boolean;
  /** Флаг онлайн-статуса */
  online: boolean;
  /** Текущая позиция воспроизведения */
  timeLine: number;
  /** Скорость воспроизведения */
  speed: number;
  /** Настройки комнаты пользователя */
  settings: RoomSettingsResponse;
  /** Текущий сезон (для сериалов) */
  season: number | null;
  /** Текущая серия (для сериалов) */
  episode: number | null;
  /** Список тегов зрителя */
  tags: ViewerTagResponse[];
}

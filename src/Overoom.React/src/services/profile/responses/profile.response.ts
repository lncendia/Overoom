import { RoomSettingsResponse } from '../../common/room-settings.response.ts';

/** Данные профиля пользователя */
export interface ProfileResponse {
  /** Имя пользователя */
  userName: string;
  /** Ключ фотографии в хранилище */
  photoKey: string | null;
  /** URL фотографии пользователя */
  photoUrl: string | null;
  /** Настройки комнаты пользователя */
  roomSettings: RoomSettingsResponse;
  /** Предпочтительные жанры пользователя */
  genres: string[];
}

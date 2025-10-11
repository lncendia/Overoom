import { RoomSettingsResponse } from '../../../common/room-settings.response.ts';
import { ViewerTagResponse } from '../../responses/viewer-tag.response.ts';

/** Модель данных для события паузы */
export default interface UpdateViewerEvent {
  /** Идентификатор комнаты */
  id: string;

  /** Имя пользователя */
  userName: string | null;

  /** Ключ фотографии */
  photoKey: string | null;

  /** Флаг онлайн-статуса */
  online: boolean | null;

  /** Права пользователя на действия в комнате */
  settings: RoomSettingsResponse | null;

  /** Список тегов зрителя */
  tags: ViewerTagResponse[] | null;

  /** Список обновленных полей */
  updatedFields: string[];
}

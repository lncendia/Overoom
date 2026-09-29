import { ViewerResponse } from '../../responses/viewer.response.ts';

/** Модель данных для события подключения пользователя */
export default interface JoinEvent {
  /** Данные зрителя */
  viewer: ViewerResponse;
}

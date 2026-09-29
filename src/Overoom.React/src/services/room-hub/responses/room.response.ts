import { ViewerResponse } from './viewer.response.ts';

/** Полные данные комнаты с информацией о зрителях */
export default interface RoomResponse {
  /** Идентификатор комнаты */
  id: string;
  /** Идентификатор владельца комнаты */
  ownerId: string;
  /** Список зрителей в комнате */
  viewers: ViewerResponse[];
  /** Название комнаты/фильма */
  title: string;
  /** Флаг сериала */
  isSerial: boolean;
}

import RoomResponse from '../../responses/room.response.ts';

/** Модель данных для события получения данных комнаты */
export default interface RoomEvent {
  /** Данные комнаты */
  room: RoomResponse;
}

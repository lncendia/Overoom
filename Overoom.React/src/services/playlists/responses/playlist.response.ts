/** Данные плейлиста */
export interface PlaylistResponse {
  /** Идентификатор плейлиста */
  id: string;
  /** Название плейлиста */
  name: string;
  /** URL постера плейлиста */
  posterUrl: string;
  /** Ключ постера в хранилище */
  posterKey: string;
  /** Описание плейлиста */
  description: string;
  /** Жанры плейлиста */
  genres: string[];
  /** Время последнего обновления */
  updated: string;
}

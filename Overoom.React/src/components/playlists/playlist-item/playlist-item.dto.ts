/** Интерфейс элемента плейлиста */
export interface PlaylistItemDto {
  /** Уникальный идентификатор плейлиста */
  id: string;
  /** Название плейлиста */
  name: string;
  /** Описание плейлиста */
  description: string;
  /** URL постера плейлиста */
  posterUrl: string;
  /** Дата последнего обновления плейлиста */
  updated: string;
  /** Жанры плейлиста */
  genres: string[];
}

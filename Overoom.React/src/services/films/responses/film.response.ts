import { ActorResponse } from './actor.response.ts';
import { FilmShortResponse } from './film-short.response.ts';
import { MediaContentResponse } from './media-content.response.ts';
import { SeasonResponse } from './season.response.ts';

/** Полные данные фильма с расширенной информацией */
export interface FilmResponse extends FilmShortResponse {
  /** Рейтинг текущего пользователя (опционально) */
  userRating: number | null;
  /** Количество оценок пользователей */
  userRatingsCount: number;
  /** Средняя оценка пользователей (опционально) */
  userScore: number | null;
  /** Флаг наличия в watchlist пользователя (опционально) */
  inWatchlist: boolean | null;
  /** Медиаконтент фильма (опционально) */
  content: MediaContentResponse | null;
  /** Сезоны сериала (опционально) */
  seasons: SeasonResponse[] | null;
  /** Страны производства */
  countries: string[];
  /** Режиссеры фильма */
  directors: string[];
  /** Сценаристы фильма */
  screenWriters: string[];
  /** Актеры фильма */
  actors: ActorResponse[];
  /**  Флаг, может ли быть создана комната с этим фильмом */
  canCreateRoom: boolean;
}

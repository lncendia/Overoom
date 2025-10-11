import { FilmShortResponse } from '../../films/responses/film-short.response.ts';

/** Рейтинг пользователя с информацией о фильме */
export interface RatingResponse extends FilmShortResponse {
  /** Оценка пользователя */
  score: number;
}

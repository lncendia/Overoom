import { EpisodeResponse } from './episode.response.ts';

/** Данные сезона сериала с эпизодами */
export interface SeasonResponse {
  /** Номер сезона */
  number: number;
  /** Эпизоды сезона */
  episodes: EpisodeResponse[];
}

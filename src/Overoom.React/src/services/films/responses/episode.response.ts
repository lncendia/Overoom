import { MediaContentResponse } from './media-content.response.ts';

/** Данные эпизода сериала с медиаконтентом */
export interface EpisodeResponse extends MediaContentResponse {
  /** Номер эпизода в сезоне */
  number: number;
}

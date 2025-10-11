/** DTO сезона сериала */
export interface SeasonDto {
  /** Номер сезона */
  number: number;
  /** Эпизоды сезона */
  episodes: EpisodeDto[];
}

/** DTO эпизода сериала */
export interface EpisodeDto extends MediaDto {
  /** Номер эпизода */
  number: number;
}

/** Базовый DTO медиаконтента */
export interface MediaDto {
  /** Версии медиаконтента */
  versions: VersionDto[];
}

/** DTO версии медиаконтента */
export interface VersionDto {
  /** Название версии */
  name: string;
  /** URL источника */
  src: string;
}

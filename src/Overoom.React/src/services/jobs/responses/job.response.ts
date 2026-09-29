/** Обобщённое состояние фоновой задачи */
export type JobStatus = 'Pending' | 'Running' | 'Completed' | 'Faulted' | 'Canceled' | 'Unknown';

/** Этап обработки фильма, на котором находится задача */
export type JobStage = 'Downloading' | 'Transcoding' | 'Uploading' | 'Publishing';

/** Разрешение исходного видео */
export type FilmResolution = 'P360' | 'P480' | 'P720' | 'P1080' | 'P2160';

/** Метаданные фильма, для которого выполняется задача */
export interface JobFilmResponse {
  /** Идентификатор фильма */
  id: string;
  /** Разрешение исходного видео */
  resolution: FilmResolution;
  /** Версия (перевод, редакция) */
  version: string;
  /** Номер сезона */
  season: number | null;
  /** Номер серии */
  episode: number | null;
}

/** Фоновая задача загрузки фильма */
export interface JobResponse {
  /** Идентификатор задачи */
  id: string;
  /** Обобщённое состояние */
  status: JobStatus;
  /** Название состояния в терминах MassTransit */
  state: string;
  /** Время постановки в очередь */
  submitted: string | null;
  /** Время запуска */
  started: string | null;
  /** Время успешного завершения */
  completed: string | null;
  /** Время ошибки */
  faulted: string | null;
  /** Причина ошибки или отмены */
  reason: string | null;
  /** Номер последней попытки */
  retryAttempt: number;
  /** Прогресс в процентах */
  progress: number | null;
  /** Этап, на котором задача выполняется, упала или с которого продолжит */
  stage: JobStage | null;
  /** Фильм */
  film: JobFilmResponse | null;
  /** Название фильма */
  filmTitle: string | null;
  /** Magnet-ссылка */
  magnetUri: string | null;
  /** Имя файла внутри торрента */
  fileName: string | null;
}

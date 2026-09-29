import { FilmResolution } from '../responses/job.response.ts';

/** Данные для постановки задачи загрузки фильма в очередь */
export interface SubmitJobRequest {
  /** Идентификатор фильма */
  filmId: string;
  /** Название фильма для отображения в списке задач */
  filmTitle?: string;
  /** Magnet-ссылка */
  magnetUri: string;
  /** Имя файла внутри торрента (обязательно, если в торренте несколько файлов) */
  fileName?: string;
  /** Разрешение исходного видео */
  resolution: FilmResolution;
  /** Версия (перевод, редакция) */
  version: string;
  /** Номер сезона */
  season?: number;
  /** Номер серии */
  episode?: number;
}

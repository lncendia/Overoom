import { useInjection } from 'inversify-react';
import { ReactElement, useMemo } from 'react';

import FilmPlayerSkeleton from '../../../components/film/film-player/FilmPlayer.skeleton.tsx';
import FilmPlayer from '../../../components/film/film-player/FilmPlayer.tsx';
import {
  EpisodeDto,
  MediaDto,
  SeasonDto,
  VersionDto,
} from '../../../components/film/film-player/media.dto.ts';
import { Configuration } from '../../../container/configuration.ts';
import { useFilm } from '../../../contexts/film-context/useFilm.tsx';
import { FilmResponse } from '../../../services/films/responses/film.response.ts';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Модуль плеера для воспроизведения фильма или сериала.
 * Отображает плеер с контентом или сезоны/эпизоды.
 * @returns {ReactElement} JSX-элемент модуля плеера
 */
const FilmPlayerModule = (): ReactElement => {
  /** Получение данных текущего фильма */
  const { film } = useFilm();

  /** Получение конфигурации приложения */
  const config = useInjection<Configuration>('Configuration');

  /** Доступный контент фильма */
  const content = useFilmContent(film, config);

  /** Сезоны и эпизоды фильма/сериала */
  const seasons = useFilmSeasons(film, config);

  // Показываем скелетон, если данные фильма ещё не загружены
  if (!film) return <FilmPlayerSkeleton />;

  // Если нет ни контента, ни сезонов, показываем сообщение "Нет данных"
  if (!seasons && !content) {
    return <NoData text="Фильм еще не загружен" />;
  }

  // Основной рендер: компонент плеера с контентом и сезонами
  return <FilmPlayer content={content} seasons={seasons} />;
};

export default FilmPlayerModule;

/**
 * Хук для получения контента фильма
 * @param film - текущий фильм
 * @param config - конфигурация приложения
 * @returns {MediaDto | undefined} Объект с доступными версиями видео или undefined
 */
const useFilmContent = (film: FilmResponse | null, config: Configuration): MediaDto | undefined => {
  return useMemo<MediaDto | undefined>(() => {
    if (!film?.content) return undefined;

    const versions = film.content.versions.map<VersionDto>((v) => ({
      name: v,
      src: `${config.services.films.baseURL}${config.files.hlsPrefix}/${film.id}/${v}/master.m3u8`,
    }));

    return { versions };
  }, [config.files.hlsPrefix, config.services.films.baseURL, film?.id, film?.content]);
};

/**
 * Хук для получения сезонов и эпизодов фильма/сериала
 * @param film - текущий фильм
 * @param config - конфигурация приложения
 * @returns {SeasonDto[] | undefined} Массив сезонов с эпизодами и версиями видео или undefined
 */
const useFilmSeasons = (
  film: FilmResponse | null,
  config: Configuration
): SeasonDto[] | undefined => {
  return useMemo<SeasonDto[] | undefined>(() => {
    if (!film?.seasons) return undefined;

    return film.seasons.map<SeasonDto>((s) => ({
      number: s.number,
      episodes: s.episodes.map<EpisodeDto>((e) => ({
        number: e.number,
        versions: e.versions.map<VersionDto>((v) => ({
          name: v,
          src: `${config.services.films.baseURL}${config.files.hlsPrefix}/${film.id}/${s.number}/${e.number}/${v}/master.m3u8`,
        })),
      })),
    }));
  }, [config.files.hlsPrefix, config.services.films.baseURL, film?.id, film?.seasons]);
};

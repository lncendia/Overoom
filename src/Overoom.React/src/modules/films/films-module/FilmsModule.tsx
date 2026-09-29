import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

import FilmsCatalogSkeleton from '../../../components/films/films-catalog/FilmsCatalog.skeleton.tsx';
import FilmsCatalog from '../../../components/films/films-catalog/FilmsCatalog.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { routes } from '../../../routes.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';

/** Пропсы компонента FilmsModule */
interface FilmsModuleProps {
  /** Жанр для фильтрации фильмов */
  genre?: string;
  /** Персона для фильтрации фильмов */
  person?: string;
  /** Страна для фильтрации фильмов */
  country?: string;
  /** Флаг, указывающий на сериалы */
  serial?: boolean;
  /** Идентификатор плейлиста */
  playlistId?: string;
  /** Год выпуска фильма */
  year?: number;
}

/**
 * Компонент для отображения каталога фильмов с поддержкой фильтров и пагинации.
 * @param props - Параметры фильтрации и отображения фильмов
 * @param props.genre - Жанр для фильтрации
 * @param props.person - Персона для фильтрации
 * @param props.country - Страна для фильтрации
 * @param props.serial - Флаг сериалов
 * @param props.playlistId - Идентификатор подборки
 * @param props.year - Год выпуска
 * @returns {ReactElement} JSX-элемент каталога фильмов
 */
const FilmsModule = ({
  genre,
  person,
  country,
  serial,
  playlistId,
  year,
}: FilmsModuleProps): ReactElement => {
  /** Сервис для работы с API фильмов */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /**
   * Функция поиска фильмов с учетом фильтров и пагинации
   * @param skip - количество пропускаемых фильмов
   * @param take - количество загружаемых фильмов
   * @returns Promise с результатами поиска фильмов
   */
  const fetch = useCallback(
    (skip: number, take: number) => {
      return filmsApi.search({
        genre,
        person,
        country,
        serial,
        playlistId,
        minYear: year,
        maxYear: year,
        skip: skip,
        take: take,
      });
    },
    [filmsApi, genre, person, country, serial, playlistId, year]
  );

  /** Хук для порционно-загружаемых фильмов */
  const {
    items: films,
    isLoading,
    hasMore,
    fetchMore,
    error,
    retry,
  } = usePaginatedFetch<FilmShortResponse>(fetch, 20);

  if (isLoading) return <FilmsCatalogSkeleton />;
  if (error && films.length === 0) return <LoadError onRetry={retry} />;
  if (films.length === 0) return <NoData text="Фильмы не найдены" />;

  return (
    <>
      <FilmsCatalog
        hasMore={hasMore}
        next={fetchMore}
        genre={genre}
        films={films}
        getLink={routes.film}
        typeSelected={serial !== undefined}
      />
      {error && <LoadError onRetry={retry} />}
    </>
  );
};

export default FilmsModule;

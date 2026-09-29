import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

import FilmsSliderSkeleton from '../../../components/films/films-slider/FilmsSlider.skeleton.tsx';
import FilmsSlider from '../../../components/films/films-slider/FilmsSlider.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { routes } from '../../../routes.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Модуль для отображения популярных фильмов в виде слайдера.
 * Позволяет пользователю просматривать популярные фильмы и выбирать конкретный фильм.
 * @returns {ReactElement} JSX-элемент слайдера популярных фильмов
 */
const PopularFilmsModule = (): ReactElement => {
  /** Сервис для работы с API фильмов */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /**
   * Функция для загрузки популярных фильмов с поддержкой пагинации
   * @param _skip - количество пропускаемых фильмов (не используется, оставлено для совместимости)
   * @param take - количество загружаемых фильмов
   * @returns Promise с объектом, содержащим список фильмов и общее количество
   */
  const fetch = useCallback(
    async (_skip: number, take: number) => {
      const films = await filmsApi.getPopular(take);
      return {
        list: films,
        totalCount: films.length,
      };
    },
    [filmsApi]
  );

  /** Хук для порционно-загружаемых фильмов */
  const { items: films, isLoading, error, retry } = usePaginatedFetch<FilmShortResponse>(fetch, 20);

  if (isLoading) return <FilmsSliderSkeleton />;
  if (error) return <LoadError onRetry={retry} />;
  if (films.length === 0) return <NoData text="Популярных фильмов пока нет" />;

  return <FilmsSlider films={films} getLink={routes.film} />;
};

export default PopularFilmsModule;

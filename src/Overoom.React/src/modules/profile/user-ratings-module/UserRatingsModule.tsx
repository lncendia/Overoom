import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

import FilmsListSkeleton from '../../../components/films/films-list/FilmsList.skeleton.tsx';
import FilmsList from '../../../components/films/films-list/FilmsList.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { routes } from '../../../routes.ts';
import { ProfileApi } from '../../../services/profile/profile.api.ts';
import { RatingResponse } from '../../../services/profile/responses/rating.response.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Модуль для отображения оцененных пользователем фильмов.
 * Позволяет загружать и просматривать фильмы с пагинацией.
 * @returns {ReactElement} JSX элемент модуля отображения оцененных пользователем фильмов.
 */
const UserRatingsModule = (): ReactElement => {
  /** Сервис для работы с API профиля */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /**
   * Загружает оценки пользователя
   * @param skip - количество пропускаемых элементов
   * @param take - количество загружаемых элементов
   * @returns Promise с оценками пользователя
   */
  const fetch = useCallback(
    (skip: number, take: number) => {
      return profileApi.getRatings({
        skip: skip,
        take: take,
      });
    },
    [profileApi]
  );

  /** Хук для порционированной загрузки оценок */
  const {
    items: ratings,
    isLoading,
    hasMore,
    fetchMore,
    error,
    retry,
  } = usePaginatedFetch<RatingResponse>(fetch, 20);

  if (isLoading) return <FilmsListSkeleton />;
  if (error && ratings.length === 0) return <LoadError onRetry={retry} />;
  if (ratings.length === 0) return <NoData text="Пусто" />;

  return (
    <>
      <FilmsList hasMore={hasMore} next={fetchMore} films={ratings} getLink={routes.film} />
      {error && <LoadError onRetry={retry} />}
    </>
  );
};

export default UserRatingsModule;

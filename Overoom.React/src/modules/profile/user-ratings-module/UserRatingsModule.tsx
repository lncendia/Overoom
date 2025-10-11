import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmsListSkeleton from '../../../components/films/films-list/FilmsList.skeleton.tsx';
import FilmsList from '../../../components/films/films-list/FilmsList.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { ProfileApi } from '../../../services/profile/profile.api.ts';
import { RatingResponse } from '../../../services/profile/responses/rating.response.ts';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Модуль для отображения оцененных пользователем фильмов.
 * Позволяет загружать и просматривать фильмы с пагинацией.
 * @returns {ReactElement} JSX элемент модуля отображения оцененных пользователем фильмов.
 */
const UserRatingsModule = (): ReactElement => {
  /** Сервис для работы с API профиля */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Загружает оценки пользователя
   * @param skip - количество пропускаемых элементов
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
  } = usePaginatedFetch<RatingResponse>(fetch, 20);

  /**
   * Обработчик выбора фильма
   * @param id - ID выбранного фильма
   */
  const onSelect = useCallback(
    (id: string) => {
      navigate('/film', { state: { id: id } });
    },
    [navigate]
  );

  if (isLoading) return <FilmsListSkeleton />;
  if (ratings.length === 0) return <NoData text="Пусто" />;

  return <FilmsList hasMore={hasMore} next={fetchMore} films={ratings} onSelect={onSelect} />;
};

export default UserRatingsModule;

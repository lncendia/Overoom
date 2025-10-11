import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useEffect, useState } from 'react';

import FilmsListSkeleton from '../../../components/films/films-list/FilmsList.skeleton.tsx';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import { ProfileApi } from '../../../services/profile/profile.api.ts';
import UserFilmsModule from '../user-films-module/UserFilmsModule.tsx';

/**
 * Модуль "Буду смотреть" пользователя.
 * Отображает список фильмов "Буду смотреть".
 * @returns {ReactElement} JSX элемент модуля "Буду смотреть" пользователя.
 */
const UserWatchlistModule = (): ReactElement => {
  /** Список фильмов "Буду смотреть" */
  const [films, setFilms] = useState<FilmShortResponse[]>([]);

  /** Флаг состояния загрузки */
  const [isLoading, setIsLoading] = useState(true);

  /** Сервис для работы с API профиля */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /**
   * Загружает список "Буду смотреть" пользователя
   */
  const fetch = useCallback(async () => {
    const films = await profileApi.getWatchlist();
    setFilms(films);
    setIsLoading(false);
  }, [profileApi]);

  /** Эффект для загрузки данных при монтировании компонента */
  useEffect(() => {
    fetch().then();
    return () => {
      setIsLoading(true);
      setFilms([]);
    };
  }, [fetch]);

  if (isLoading) return <FilmsListSkeleton />;

  return <UserFilmsModule films={films} />;
};

export default UserWatchlistModule;

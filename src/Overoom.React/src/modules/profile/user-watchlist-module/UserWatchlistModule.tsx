import { useInjection } from 'inversify-react';
import { ReactElement, useEffect, useState } from 'react';

import FilmsListSkeleton from '../../../components/films/films-list/FilmsList.skeleton.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import { ProfileApi } from '../../../services/profile/profile.api.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';
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

  /** Счетчик для повторной загрузки */
  const [reloadKey, setReloadKey] = useState(0);

  /** Флаг ошибки загрузки */
  const [isError, setIsError] = useState(false);

  /** Сервис для работы с API профиля */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /**
   * Загружает данные. Загрузка завершается и при ошибке
   * @param isActual - функция, возвращающая false, если компонент уже размонтирован
   */
  const fetch = useSafeCallback(
    async (isActual: () => boolean) => {
      setIsError(false);
      try {
        const films = await profileApi.getWatchlist();
        if (isActual()) setFilms(films);
      } catch (e) {
        if (isActual()) setIsError(true);
        throw e;
      } finally {
        if (isActual()) setIsLoading(false);
      }
    },
    [profileApi]
  );

  /** Эффект для загрузки данных при монтировании компонента */
  useEffect(() => {
    let actual = true;
    fetch(() => actual).then();
    return () => {
      actual = false;
      setIsLoading(true);
      setFilms([]);
    };
  }, [fetch, reloadKey]);

  if (isLoading) return <FilmsListSkeleton />;
  if (isError)
    return (
      <LoadError
        text="Не удалось загрузить список «Смотреть позже»"
        onRetry={() => setReloadKey((key) => key + 1)}
      />
    );

  return <UserFilmsModule films={films} />;
};

export default UserWatchlistModule;

import { useInjection } from 'inversify-react';
import { ReactElement } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmInfoSkeleton from '../../../components/film/film-info/FilmInfo.skeleton.tsx';
import FilmInfo from '../../../components/film/film-info/FilmInfo.tsx';
import { useAuthentication } from '../../../contexts/authentication-context/useAuthentication.tsx';
import { useFilm } from '../../../contexts/film-context/useFilm.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { ProfileApi } from '../../../services/profile/profile.api.ts';

/** Пропсы компонента FilmModule */
interface FilmModuleProps {
  /** Callback при создании комнаты просмотра */
  onButtonClicked: (() => void) | undefined;
  /** Текст кнопки создания комнаты */
  buttonText: string | undefined;
}

/**
 * Модуль для отображения информации о фильме и управления watchlist.
 * @param props - Пропсы компонента
 * @param props.onButtonClicked - Callback при создании комнаты просмотра
 * @param props.buttonText - Текст кнопки создания комнаты просмотра
 * @returns {ReactElement} JSX-элемент модуля информации о фильме
 */
const FilmModule = (props: FilmModuleProps): ReactElement => {
  /** Получение данных фильма и функции его обновления */
  const { film, editFilm } = useFilm();

  /** Сервис для работы с API профиля */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /** Получение данных авторизованного пользователя */
  const { authorizedUser } = useAuthentication();

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /** Переключает статус фильма в watchlist пользователя */
  const toggleWatchlist = useSafeCallback(async () => {
    if (authorizedUser === null || film == null) return;

    // Обновляем локальный статус watchlist в контексте фильма
    editFilm((prev) => ({
      ...prev!,
      inWatchlist: !prev!.inWatchlist,
    }));

    // Обновляем статус watchlist на сервере
    await profileApi.toggleWatchlist(film.id);
  }, [authorizedUser, profileApi, film, editFilm]);

  // Показываем скелетон, если данные фильма ещё не загружены
  if (!film) return <FilmInfoSkeleton />;

  // Основной рендер: компонент FilmInfo с навигацией по фильтрам и управлением watchlist
  return (
    <FilmInfo
      film={film}
      onCountrySelect={(value) => navigate('/search', { state: { country: value } })}
      onGenreSelect={(value) => navigate('/search', { state: { genre: value } })}
      onPersonSelect={(value) => navigate('/search', { state: { person: value } })}
      onYearSelect={(value) => navigate('/search', { state: { year: value } })}
      onTypeSelect={(value) => navigate('/search', { state: { serial: value === 'Сериал' } })}
      isWatchlistEnabled={authorizedUser !== null}
      inWatchlist={film?.inWatchlist ?? false}
      onWatchlistToggle={toggleWatchlist}
      {...props}
    />
  );
};

export default FilmModule;

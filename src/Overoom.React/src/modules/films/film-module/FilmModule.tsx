import { Box, Link } from '@mui/material';
import Typography from '@mui/material/Typography';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmInfoSkeleton from '../../../components/film/film-info/FilmInfo.skeleton.tsx';
import FilmInfo from '../../../components/film/film-info/FilmInfo.tsx';
import { useAuthentication } from '../../../contexts/authentication-context/useAuthentication.tsx';
import { useFilm } from '../../../contexts/film-context/useFilm.tsx';
import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { routes } from '../../../routes.ts';
import { AuthApi } from '../../../services/auth/auth.api.ts';
import { ProfileApi } from '../../../services/profile/profile.api.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';

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
  const { film, editFilm, isError, reload } = useFilm();

  /** Сервис для работы с API профиля */
  const profileApi = useInjection<ProfileApi>('ProfileApi');

  /** Сервис для работы с API аутентификации */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Получение данных авторизованного пользователя */
  const { authorizedUser } = useAuthentication();

  /** Хук для показа уведомлений */
  const { setNotification } = useNotify();

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /** Обработчик входа в аккаунт */
  const signIn = useSafeCallback(() => authApi.signIn(), [authApi]);

  /**
   * Показывает предупреждение о необходимости авторизации для работы со списком «Смотреть позже»
   */
  const renderAuthWarning = useCallback(() => {
    const content = (
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Typography variant="body2">
          Добавлять фильмы в «Смотреть позже» могут только авторизованные пользователи.
        </Typography>
        <Link component="button" variant="body2" onClick={() => signIn()}>
          Войти
        </Link>
      </Box>
    );
    setNotification({
      message: content,
      severity: 'warning',
    });
  }, [signIn, setNotification]);

  /** Переключает статус фильма в watchlist пользователя */
  const toggleWatchlist = useSafeCallback(async () => {
    if (film == null) return;

    if (!authorizedUser) {
      renderAuthWarning();
      return;
    }

    /**
     * Инвертирует флаг наличия фильма в watchlist
     * @returns {void}
     */
    const toggle = (): void =>
      editFilm((prev) => (prev ? { ...prev, inWatchlist: !prev.inWatchlist } : prev));

    toggle();

    try {
      await profileApi.toggleWatchlist(film.id);
    } catch (e) {
      toggle();
      throw e;
    }
  }, [authorizedUser, profileApi, film, editFilm, renderAuthWarning]);

  if (!film && isError && reload)
    return <LoadError text="Не удалось загрузить фильм" onRetry={reload} />;
  if (!film) return <FilmInfoSkeleton />;

  return (
    <FilmInfo
      film={film}
      onCountrySelect={(value) => navigate(routes.search({ country: value }))}
      onGenreSelect={(value) => navigate(routes.search({ genre: value }))}
      onPersonSelect={(value) => navigate(routes.search({ person: value }))}
      onYearSelect={(value) => navigate(routes.search({ year: Number(value) }))}
      onTypeSelect={(value) => navigate(routes.search({ serial: value === 'Сериал' }))}
      isWatchlistEnabled={!!authorizedUser}
      inWatchlist={film.inWatchlist ?? false}
      onWatchlistToggle={toggleWatchlist}
      {...props}
    />
  );
};

export default FilmModule;

import { Box, Link, SxProps, Theme } from '@mui/material';
import Typography from '@mui/material/Typography';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';

import FilmRatingSkeleton from '../../../components/film/film-rating/FilmRating.skeleton.tsx';
import FilmRating from '../../../components/film/film-rating/FilmRating.tsx';
import { useAuthentication } from '../../../contexts/authentication-context/useAuthentication.tsx';
import { useFilm } from '../../../contexts/film-context/useFilm.tsx';
import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { AuthApi } from '../../../services/auth/auth.api.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';

/**
 * Компонент для управления рейтингом фильма.
 * Отображает текущий рейтинг, позволяет авторизованному пользователю выставлять оценку.
 * @param props - Пропсы компонента
 * @param props.sx - дополнительные стили
 * @returns {ReactElement} JSX-элемент модуля рейтинга
 */
const FilmRatingModule = ({ sx }: { sx?: SxProps<Theme> }): ReactElement => {
  /** Данные текущего авторизованного пользователя */
  const { authorizedUser } = useAuthentication();

  /** Данные фильма и функция их обновления */
  const { film, editFilm } = useFilm();

  /** Хук для показа уведомлений */
  const { setNotification } = useNotify();

  /** Сервис для работы с API фильмов */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Сервис для работы с API аутентификации */
  const authApi = useInjection<AuthApi>('AuthApi');

  /**
   * Показывает предупреждение о необходимости авторизации для выставления рейтинга
   */
  const renderAuthWarning = useCallback(() => {
    const content = (
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
        <Typography variant="body2">
          Рейтинг могут выставлять только авторизованные пользователи.
        </Typography>
        <Link component="button" variant="body2" onClick={() => authApi.signIn()}>
          Войти
        </Link>
      </Box>
    );
    setNotification({
      message: content,
      severity: 'warning',
    });
  }, [authApi, setNotification]);

  /**
   * Обработчик изменения оценки фильма пользователем
   * @param value - новая оценка фильма
   */
  const scoreChanged = useSafeCallback(
    async (value: number) => {
      if (!authorizedUser) {
        renderAuthWarning();
        return;
      }

      if (!film) return;

      const ratingCount = film.userScore ? film.userRatingsCount : film.userRatingsCount + 1;

      const scoreSum =
        (film.userRating ?? 0) * film.userRatingsCount - (film.userScore ?? 0) + value;

      editFilm((prev) => ({
        ...prev!,
        userRating: scoreSum / ratingCount,
        userRatingsCount: ratingCount,
        userScore: value,
      }));

      await filmsApi.rateFilm(film.id, value);
    },
    [authorizedUser, editFilm, film, filmsApi, renderAuthWarning]
  );

  if (!film) return <FilmRatingSkeleton />;

  return (
    <FilmRating
      sx={sx}
      rating={film}
      isSerial={film.isSerial}
      userName={authorizedUser?.userName}
      onScoreChanged={scoreChanged}
    />
  );
};

export default FilmRatingModule;

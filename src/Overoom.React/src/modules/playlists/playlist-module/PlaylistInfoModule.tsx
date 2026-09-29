import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmInfoSkeleton from '../../../components/film/film-info/FilmInfo.skeleton.tsx';
import PlaylistInfo from '../../../components/playlists/playlist-info/PlaylistInfo.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { routes } from '../../../routes.ts';
import { PlaylistsApi } from '../../../services/playlists/playlists.api.ts';
import { PlaylistResponse } from '../../../services/playlists/responses/playlist.response.ts';
import LoadError from '../../../ui/load-error/LoadError.tsx';

/**
 * Модуль информации о плейлисте.
 * @param props - Свойства компонента
 * @param props.id - Уникальный идентификатор плейлиста
 * @returns {ReactElement} JSX элемент модуля информации о плейлисте
 */
const PlaylistInfoModule = ({ id }: { id: string }): ReactElement => {
  /** Состояние плейлиста с детальной информацией */
  const [playlist, setPlaylist] = useState<PlaylistResponse>();

  /** Флаг ошибки загрузки */
  const [isError, setIsError] = useState(false);

  /** Счетчик для повторной загрузки */
  const [reloadKey, setReloadKey] = useState(0);

  /** API сервис для работы с плейлистами */
  const playlistsApi = useInjection<PlaylistsApi>('PlaylistsApi');

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Загружает информацию о плейлисте по ID. Игнорирует устаревшие ответы
   * @param isActual - функция, возвращающая false, если ответ устарел (сменился id)
   * @returns {Promise<void>} Promise, разрешающийся после загрузки данных
   */
  const fetchPlaylist = useSafeCallback(
    async (isActual: () => boolean) => {
      setIsError(false);
      try {
        const response = await playlistsApi.get(id);
        if (isActual()) setPlaylist(response);
      } catch (e) {
        if (isActual()) setIsError(true);
        throw e;
      }
    },
    [playlistsApi, id]
  );

  /** Эффект для загрузки данных плейлиста при монтировании компонента и смене id */
  useEffect(() => {
    let actual = true;
    fetchPlaylist(() => actual).then();

    return (): void => {
      actual = false;
      setPlaylist(undefined);
    };
  }, [fetchPlaylist, reloadKey]);

  /**
   * Обработчик выбора жанра в плейлисте.
   * Выполняет переход на страницу поиска с выбранным жанром
   * @param value - Название выбранного жанра
   * @returns {void}
   */
  const onGenreSelect = useCallback(
    (value: string) => navigate(routes.search({ genre: value })),
    [navigate]
  );

  if (!playlist && isError)
    return (
      <LoadError
        text="Не удалось загрузить подборку"
        onRetry={() => setReloadKey((key) => key + 1)}
      />
    );
  if (!playlist) return <FilmInfoSkeleton />;

  return (
    <PlaylistInfo
      {...playlist}
      updated={new Date(playlist.updated)}
      onGenreSelect={onGenreSelect}
    />
  );
};

export default PlaylistInfoModule;

import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmInfoSkeleton from '../../../components/film/film-info/FilmInfo.skeleton.tsx';
import PlaylistInfo from '../../../components/playlists/playlist-info/PlaylistInfo.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { PlaylistsApi } from '../../../services/playlists/playlists.api.ts';
import { PlaylistResponse } from '../../../services/playlists/responses/playlist.response.ts';

/**
 * Модуль информации о плейлисте.
 * @param props - Свойства компонента
 * @param props.id - Уникальный идентификатор плейлиста
 * @returns {ReactElement} JSX элемент модуля информации о плейлисте
 */
const PlaylistInfoModule = ({ id }: { id: string }): ReactElement => {
  /** Состояние плейлиста с детальной информацией */
  const [playlist, setPlaylist] = useState<PlaylistResponse>();

  /** API сервис для работы с плейлистами */
  const playlistsApi = useInjection<PlaylistsApi>('PlaylistsApi');

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Загружает информацию о плейлисте по ID
   * @returns {Promise<void>} Promise, разрешающийся после загрузки данных
   */
  const fetchPlaylist = useSafeCallback(async () => {
    const response = await playlistsApi.get(id);
    setPlaylist(response);
  }, [playlistsApi, id]);

  /** Эффект для загрузки данных плейлиста при монтировании компонента. */
  useEffect(() => {
    fetchPlaylist().then();

    // Функция очистки эффекта
    return (): void => {
      setPlaylist(undefined);
    };
  }, [fetchPlaylist]);

  /**
   * Обработчик выбора жанра в плейлисте.
   * Выполняет переход на страницу поиска с выбранным жанром
   * @param value - Название выбранного жанра
   * @returns {void}
   */
  const onGenreSelect = useCallback(
    (value: string) => navigate('/search', { state: { genre: value } }),
    [navigate]
  );

  // Отображает скелетон пока данные плейлиста не получены
  if (!playlist) return <FilmInfoSkeleton />;

  // Возвращает компонент информации о плейлисте с загруженными данными
  return (
    <PlaylistInfo
      {...playlist}
      updated={new Date(playlist.updated)}
      onGenreSelect={onGenreSelect}
    />
  );
};

export default PlaylistInfoModule;

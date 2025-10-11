import { ReactElement } from 'react';
import { useLocation } from 'react-router-dom';

import FilmsModule from '../modules/films/films-module/FilmsModule.tsx';
import PlaylistInfoModule from '../modules/playlists/playlist-module/PlaylistInfoModule.tsx';

/**
 * Страница плейлиста.
 * Отображает информацию о плейлисте и список фильмов, входящих в него.
 * @returns {ReactElement} JSX-элемент страницы плейлиста
 */
const PlaylistPage = (): ReactElement => {
  /** Используем хук useLocation для получения состояния навигации (в т.ч. ID плейлиста) */
  const { state } = useLocation();

  // Рендерим модуль информации о плейлисте и связанные с ним фильмы
  return (
    <>
      <PlaylistInfoModule id={state?.id} />
      <FilmsModule playlistId={state?.id} />
    </>
  );
};

export default PlaylistPage;

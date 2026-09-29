import { ReactElement } from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import NotFound from '../components/pages/NotFound.tsx';
import FilmsModule from '../modules/films/films-module/FilmsModule.tsx';
import PlaylistInfoModule from '../modules/playlists/playlist-module/PlaylistInfoModule.tsx';
import { routes } from '../routes.ts';

/**
 * Страница плейлиста.
 * Отображает информацию о плейлисте и список фильмов, входящих в него.
 * @returns {ReactElement} JSX-элемент страницы плейлиста
 */
const PlaylistPage = (): ReactElement => {
  /** Идентификатор плейлиста из адреса страницы (/playlist/:id) */
  const { id } = useParams();

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  if (!id) return <NotFound action={() => navigate(routes.home)} />;

  return (
    <>
      <PlaylistInfoModule id={id} />
      <FilmsModule playlistId={id} />
    </>
  );
};

export default PlaylistPage;

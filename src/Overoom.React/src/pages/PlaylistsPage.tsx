import { ReactElement, useState } from 'react';

import GenreSelectModule from '../modules/films/genres-module/GenreSelectModule.tsx';
import PlaylistsModule from '../modules/playlists/playlists-module/PlaylistsModule.tsx';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/**
 * Страница подборок с фильмами
 * @returns {ReactElement} JSX элемент страницы подборок
 */
const PlaylistsPage = (): ReactElement => {
  /** Хук состояния для выбранного жанра */
  const [genre, genreSelect] = useState<string>();

  return (
    <>
      <BlockTitle title="Жанры" />

      <GenreSelectModule genre={genre} onSelect={(g) => genreSelect(g)} />

      <PlaylistsModule genre={genre} />
    </>
  );
};

export default PlaylistsPage;

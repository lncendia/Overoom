import { ReactElement, useState } from 'react';

import FilmsModule from '../modules/films/films-module/FilmsModule.tsx';
import GenreSelectModule from '../modules/films/genres-module/GenreSelectModule.tsx';
import PopularFilmsModule from '../modules/films/popular-films-module/PopularFilmsModule.tsx';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/**
 * Страница каталога фильмов.
 * @returns {ReactElement} JSX элемент страницы каталога
 */
const CatalogPage = (): ReactElement => {
  /** Состояние выбранного жанра */
  const [genre, setGenre] = useState<string | undefined>();

  return (
    <>
      <BlockTitle title="Популярное сейчас" />
      <PopularFilmsModule />

      <BlockTitle title="Жанры" sx={{ mt: 3 }} />
      <GenreSelectModule genre={genre} onSelect={setGenre} />

      <FilmsModule genre={genre} />
    </>
  );
};

export default CatalogPage;

import { ReactElement } from 'react';
import { useSearchParams } from 'react-router-dom';

import FilmsModule from '../modules/films/films-module/FilmsModule.tsx';
import { parseSearchFilters, SearchFilters } from '../routes.ts';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/**
 * Определяет заголовок блока в зависимости от фильтров поиска.
 * @param filters - Фильтры поиска из query-параметров
 * @returns {string} Заголовок, соответствующий выбранному фильтру
 */
const getTitle = (filters: SearchFilters): string => {
  if (filters.genre) return `Жанр: ${filters.genre}`;
  if (filters.person) return `Принял участие: ${filters.person}`;
  if (filters.country) return `Страна: ${filters.country}`;
  if (filters.serial !== undefined) return filters.serial ? 'Сериалы' : 'Фильмы';
  if (filters.year) return `Всё за ${filters.year} год`;
  return 'Поиск';
};

/**
 * Страница поиска фильмов.
 * Отображает список фильмов в зависимости от фильтров, переданных через query-параметры.
 * @returns {ReactElement} JSX-элемент страницы поиска фильмов
 */
const FilmSearchPage = (): ReactElement => {
  /** Используем хук useSearchParams для получения фильтров из адреса */
  const [searchParams] = useSearchParams();

  /** Фильтры поиска */
  const filters = parseSearchFilters(searchParams);

  return (
    <>
      <BlockTitle title={getTitle(filters)} />
      <FilmsModule
        year={filters.year}
        genre={filters.genre}
        person={filters.person}
        country={filters.country}
        serial={filters.serial}
      />
    </>
  );
};

export default FilmSearchPage;

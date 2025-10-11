import { ReactElement } from 'react';
import { useLocation } from 'react-router-dom';

import FilmsModule from '../modules/films/films-module/FilmsModule.tsx';
import BlockTitle from '../ui/block-title/BlockTitle.tsx';

/**
 * Определяет заголовок блока в зависимости от состояния навигации.
 * @param state - Объект состояния, переданный при навигации
 * @returns {string} Заголовок, соответствующий выбранному фильтру
 */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
const getTitle = (state: any): string => {
  if (state.genre) return `Жанр: ${state.genre}`;
  if (state.person) return `Принял участие: ${state.person}`;
  if (state.country) return `Страна: ${state.country}`;
  if (state.serial) return state.serial ? 'Сериалы' : 'Фильмы';
  if (state.year) return `Всё за ${state.year} год`;
  return '';
};

/**
 * Страница поиска фильмов.
 * Отображает список фильмов в зависимости от параметров, переданных через состояние навигации.
 * @returns {ReactElement} JSX-элемент страницы поиска фильмов
 */
const FilmSearchPage = (): ReactElement => {
  /** Используем хук useLocation для получения состояния навигации */
  const { state } = useLocation();

  // Рендерим заголовок и список фильмов в зависимости от выбранных параметров
  return (
    <>
      <BlockTitle title={getTitle(state)} />
      <FilmsModule
        year={state?.year}
        genre={state?.genre}
        person={state?.person}
        country={state?.country}
        serial={state?.serial}
      />
    </>
  );
};

export default FilmSearchPage;

import { ReactElement } from 'react';

import FilmsList from '../../../components/films/films-list/FilmsList.tsx';
import { routes } from '../../../routes.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Модуль для отображения списка фильмов пользователя.
 * Отображает фильмы ссылками на их страницы и состояние пустого списка
 * @param props - Свойства компонента
 * @param props.films - Массив фильмов пользователя для отображения
 * @returns {ReactElement} JSX элемент модуля фильмов пользователя
 */
const UserFilmsModule = ({ films }: { films: FilmShortResponse[] }): ReactElement => {
  if (films.length === 0) return <NoData text="Пусто" />;

  return <FilmsList films={films} getLink={routes.film} />;
};

export default UserFilmsModule;

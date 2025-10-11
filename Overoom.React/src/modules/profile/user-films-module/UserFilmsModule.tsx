import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmsList from '../../../components/films/films-list/FilmsList.tsx';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';
import NoData from '../../../ui/no-data/NoData.tsx';

/**
 * Модуль для отображения списка фильмов пользователя.
 * Обрабатывает навигацию к детальной странице фильма и отображает состояние пустого списка
 * @param props - Свойства компонента
 * @param props.films - Массив фильмов пользователя для отображения
 * @returns {ReactElement} JSX элемент модуля фильмов пользователя
 */
const UserFilmsModule = ({ films }: { films: FilmShortResponse[] }): ReactElement => {
  /** Хук для навигации между страницами приложения */
  const navigate = useNavigate();

  /**
   * Обработчик выбора фильма из списка
   * @param id - ID выбранного фильма
   * @returns {void}
   */
  const onSelect = useCallback(
    (id: string) => {
      navigate('/film', { state: { id: id } });
    },
    [navigate]
  );

  // Отображает компонент пустого состояния если список фильмов пуст
  if (films.length === 0) return <NoData text="Пусто" />;

  // Возвращает список фильмов с обработчиком выбора
  return <FilmsList films={films} onSelect={onSelect} />;
};

export default UserFilmsModule;

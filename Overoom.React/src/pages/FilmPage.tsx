import { ReactElement, useState } from 'react';
import { useLocation } from 'react-router-dom';

import { FilmContextProvider } from '../contexts/film-context/FilmContextProvider.tsx';
import { useFilm } from '../contexts/film-context/useFilm.tsx';
import CommentsModule from '../modules/films/comments-module/CommentsModule.tsx';
import FilmModule from '../modules/films/film-module/FilmModule.tsx';
import FilmPlayerModule from '../modules/films/film-player-module/FilmPlayerModule.tsx';
import FilmRatingModule from '../modules/films/film-rating-module/FilmRatingModule.tsx';
import CreateRoomModule from '../modules/rooms/create-room-module/CreateRoomModule.tsx';

/**
 * Компонент страницы фильма — корневой элемент, предоставляющий контекст фильма.
 * Получает идентификатор фильма из состояния навигации и оборачивает дочерний компонент.
 * @returns {ReactElement} JSX-элемент страницы фильма с провайдером контекста
 */
const FilmPage = (): ReactElement => {
  /** Используем хук useLocation для получения состояния навигации (в т.ч. ID фильма) */
  const { state } = useLocation();

  // Оборачиваем внутреннюю страницу в контекст фильма
  return (
    <FilmContextProvider filmId={state.id}>
      <FilmPageInternal />
    </FilmContextProvider>
  );
};

/**
 * Внутренний компонент страницы фильма.
 * Содержит модули отображения фильма, рейтинга, плеера, комментариев и создания комнаты.
 * @returns {ReactElement} JSX-элемент с основным содержимым страницы фильма
 */
const FilmPageInternal = (): ReactElement => {
  /** Состояние, контролирующее отображение формы создания комнаты */
  const [formOpen, setFormOpen] = useState(false);

  /** Получаем данные о фильме из контекста с помощью хука useFilm */
  const { film } = useFilm();

  // Рендерим основную структуру страницы фильма
  return (
    <>
      <FilmModule
        buttonText={film?.canCreateRoom ? 'Создать комнату' : undefined}
        onButtonClicked={() => setFormOpen(true)}
      />
      <FilmPlayerModule />
      <FilmRatingModule />
      <CreateRoomModule open={formOpen} onClose={() => setFormOpen(false)} />
      <CommentsModule />
    </>
  );
};

export default FilmPage;

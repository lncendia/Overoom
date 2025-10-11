import { useInjection } from 'inversify-react';
import { ReactElement, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';

import FilmsSliderSkeleton from '../../../components/films/films-slider/FilmsSlider.skeleton.tsx';
import FilmsSlider from '../../../components/films/films-slider/FilmsSlider.tsx';
import { usePaginatedFetch } from '../../../hooks/paginated-fetch-hook/usePaginatedFetch.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';

/**
 * Модуль для отображения популярных фильмов в виде слайдера.
 * Позволяет пользователю просматривать популярные фильмы и выбирать конкретный фильм.
 * @returns {ReactElement} JSX-элемент слайдера популярных фильмов
 */
const PopularFilmsModule = (): ReactElement => {
  /** Сервис для работы с API фильмов */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /**
   * Функция для загрузки популярных фильмов с поддержкой пагинации
   * @param _skip - количество пропускаемых фильмов (не используется, оставлено для совместимости)
   * @param take - количество загружаемых фильмов
   * @returns Promise с объектом, содержащим список фильмов и общее количество
   */
  const fetch = useCallback(
    async (_skip: number, take: number) => {
      const films = await filmsApi.getPopular(take);
      return {
        list: films,
        totalCount: films.length,
      };
    },
    [filmsApi]
  );

  /** Хук для порционно-загружаемых фильмов */
  const { items: films, isLoading } = usePaginatedFetch<FilmShortResponse>(fetch, 20);

  /**
   * Обработчик выбора фильма
   * @param id - ID выбранного фильма
   */
  const onSelect = useCallback(
    (id: string) => {
      navigate('/film', { state: { id: id } });
    },
    [navigate]
  );

  // Показываем скелетон, пока данные фильмов загружаются
  if (isLoading) return <FilmsSliderSkeleton />;

  // Основной рендер: компонент слайдера фильмов
  return <FilmsSlider films={films} onSelect={onSelect} />;
};

export default PopularFilmsModule;

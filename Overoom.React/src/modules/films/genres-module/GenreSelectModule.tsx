import 'react-multi-carousel/lib/styles.css';
import { ReactElement } from 'react';

import GenreSlider from '../../../components/genres/genres-slider/GenreSlider.tsx';

/** Список доступных жанров для выбора */
const genres = [
  'Аниме',
  'Боевик',
  'Военный',
  'Детектив',
  'Драма',
  'Комедия',
  'Мелодрама',
  'Мультфильм',
  'Приключения',
  'Триллер',
  'Ужасы',
  'Фантастика',
  'Фэнтези',
];

/**
 * Пропсы компонента GenreSelectModule
 */
interface GenreSelectModuleProps {
  /** Текущий выбранный жанр */
  genre: string | undefined;
  /** Callback, вызываемый при выборе жанра */
  onSelect: (genre?: string) => void;
}

/**
 * Модуль выбора жанра с горизонтальным слайдером
 * @param props - Пропсы компонента
 * @param props.genre - Текущий выбранный жанр
 * @param props.onSelect - Callback, вызываемый при выборе жанра
 * @returns {ReactElement} JSX-элемент, отображающий слайдер жанров
 */
const GenreSelectModule = ({ genre, onSelect }: GenreSelectModuleProps): ReactElement => {
  // Рендерим компонент GenreSlider с переданными жанрами и выбранным значением
  return <GenreSlider genre={genre} onSelect={onSelect} genres={genres} />;
};

export default GenreSelectModule;

import { styled, Tooltip } from '@mui/material';
import { ReactElement } from 'react';

/** Стилизованный SVG с курсором-указателем */
const PointerSvg = styled('svg')({
  cursor: 'pointer',
});

/**
 * Функция рендеринга SVG иконки для кнопки watchlist
 * @param inWatchlist - флаг, показывающий, находится ли фильм в watchlist
 * @param onWatchlistToggle - функция переключения состояния watchlist
 * @returns {ReactElement} JSX элемент SVG иконки
 */
const renderSvg = (inWatchlist: boolean, onWatchlistToggle: () => void): ReactElement => {
  if (inWatchlist)
    return (
      <PointerSvg
        onClick={onWatchlistToggle}
        xmlns="http://www.w3.org/2000/svg"
        width="16"
        height="16"
        fill="currentColor"
        viewBox="0 0 16 16"
      >
        <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z" />
      </PointerSvg>
    );

  return (
    <PointerSvg
      onClick={onWatchlistToggle}
      xmlns="http://www.w3.org/2000/svg"
      width="16"
      height="16"
      fill="currentColor"
      viewBox="0 0 16 16"
    >
      <path d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z" />
      <path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0z" />
    </PointerSvg>
  );
};

/** Интерфейс пропсов компонента FilmWatchlist */
interface FilmWatchlistProps {
  /** Флаг наличия фильма в watchlist */
  inWatchlist: boolean;
  /** Функция переключения состояния watchlist */
  onWatchlistToggle: () => void;
}

/**
 * Компонент кнопки добавления или удаления фильма из watchlist.
 * Показывает тултип "Смотреть позже" и иконку в зависимости от состояния.
 * @param props - Свойства компонента
 * @param props.inWatchlist - флаг наличия фильма в watchlist
 * @param props.onWatchlistToggle - функция переключения состояния watchlist
 * @returns {ReactElement} JSX элемент кнопки watchlist с иконкой
 */
const FilmWatchlist = ({ inWatchlist, onWatchlistToggle }: FilmWatchlistProps): ReactElement => {
  return <Tooltip title="Смотреть позже">{renderSvg(inWatchlist, onWatchlistToggle)}</Tooltip>;
};

export default FilmWatchlist;

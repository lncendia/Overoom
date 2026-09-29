import { IconButton, Tooltip } from '@mui/material';
import { ReactElement } from 'react';

/**
 * Функция рендеринга SVG иконки для кнопки watchlist
 * @param inWatchlist - флаг, показывающий, находится ли фильм в watchlist
 * @returns {ReactElement} JSX элемент SVG иконки
 */
const renderSvg = (inWatchlist: boolean): ReactElement => {
  if (inWatchlist)
    return (
      <svg
        xmlns="http://www.w3.org/2000/svg"
        width="16"
        height="16"
        fill="currentColor"
        viewBox="0 0 16 16"
        aria-hidden="true"
      >
        <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z" />
      </svg>
    );

  return (
    <svg
      xmlns="http://www.w3.org/2000/svg"
      width="16"
      height="16"
      fill="currentColor"
      viewBox="0 0 16 16"
      aria-hidden="true"
    >
      <path d="M8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z" />
      <path d="M8 16A8 8 0 1 0 8 0a8 8 0 0 0 0 16zm7-8A7 7 0 1 1 1 8a7 7 0 0 1 14 0z" />
    </svg>
  );
};

/**
 * Возвращает текст подсказки для кнопки watchlist
 * @param inWatchlist - флаг наличия фильма в watchlist
 * @param enabled - флаг доступности watchlist (пользователь авторизован)
 * @returns {string} Текст подсказки
 */
const getTooltip = (inWatchlist: boolean, enabled: boolean): string => {
  if (!enabled) return 'Войдите, чтобы добавить в «Смотреть позже»';
  return inWatchlist ? 'Убрать из «Смотреть позже»' : 'Добавить в «Смотреть позже»';
};

/** Интерфейс пропсов компонента FilmWatchlist */
interface FilmWatchlistProps {
  /** Флаг наличия фильма в watchlist */
  inWatchlist: boolean;
  /** Флаг доступности watchlist (пользователь авторизован) */
  enabled: boolean;
  /** Функция переключения состояния watchlist */
  onWatchlistToggle: () => void;
}

/**
 * Компонент кнопки добавления или удаления фильма из watchlist.
 * Показывает тултип и иконку в зависимости от состояния.
 * @param props - Свойства компонента
 * @param props.inWatchlist - флаг наличия фильма в watchlist
 * @param props.enabled - флаг доступности watchlist
 * @param props.onWatchlistToggle - функция переключения состояния watchlist
 * @returns {ReactElement} JSX элемент кнопки watchlist с иконкой
 */
const FilmWatchlist = ({
  inWatchlist,
  enabled,
  onWatchlistToggle,
}: FilmWatchlistProps): ReactElement => {
  const title = getTooltip(inWatchlist, enabled);

  return (
    <Tooltip title={title}>
      <IconButton
        size="small"
        color="inherit"
        aria-label={title}
        aria-pressed={enabled ? inWatchlist : undefined}
        onClick={onWatchlistToggle}
      >
        {renderSvg(inWatchlist)}
      </IconButton>
    </Tooltip>
  );
};

export default FilmWatchlist;

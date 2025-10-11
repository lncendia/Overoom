import SearchIcon from '@mui/icons-material/Search';
import { alpha, Paper, Popper, styled } from '@mui/material';
import InputBase from '@mui/material/InputBase';
import React, { useState, useRef, ReactElement } from 'react';

import FilmSearchElement from './FilmSearchElement.tsx';
import withDelayedInput from '../../../hocs/withDelayedInput.tsx';
import useDelayedAction from '../../../hooks/delayed-action-hook/useDelayedAction.ts';
import { FilmShortResponse } from '../../../services/films/responses/film-short.response.ts';

/** Пропсы компонента поиска фильмов */
interface FilmSearchProps {
  /** Список найденных фильмов */
  films: FilmShortResponse[];
  /** Коллбэк для обработки поиска фильмов */
  onFilmSearch: (value: string) => void;
  /** Коллбэк для выбора фильма */
  onClick: (id: string) => void;
}

/** Стилизованный контейнер поиска */
const Search = styled('div')(({ theme }) => ({
  position: 'relative',
  borderRadius: theme.shape.borderRadius,
  backgroundColor: alpha(theme.palette.common.white, 0.15),
  '&:hover': {
    backgroundColor: alpha(theme.palette.common.white, 0.25),
  },
  marginLeft: 0,
  width: '100%',
  [theme.breakpoints.up('md')]: {
    marginLeft: theme.spacing(1),
    width: 'auto',
  },
}));

/** Стилизованная обертка для иконки поиска */
const SearchIconWrapper = styled('div')(({ theme }) => ({
  padding: theme.spacing(0, 2),
  height: '100%',
  position: 'absolute',
  pointerEvents: 'none',
  display: 'flex',
  alignItems: 'center',
  justifyContent: 'center',
}));

/** Компонент ввода с задержкой */
const Input = withDelayedInput(InputBase);

/** Стилизованное поле ввода */
const StyledInputBase = styled(Input)(({ theme }) => ({
  color: 'inherit',
  width: '100%',
  '& .MuiInputBase-input': {
    padding: theme.spacing(1, 1, 1, 0),
    paddingLeft: `calc(1em + ${theme.spacing(4)})`,
    transition: theme.transitions.create('width'),
    [theme.breakpoints.up('sm')]: {
      width: '20ch',
      '&:focus': {
        width: '30ch',
      },
    },
  },
}));

/**
 * Компонент поиска фильмов с автодополнением
 * @param props - объект с пропсами FilmSearchProps
 * @param props.films - список найденных фильмов
 * @param props.onFilmSearch - функция обработки ввода поиска
 * @param props.onClick - функция выбора фильма
 * @returns {ReactElement} JSX элемент поиска
 */
const FilmSearch = ({ onFilmSearch, films, onClick }: FilmSearchProps): ReactElement => {
  /** Состояние открытия выпадающего списка */
  const [open, setOpen] = useState(false);

  /** Референс для позиционирования Popper */
  const anchorRef = useRef<HTMLDivElement>(null);

  /**
   * Обработчик ввода текста в поле поиска
   * @param e - событие изменения input
   */
  const onInput = (e: React.ChangeEvent<HTMLInputElement>) => {
    onFilmSearch(e.target.value);
    setOpen(true);
  };

  /** Обработчик закрытия Popper с задержкой */
  const handleBlur = useDelayedAction(() => setOpen(false), 1000);

  return (
    <>
      {/* Контейнер поиска с иконкой и полем ввода */}
      <Search ref={anchorRef}>
        <SearchIconWrapper>
          <SearchIcon />
        </SearchIconWrapper>
        <StyledInputBase
          onChange={onInput}
          onBlur={handleBlur}
          placeholder="Поиск…"
          inputProps={{ 'aria-label': 'search' }}
        />
      </Search>

      {/* Выпадающий список найденных фильмов */}
      <Popper
        open={open && films.length > 0}
        anchorEl={anchorRef.current}
        placement="bottom-start"
        sx={{
          width: anchorRef.current?.clientWidth,
          zIndex: 1300,
          mt: 1,
        }}
      >
        <Paper
          elevation={4}
          sx={{
            p: 1,
            width: '100%',
            maxHeight: 400,
            overflow: 'auto',
            backgroundColor: (theme) => alpha(theme.palette.background.paper, 1),
          }}
        >
          {films.map((film) => (
            <FilmSearchElement key={film.id} film={film} onClick={() => onClick(film.id)} />
          ))}
        </Paper>
      </Popper>
    </>
  );
};

export default FilmSearch;

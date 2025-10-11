import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import {
  Box,
  Button,
  Collapse,
  Divider,
  IconButton,
  Paper,
  Stack,
  useMediaQuery,
  useTheme,
} from '@mui/material';
import Typography from '@mui/material/Typography';
import { ReactElement, useMemo, useState } from 'react';

import { FilmInfoDto } from './film-info.dto.ts';
import FilmWatchlist from './FilmWatchlist.tsx';
import FilmInfoCard from '../../../ui/film-info-card/FilmInfoCard.tsx';
import KeyList from '../../../ui/key-list/KeyList.tsx';

/** Пропсы компонента информации о фильме */
export interface FilmInfoProps {
  /** Данные фильма */
  film: FilmInfoDto;
  /** Флаг доступности watchlist */
  isWatchlistEnabled: boolean;
  /** Обработчик выбора страны */
  onCountrySelect: (value: string) => void;
  /** Обработчик выбора жанра */
  onGenreSelect: (value: string) => void;
  /** Обработчик выбора персоны */
  onPersonSelect: (value: string) => void;
  /** Обработчик выбора типа */
  onTypeSelect: (value: string) => void;
  /** Обработчик выбора года */
  onYearSelect: (value: string) => void;
  /** Обработчик создания комнаты */
  onButtonClicked: (() => void) | undefined;
  /** Текст кнопки */
  buttonText: string | undefined;
  /** Флаг наличия в watchlist */
  inWatchlist: boolean;
  /** Обработчик переключения watchlist */
  onWatchlistToggle: () => void;
}
/**
 * Компонент отображения информации о фильме
 * @param props - Пропсы компонента
 * @returns {ReactElement} JSX элемент информации о фильме
 */
const FilmInfo = (props: FilmInfoProps): ReactElement => {
  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();

  /** Используем хук useMediaQuery для определения мобильного разрешения */
  const isMobile = useMediaQuery(theme.breakpoints.down('lg'));

  /** Используем хук useState для управления состоянием открытия выпадающего списка */
  const [open, setOpen] = useState(false);

  /** Используем хук useMemo для мемоизации состояния открытия collapse */
  const collapseOpen = useMemo(() => open || !isMobile, [isMobile, open]);

  return (
    <Paper>
      <FilmInfoCard {...props.film}>
        {/* Заголовок и кнопка "Смотреть позже" */}
        <Stack direction="row" justifyContent="space-between" alignItems="center" sx={{ mb: 2 }}>
          <Typography variant="h5">{props.film.title}</Typography>
          <FilmWatchlist
            inWatchlist={props.inWatchlist}
            onWatchlistToggle={props.onWatchlistToggle}
          />
        </Stack>

        <Divider sx={{ my: 1, borderWidth: 1 }} />

        {/* Описание фильма */}
        <Stack direction="row" alignItems="flex-start" spacing={1}>
          <Typography variant="body2" sx={{ flexGrow: 1 }}>
            {props.film.description}
          </Typography>

          {/* Кнопка меню только на мобильных */}
          {isMobile && (
            <IconButton
              color={'secondary'}
              size="small"
              onClick={() => setOpen((v) => !v)}
              sx={{
                flexShrink: 0,
                p: 0.5,
                border: '1px solid',
                borderRadius: 1,
              }}
            >
              {open ? <ExpandLessIcon fontSize="small" /> : <ExpandMoreIcon fontSize="small" />}
            </IconButton>
          )}
        </Stack>

        <Divider sx={{ my: 1, borderWidth: 1 }} />

        {/* Списки информации */}
        <Collapse in={collapseOpen} timeout="auto">
          <Stack spacing={1} sx={{ mb: 3 }}>
            <KeyList
              title="Год:"
              values={[props.film.year.toString()]}
              onKeySelect={props.onYearSelect}
            />
            <KeyList
              title="Тип:"
              values={[props.film.isSerial ? 'Сериал' : 'Фильм']}
              onKeySelect={props.onTypeSelect}
            />
            <KeyList
              title="Страна:"
              values={props.film.countries}
              onKeySelect={props.onCountrySelect}
            />
            <KeyList title="Жанр:" values={props.film.genres} onKeySelect={props.onGenreSelect} />
            <KeyList
              title="Режиссер:"
              values={props.film.directors}
              onKeySelect={props.onPersonSelect}
            />
            <KeyList
              title="Сценарий:"
              values={props.film.screenWriters}
              onKeySelect={props.onPersonSelect}
            />
            <KeyList
              title="Актеры:"
              values={props.film.actors.map<[string, string | null]>((actor) => [
                actor.name,
                actor.role,
              ])}
              onKeySelect={props.onPersonSelect}
            />
          </Stack>
        </Collapse>
        {/* Кнопка */}
        {props.buttonText && props.onButtonClicked && (
          <Box sx={{ display: 'flex', justifyContent: 'center' }}>
            <Button
              variant="contained"
              color="primary"
              size="small"
              onClick={props.onButtonClicked}
              sx={{ width: '100%' }}
            >
              {props.buttonText}
            </Button>
          </Box>
        )}
      </FilmInfoCard>
    </Paper>
  );
};

export default FilmInfo;

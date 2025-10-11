import FullscreenIcon from '@mui/icons-material/Fullscreen';
import FullscreenExitIcon from '@mui/icons-material/FullscreenExit';
import KeyboardIcon from '@mui/icons-material/Keyboard';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import PauseIcon from '@mui/icons-material/Pause';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import {
  Avatar,
  Box,
  Chip,
  IconButton,
  Menu,
  MenuItem,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import React, { ReactElement, useState } from 'react';

import { ViewerTagDto } from './viewer-tag.dto.ts';
import { ViewerDto } from './viewer.dto.ts';

/** Пропсы компонента Viewer */
interface ViewerProps {
  /** Данные зрителя для отображения */
  viewer: ViewerDto;
  /** Обработчик сигнала "бип" */
  onBeep: () => void;
  /** Обработчик сигнала "крик" */
  onScream: () => void;
  /** Обработчик выгона зрителя из комнаты (опционально) */
  onKick?: () => void;
  /** Обработчик синхронизации времени просмотра (опционально) */
  onSync?: () => void;
}

/**
 * Основной компонент отображения зрителя
 * @param props - Свойства компонента
 * @returns {ReactElement} JSX элемент карточки зрителя
 */
const Viewer = (props: ViewerProps): ReactElement => {
  const { viewer } = props;

  return (
    <Stack direction="row" spacing={2} alignItems="top" sx={{ mt: 1 }}>
      {/* Аватар зрителя */}
      <Avatar src={viewer.photoUrl ?? undefined} sx={{ width: 48, height: 48 }} />

      {/* Основная информация о зрителе */}
      <Box flex={1}>
        {viewer.online ? <OnlineViewer {...props} /> : <OfflineViewer {...props} />}
        <TagsList tags={viewer.tags} />
      </Box>
    </Stack>
  );
};

/**
 * Компонент отображения онлайн-зрителя с индикаторами состояния
 * @param props - Свойства компонента
 * @returns {ReactElement} JSX элемент онлайн-зрителя
 */
const OnlineViewer = (props: ViewerProps): ReactElement => {
  const { viewer } = props;

  return (
    <>
      {/* Строка с именем и индикаторами состояния */}
      <Stack direction="row" spacing={1} alignItems="center">
        <Username {...props} />

        {/* Индикатор паузы/воспроизведения */}
        {viewer.onPause ? <PauseIcon fontSize="small" /> : <PlayArrowIcon fontSize="small" />}

        {/* Индикатор полноэкранного режима */}
        {viewer.fullScreen ? (
          <FullscreenIcon fontSize="small" />
        ) : (
          <FullscreenExitIcon fontSize="small" />
        )}

        {/* Индикатор печатания */}
        {viewer.typing && <KeyboardIcon fontSize="small" />}
      </Stack>

      {/* Время просмотра и информация о сезоне/эпизоде */}
      <Typography variant="body2">
        {formatTime(viewer.timeLine)}
        {viewer.season && viewer.episode && (
          <>
            {' '}
            ({viewer.season}x{viewer.episode})
          </>
        )}
      </Typography>
    </>
  );
};

/**
 * Компонент отображения оффлайн-зрителя
 * @param props - Свойства компонента
 * @returns {ReactElement} JSX элемент оффлайн-зрителя
 */
const OfflineViewer = (props: ViewerProps): ReactElement => {
  return (
    <Stack direction="row" spacing={1} alignItems="center">
      <Username {...props} />
      {/* Бейдж оффлайн статуса */}
      <Chip label="offline" size="small" color="default" />
    </Stack>
  );
};

/**
 * Компонент отображения имени пользователя с выпадающим меню действий
 * @param props - Свойства компонента
 * @returns {ReactElement} JSX элемент имени пользователя с меню
 */
const Username = (props: ViewerProps): ReactElement => {
  const { viewer, onBeep, onScream, onKick, onSync } = props;

  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const open = Boolean(anchorEl);

  // Проверяем наличие доступных действий для этого зрителя
  const hasActions = viewer.canBeep || viewer.canScream || viewer.canKick || viewer.canSync;

  /**
   * Обработчик открытия меню действий
   * @param event - Объект события мыши
   */
  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  /** Обработчик закрытия меню действий */
  const handleClose = () => {
    setAnchorEl(null);
  };

  return (
    <Stack direction="row" alignItems="center" spacing={0.5}>
      {/* Имя пользователя с цветом в зависимости от статуса */}
      <Typography variant="subtitle1" color={viewer.online ? 'text.primary' : 'text.disabled'}>
        {viewer.userName}
      </Typography>

      {/* Кнопка меню действий (отображается только если есть доступные действия) */}
      {hasActions && (
        <>
          <IconButton size="small" onClick={handleClick} color="inherit">
            <MoreVertIcon fontSize="small" />
          </IconButton>
          <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
            {/* Действие синхронизации (только для онлайн зрителей) */}
            {viewer.canSync && <MenuItem onClick={onSync}>Синхронизовать</MenuItem>}

            {/* Действие выгона из комнаты */}
            {viewer.canKick && <MenuItem onClick={onKick}>Выгнать</MenuItem>}

            {/* Действие "бип" (только для онлайн зрителей) */}
            {viewer.canBeep && <MenuItem onClick={onBeep}>Разбудить</MenuItem>}

            {/* Действие "крик" (только для онлайн зрителей) */}
            {viewer.canScream && <MenuItem onClick={onScream}>Напугать</MenuItem>}
          </Menu>
        </>
      )}
    </Stack>
  );
};

/**
 * Компонент отображения списка тегов зрителя
 *
 * @param props - Свойства компонента
 * @param props.tags - Массив тегов зрителя
 * @returns {ReactElement | null} JSX элемент списка тегов или null если тегов нет
 */
const TagsList = ({ tags }: { tags: ViewerTagDto[] }): ReactElement | null => {
  if (!tags || tags.length === 0) return null;

  return (
    <Stack direction="row" gap={1} flexWrap="wrap" mt={0.5}>
      {tags.map((tag, i) => (
        <Tooltip key={i} title={<>{tag.description && <div>{tag.description}</div>}</>}>
          <Chip label={tag.name} size="small" color="primary" />
        </Tooltip>
      ))}
    </Stack>
  );
};

export default Viewer;

/**
 * Форматирует время из наносекунд в читаемый формат HH:MM:SS
 *
 * @param nano - Время в наносекундах
 * @returns {string} Отформатированное время в формате HH:MM:SS
 */
function formatTime(nano: number): string {
  const totalSeconds = Math.floor(nano / 10_000_000);
  const h = Math.floor(totalSeconds / 3600)
    .toString()
    .padStart(2, '0');
  const m = Math.floor((totalSeconds % 3600) / 60)
    .toString()
    .padStart(2, '0');
  const s = (totalSeconds % 60).toString().padStart(2, '0');
  return `${h}:${m}:${s}`;
}

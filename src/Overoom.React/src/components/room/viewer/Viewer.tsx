import FullscreenIcon from '@mui/icons-material/Fullscreen';
import FullscreenExitIcon from '@mui/icons-material/FullscreenExit';
import KeyboardIcon from '@mui/icons-material/Keyboard';
import MoreVertIcon from '@mui/icons-material/MoreVert';
import PauseIcon from '@mui/icons-material/Pause';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import {
  Avatar,
  Box,
  Button,
  Chip,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  IconButton,
  Menu,
  MenuItem,
  Stack,
  Tooltip,
  Typography,
} from '@mui/material';
import React, { ReactElement, useEffect, useState } from 'react';

import { ViewerTagDto } from './viewer-tag.dto.ts';
import { ViewerDto } from './viewer.dto.ts';

/** Количество тиков (100-наносекундных интервалов) в миллисекунде */
const TICKS_PER_MILLISECOND = 10_000;

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
    <Stack direction="row" spacing={2} sx={{ mt: 1, alignItems: 'flex-start' }}>
      <Avatar src={viewer.photoUrl ?? undefined} sx={{ width: 48, height: 48 }} />

      <Box sx={{ flex: 1 }}>
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
      <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
        <Username {...props} />

        {viewer.onPause ? <PauseIcon fontSize="small" /> : <PlayArrowIcon fontSize="small" />}

        {viewer.fullScreen ? (
          <FullscreenIcon fontSize="small" />
        ) : (
          <FullscreenExitIcon fontSize="small" />
        )}

        {viewer.typing && <KeyboardIcon fontSize="small" />}
      </Stack>

      <Typography variant="body2">
        <ViewerTimeLine viewer={viewer} />
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
 * Текущее время просмотра зрителя.
 * Пока зритель не на паузе, время обновляется локальным таймером раз в секунду,
 * поэтому каждую секунду перерисовывается только этот компонент
 * @param props - Свойства компонента
 * @param props.viewer - Данные зрителя
 * @returns {ReactElement} Отформатированное время просмотра
 */
const ViewerTimeLine = ({ viewer }: { viewer: ViewerDto }): ReactElement => {
  const { timeLine, timeLineUpdatedAt, speed = 1 } = viewer;

  /** Флаг: время не идет (пауза или неизвестен момент обновления позиции) */
  const stopped = viewer.onPause || timeLineUpdatedAt === undefined;

  /** Текущий момент, обновляемый таймером */
  const [now, setNow] = useState(() => Date.now());

  /** Эффект запуска таймера, пока идет воспроизведение */
  useEffect(() => {
    if (stopped) return;
    const id = setInterval(() => setNow(Date.now()), 1000);
    return () => clearInterval(id);
  }, [stopped]);

  const elapsed = stopped ? 0 : Math.max(0, now - (timeLineUpdatedAt ?? now));
  return <>{formatTime(timeLine + elapsed * TICKS_PER_MILLISECOND * speed)}</>;
};

/**
 * Компонент отображения оффлайн-зрителя
 * @param props - Свойства компонента
 * @returns {ReactElement} JSX элемент оффлайн-зрителя
 */
const OfflineViewer = (props: ViewerProps): ReactElement => {
  return (
    <Stack direction="row" spacing={1} sx={{ alignItems: 'center' }}>
      <Username {...props} />
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
  const hasActions = viewer.canBeep || viewer.canScream || viewer.canKick || viewer.canSync;

  /** Состояние видимости диалога подтверждения исключения зрителя */
  const [kickDialogOpen, setKickDialogOpen] = useState(false);

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

  /**
   * Создает обработчик пункта меню, который закрывает меню и выполняет действие
   * @param action - Действие пункта меню
   * @returns {() => void} Обработчик клика по пункту меню
   */
  const menuAction = (action?: () => void) => () => {
    handleClose();
    action?.();
  };

  /** Подтверждение исключения зрителя */
  const confirmKick = () => {
    setKickDialogOpen(false);
    onKick?.();
  };

  return (
    <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
      <Typography
        variant="subtitle1"
        sx={{ color: viewer.online ? 'text.primary' : 'text.disabled' }}
      >
        {viewer.userName}
      </Typography>

      {hasActions && (
        <>
          <IconButton
            size="small"
            onClick={handleClick}
            color="inherit"
            aria-label={`Действия со зрителем ${viewer.userName}`}
            aria-haspopup="menu"
            aria-expanded={open}
          >
            <MoreVertIcon fontSize="small" />
          </IconButton>
          <Menu anchorEl={anchorEl} open={open} onClose={handleClose}>
            {viewer.canSync && <MenuItem onClick={menuAction(onSync)}>Синхронизировать</MenuItem>}

            {viewer.canKick && (
              <MenuItem onClick={menuAction(() => setKickDialogOpen(true))}>Выгнать</MenuItem>
            )}

            {viewer.canBeep && <MenuItem onClick={menuAction(onBeep)}>Разбудить</MenuItem>}

            {viewer.canScream && <MenuItem onClick={menuAction(onScream)}>Напугать</MenuItem>}
          </Menu>
          {viewer.canKick && (
            <Dialog open={kickDialogOpen} onClose={() => setKickDialogOpen(false)}>
              <DialogTitle>Исключение зрителя</DialogTitle>
              <DialogContent>
                <DialogContentText>
                  Вы уверены, что хотите выгнать зрителя {viewer.userName} из комнаты?
                </DialogContentText>
              </DialogContent>
              <DialogActions>
                <Button onClick={confirmKick} color="error">
                  Выгнать
                </Button>
                <Button onClick={() => setKickDialogOpen(false)} color="secondary" autoFocus>
                  Отмена
                </Button>
              </DialogActions>
            </Dialog>
          )}
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
    <Stack direction="row" sx={{ gap: 1, flexWrap: 'wrap', mt: 0.5 }}>
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
 * Форматирует время из тиков (100-наносекундных интервалов) в читаемый формат HH:MM:SS
 *
 * @param ticks - Время в тиках
 * @returns {string} Отформатированное время в формате HH:MM:SS
 */
function formatTime(ticks: number): string {
  const totalSeconds = Math.floor(ticks / 10_000_000);
  const h = Math.floor(totalSeconds / 3600)
    .toString()
    .padStart(2, '0');
  const m = Math.floor((totalSeconds % 3600) / 60)
    .toString()
    .padStart(2, '0');
  const s = (totalSeconds % 60).toString().padStart(2, '0');
  return `${h}:${m}:${s}`;
}

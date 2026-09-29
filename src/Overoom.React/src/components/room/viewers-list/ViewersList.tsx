import MenuIcon from '@mui/icons-material/Menu';
import { Box, Collapse, Divider, IconButton, Paper } from '@mui/material';
import { ReactElement, useMemo, useState } from 'react';

import { ViewerDto } from '../viewer/viewer.dto.ts';
import Viewer from '../viewer/Viewer.tsx';

/** Пропсы компонента ViewersList */
interface ViewersListProps {
  /** Список зрителей для отображения */
  viewers: ViewerDto[];
  /** Обработчик сигнала "бип" для зрителя */
  onBeep: (id: string) => void;
  /** Обработчик сигнала "крик" для зрителя */
  onScream: (id: string) => void;
  /** Обработчик исключения зрителя */
  onKick: (id: string) => void;
  /** Обработчик синхронизации для зрителя */
  onSync: () => void;
  /** Флаг начального состояния: true — список других зрителей развернут */
  isCollapsed?: boolean;
}

/**
 * Компонент списка зрителей с возможностью разворачивания/сворачивания
 * @param props - Свойства компонента
 * @returns {ReactElement} JSX элемент списка зрителей
 */
const ViewersList = (props: ViewersListProps): ReactElement => {
  const [show, setShow] = useState(props.isCollapsed ?? false);
  const currentViewer = useMemo(() => props.viewers.filter((v) => v.isCurrent)[0], [props.viewers]);
  const otherViewers = useMemo(() => props.viewers.filter((v) => !v.isCurrent), [props.viewers]);

  const open = useMemo(() => show && otherViewers.length > 0, [otherViewers.length, show]);

  return (
    <Paper>
      <Box
        sx={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
        }}
      >
        {currentViewer && (
          <Viewer
            viewer={currentViewer}
            onBeep={() => props.onBeep(currentViewer.id)}
            onScream={() => props.onScream(currentViewer.id)}
          />
        )}

        <IconButton
          color={open ? 'primary' : 'inherit'}
          onClick={() => setShow((v) => !v)}
          size="small"
          aria-label={show ? 'Скрыть список зрителей' : 'Показать список зрителей'}
          aria-expanded={open}
          aria-controls="room-viewers-list"
        >
          <MenuIcon />
        </IconButton>
      </Box>

      <Collapse in={open} id="room-viewers-list">
        <Divider sx={{ mt: 2 }} />
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
          {otherViewers.map((v) => (
            <Viewer
              key={v.id}
              viewer={v}
              onSync={props.onSync}
              onKick={() => props.onKick(v.id)}
              onBeep={() => props.onBeep(v.id)}
              onScream={() => props.onScream(v.id)}
            />
          ))}
        </Box>
      </Collapse>
    </Paper>
  );
};

export default ViewersList;

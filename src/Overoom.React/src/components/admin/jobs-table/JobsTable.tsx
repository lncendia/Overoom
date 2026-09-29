import { Cancel, Delete, Replay } from '@mui/icons-material';
import {
  Box,
  IconButton,
  LinearProgress,
  Link,
  Paper,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Tooltip,
  Typography,
} from '@mui/material';
import { ReactElement } from 'react';
import { Link as RouterLink } from 'react-router-dom';

import { routes } from '../../../routes.ts';
import { JobResponse, JobStage } from '../../../services/jobs/responses/job.response.ts';
import JobStatusChip from '../job-status/JobStatusChip.tsx';

/** Подписи этапов обработки */
const STAGE_LABELS: Record<JobStage, string> = {
  Downloading: 'Скачивание',
  Transcoding: 'Транскодирование',
  Uploading: 'Загрузка в хранилище',
  Publishing: 'Завершение',
};

/** Формат даты и времени в таблице */
const dateFormat = new Intl.DateTimeFormat('ru-RU', {
  day: '2-digit',
  month: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
});

/**
 * Форматирует дату из ответа API
 * @param value - Дата в формате ISO или null
 * @returns {string} Отформатированная дата или прочерк
 */
const formatDate = (value: string | null): string =>
  value ? dateFormat.format(new Date(value)) : '—';

/** Пропсы компонента JobsTable */
interface JobsTableProps {
  /** Задачи */
  jobs: JobResponse[];
  /** Названия фильмов по идентификаторам */
  filmTitles: Record<string, string>;
  /** Идентификаторы задач, для которых выполняется действие */
  busy: ReadonlySet<string>;
  /** Отмена задачи */
  onCancel: (job: JobResponse) => void;
  /** Повтор задачи */
  onRetry: (job: JobResponse) => void;
  /** Удаление задачи */
  onDelete: (job: JobResponse) => void;
}

/**
 * Таблица фоновых задач загрузки фильмов с действиями над ними
 * @param props - Пропсы компонента
 * @param props.jobs - Задачи
 * @param props.filmTitles - Названия фильмов по идентификаторам
 * @param props.busy - Задачи, для которых выполняется действие
 * @param props.onCancel - Отмена задачи
 * @param props.onRetry - Повтор задачи
 * @param props.onDelete - Удаление задачи
 * @returns {ReactElement} JSX элемент таблицы
 */
const JobsTable = ({
  jobs,
  filmTitles,
  busy,
  onCancel,
  onRetry,
  onDelete,
}: JobsTableProps): ReactElement => {
  return (
    <TableContainer component={Paper}>
      <Table size="small" sx={{ minWidth: 900 }}>
        <TableHead>
          <TableRow>
            <TableCell>Фильм</TableCell>
            <TableCell>Состояние</TableCell>
            <TableCell sx={{ width: 200 }}>Прогресс</TableCell>
            <TableCell>Добавлена</TableCell>
            <TableCell>Запущена</TableCell>
            <TableCell>Завершена</TableCell>
            <TableCell align="right">Действия</TableCell>
          </TableRow>
        </TableHead>
        <TableBody>
          {jobs.map((job) => {
            const isActive = job.status === 'Pending' || job.status === 'Running';
            const canRetry = job.status === 'Faulted' || job.status === 'Canceled';
            const canDelete = canRetry || job.status === 'Completed';
            const isBusy = busy.has(job.id);
            const film = job.film;
            const title =
              job.filmTitle ?? (film ? (filmTitles[film.id] ?? film.id) : 'Неизвестный фильм');

            return (
              <TableRow key={job.id} hover>
                <TableCell sx={{ maxWidth: 280 }}>
                  {film ? (
                    <Link component={RouterLink} to={routes.film(film.id)} underline="hover">
                      {title}
                    </Link>
                  ) : (
                    <Typography variant="body2">{title}</Typography>
                  )}
                  {film && (
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                      {[
                        film.season != null && film.episode != null
                          ? `S${film.season}E${film.episode}`
                          : null,
                        film.version,
                        film.resolution.replace('P', '') + 'p',
                      ]
                        .filter(Boolean)
                        .join(' · ')}
                    </Typography>
                  )}
                  {job.reason && (
                    <Typography
                      variant="caption"
                      color={job.status === 'Faulted' ? 'error' : 'text.secondary'}
                      sx={{ display: 'block', overflowWrap: 'anywhere' }}
                    >
                      {job.reason}
                    </Typography>
                  )}
                </TableCell>
                <TableCell>
                  <Stack spacing={0.5} sx={{ alignItems: 'flex-start' }}>
                    <JobStatusChip status={job.status} state={job.state} />
                    {job.retryAttempt > 0 && (
                      <Typography variant="caption" color="text.secondary">
                        Попытка {job.retryAttempt + 1}
                      </Typography>
                    )}
                  </Stack>
                </TableCell>
                <TableCell>
                  {job.progress != null && (
                    <Box>
                      <LinearProgress
                        variant="determinate"
                        value={job.progress}
                        color={job.status === 'Faulted' ? 'error' : 'primary'}
                      />
                      <Typography variant="caption" color="text.secondary">
                        {job.progress.toFixed(0)}%{job.stage ? ` · ${STAGE_LABELS[job.stage]}` : ''}
                      </Typography>
                    </Box>
                  )}
                </TableCell>
                <TableCell>{formatDate(job.submitted)}</TableCell>
                <TableCell>{formatDate(job.started)}</TableCell>
                <TableCell>{formatDate(job.completed ?? job.faulted)}</TableCell>
                <TableCell align="right" sx={{ whiteSpace: 'nowrap' }}>
                  {isActive && (
                    <Tooltip title="Отменить">
                      <span>
                        <IconButton
                          size="small"
                          color="warning"
                          aria-label="Отменить задачу"
                          disabled={isBusy}
                          onClick={() => onCancel(job)}
                        >
                          <Cancel fontSize="small" />
                        </IconButton>
                      </span>
                    </Tooltip>
                  )}
                  {canRetry && (
                    <Tooltip title="Повторить">
                      <span>
                        <IconButton
                          size="small"
                          color="primary"
                          aria-label="Повторить задачу"
                          disabled={isBusy}
                          onClick={() => onRetry(job)}
                        >
                          <Replay fontSize="small" />
                        </IconButton>
                      </span>
                    </Tooltip>
                  )}
                  {canDelete && (
                    <Tooltip title="Удалить">
                      <span>
                        <IconButton
                          size="small"
                          color="error"
                          aria-label="Удалить задачу"
                          disabled={isBusy}
                          onClick={() => onDelete(job)}
                        >
                          <Delete fontSize="small" />
                        </IconButton>
                      </span>
                    </Tooltip>
                  )}
                </TableCell>
              </TableRow>
            );
          })}
        </TableBody>
      </Table>
    </TableContainer>
  );
};

export default JobsTable;

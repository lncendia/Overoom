import { Add, Refresh } from '@mui/icons-material';
import {
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  IconButton,
  Stack,
  TablePagination,
  Tooltip,
} from '@mui/material';
import { useInjection } from 'inversify-react';
import { ReactElement, useCallback, useEffect, useRef, useState } from 'react';

import JobsTable from '../../../components/admin/jobs-table/JobsTable.tsx';
import SubmitJobForm, {
  FilmOption,
} from '../../../components/admin/submit-job-form/SubmitJobForm.tsx';
import { useNotify } from '../../../contexts/notify-context/useNotify.tsx';
import { useSafeCallback } from '../../../hooks/safe-callback-hook/useSafeCallback.ts';
import { FilmsApi } from '../../../services/films/films.api.ts';
import { JobsApi } from '../../../services/jobs/jobs.api.ts';
import { SubmitJobRequest } from '../../../services/jobs/requests/submit-job.request.ts';
import { JobResponse } from '../../../services/jobs/responses/job.response.ts';
import Drawer from '../../../ui/drawer/Drawer.tsx';
import LoadError from '../../../ui/load-error/LoadError.tsx';
import NoData from '../../../ui/no-data/NoData.tsx';
import Spinner from '../../../ui/spinners/Spinner.tsx';

/** Интервал автоматического обновления списка задач */
const REFRESH_INTERVAL = 5000;

/** Задержка перед обновлением списка после действия над задачей */
const ACTION_SETTLE_DELAY = 1500;

/** Действие, требующее подтверждения */
interface PendingAction {
  /** Тип действия */
  kind: 'cancel' | 'delete';
  /** Задача */
  job: JobResponse;
}

/**
 * Модуль администрирования фоновых задач загрузки фильмов.
 * Показывает список задач с автообновлением и позволяет ставить, отменять, повторять и удалять задачи.
 * @returns {ReactElement} JSX элемент модуля
 */
const JobsModule = (): ReactElement => {
  /** Сервис API фоновых задач */
  const jobsApi = useInjection<JobsApi>('JobsApi');

  /** Сервис API фильмов, нужен для названий и поиска */
  const filmsApi = useInjection<FilmsApi>('FilmsApi');

  /** Хук для работы с уведомлениями */
  const { setNotification, setError } = useNotify();

  /** Задачи на текущей странице */
  const [jobs, setJobs] = useState<JobResponse[]>([]);

  /** Общее количество задач */
  const [totalCount, setTotalCount] = useState(0);

  /** Номер страницы и её размер */
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(20);

  /** Первичная загрузка и ошибка загрузки */
  const [isLoading, setIsLoading] = useState(true);
  const [loadError, setLoadError] = useState(false);

  /** Названия фильмов по идентификаторам */
  const [filmTitles, setFilmTitles] = useState<Record<string, string>>({});

  /** Задачи, для которых выполняется действие */
  const [busy, setBusy] = useState<ReadonlySet<string>>(new Set());

  /** Действие, ожидающее подтверждения */
  const [pendingAction, setPendingAction] = useState<PendingAction | null>(null);

  /** Открыта ли форма постановки задачи */
  const [showForm, setShowForm] = useState(false);

  /** Номер последнего запроса списка, чтобы игнорировать устаревшие ответы */
  const requestId = useRef(0);

  /** Фильмы, названия которых уже запрошены */
  const requestedFilms = useRef(new Set<string>());

  /**
   * Загружает текущую страницу задач
   * @param silent - не показывать ошибку уведомлением (для автообновления)
   */
  const load = useCallback(
    async (silent = false) => {
      const id = ++requestId.current;
      try {
        const result = await jobsApi.get(page * pageSize, pageSize);
        if (id !== requestId.current) return;
        setJobs(result.list);
        setTotalCount(result.totalCount);
        setLoadError(false);
      } catch (e) {
        if (id !== requestId.current) return;
        setLoadError(true);
        if (!silent) setError(e as Error);
      } finally {
        if (id === requestId.current) setIsLoading(false);
      }
    },
    [jobsApi, page, pageSize, setError]
  );

  /** Загрузка при смене страницы и автообновление, пока вкладка видима */
  useEffect(() => {
    load();
    const timer = setInterval(() => {
      if (document.visibilityState === 'visible') load(true);
    }, REFRESH_INTERVAL);
    return () => clearInterval(timer);
  }, [load]);

  /** Подгружает названия фильмов, которых ещё нет в кэше */
  useEffect(() => {
    const missing = [
      ...new Set(
        jobs
          .filter((j) => !j.filmTitle)
          .map((j) => j.film?.id)
          .filter((id): id is string => !!id)
      ),
    ].filter((id) => !requestedFilms.current.has(id));

    missing.forEach((id) => {
      requestedFilms.current.add(id);
      filmsApi
        .get(id)
        .then((film) => setFilmTitles((prev) => ({ ...prev, [id]: film.title })))
        .catch(() => undefined);
    });
  }, [jobs, filmsApi]);

  /**
   * Выполняет действие над задачей и обновляет список
   * @param job - Задача
   * @param action - Вызов API
   * @param message - Текст уведомления об успехе
   */
  const runAction = useCallback(
    async (job: JobResponse, action: (id: string) => Promise<void>, message: string) => {
      setBusy((prev) => new Set(prev).add(job.id));
      try {
        await action(job.id);
        setNotification({ message, severity: 'success' });
        // Команды MassTransit обрабатываются асинхронно: сразу после ответа сага может быть ещё в старом состоянии
        await new Promise((resolve) => setTimeout(resolve, ACTION_SETTLE_DELAY));
        await load(true);
      } catch (e) {
        setError(e as Error);
      } finally {
        setBusy((prev) => {
          const next = new Set(prev);
          next.delete(job.id);
          return next;
        });
      }
    },
    [load, setNotification, setError]
  );

  /** Подтверждение отложенного действия */
  const confirmAction = () => {
    if (!pendingAction) return;
    const { kind, job } = pendingAction;
    setPendingAction(null);

    if (kind === 'cancel') runAction(job, (id) => jobsApi.cancel(id), 'Команда отмены отправлена');
    else runAction(job, (id) => jobsApi.delete(id), 'Задача удалена');
  };

  /**
   * Ищет фильмы по названию для формы постановки задачи
   * @param query - Строка поиска
   * @returns {Promise<FilmOption[]>} Найденные фильмы
   */
  const searchFilms = useCallback(
    async (query: string): Promise<FilmOption[]> => {
      const result = await filmsApi.search({ query, take: 10 });
      return result.list.map((f) => ({
        id: f.id,
        title: f.title,
        year: f.year,
        isSerial: f.isSerial,
      }));
    },
    [filmsApi]
  );

  /**
   * Ставит задачу в очередь
   * @param request - Данные задачи
   * @returns {Promise<boolean>} true, если задача поставлена
   */
  const submit = useSafeCallback(
    async (request: SubmitJobRequest): Promise<boolean> => {
      await jobsApi.submit(request);
      setNotification({ message: 'Задача поставлена в очередь', severity: 'success' });
      setShowForm(false);
      setPage(0);
      await load(true);
      return true;
    },
    [jobsApi, load, setNotification]
  );

  if (isLoading) return <Spinner />;

  return (
    <>
      <Stack direction="row" spacing={1} sx={{ mb: 2, justifyContent: 'flex-end' }}>
        <Tooltip title="Обновить">
          <IconButton aria-label="Обновить список задач" onClick={() => load()}>
            <Refresh />
          </IconButton>
        </Tooltip>
        <Button variant="contained" startIcon={<Add />} onClick={() => setShowForm(true)}>
          Новая задача
        </Button>
      </Stack>

      {loadError && jobs.length === 0 ? (
        <LoadError text="Не удалось загрузить задачи" onRetry={() => load()} />
      ) : jobs.length === 0 ? (
        <NoData text="Задач нет" />
      ) : (
        <>
          <JobsTable
            jobs={jobs}
            filmTitles={filmTitles}
            busy={busy}
            onCancel={(job) => setPendingAction({ kind: 'cancel', job })}
            onRetry={(job) => runAction(job, (id) => jobsApi.retry(id), 'Задача перезапущена')}
            onDelete={(job) => setPendingAction({ kind: 'delete', job })}
          />
          <Box>
            <TablePagination
              component="div"
              count={totalCount}
              page={page}
              onPageChange={(_, value) => setPage(value)}
              rowsPerPage={pageSize}
              rowsPerPageOptions={[10, 20, 50]}
              onRowsPerPageChange={(e) => {
                setPageSize(Number(e.target.value));
                setPage(0);
              }}
              labelRowsPerPage="На странице"
              labelDisplayedRows={({ from, to, count }) => `${from}–${to} из ${count}`}
            />
          </Box>
        </>
      )}

      <Dialog open={pendingAction !== null} onClose={() => setPendingAction(null)}>
        <DialogTitle>
          {pendingAction?.kind === 'cancel' ? 'Отмена задачи' : 'Удаление задачи'}
        </DialogTitle>
        <DialogContent>
          <DialogContentText>
            {pendingAction?.kind === 'cancel'
              ? 'Задача будет остановлена. Скачанные данные сохранятся, её можно будет повторить.'
              : 'Задача будет удалена из списка без возможности восстановления.'}
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={confirmAction} color="error">
            {pendingAction?.kind === 'cancel' ? 'Отменить задачу' : 'Удалить'}
          </Button>
          <Button onClick={() => setPendingAction(null)} color="secondary" autoFocus>
            Назад
          </Button>
        </DialogActions>
      </Dialog>

      <Drawer
        title="Новая задача"
        show={showForm}
        onClose={() => setShowForm(false)}
        anchor="right"
        width={480}
      >
        <SubmitJobForm
          searchFilms={searchFilms}
          onSubmit={async (r) => (await submit(r)) ?? false}
        />
      </Drawer>
    </>
  );
};

export default JobsModule;

import { Chip, ChipProps, Tooltip } from '@mui/material';
import { ReactElement } from 'react';

import { JobStatus } from '../../../services/jobs/responses/job.response.ts';

/** Подпись и цвет чипа для каждого состояния задачи */
const STATUS_VIEW: Record<JobStatus, { label: string; color: ChipProps['color'] }> = {
  Pending: { label: 'В очереди', color: 'default' },
  Running: { label: 'Выполняется', color: 'info' },
  Completed: { label: 'Готово', color: 'success' },
  Faulted: { label: 'Ошибка', color: 'error' },
  Canceled: { label: 'Отменена', color: 'warning' },
  Unknown: { label: 'Неизвестно', color: 'default' },
};

/** Пропсы компонента JobStatusChip */
interface JobStatusChipProps {
  /** Обобщённое состояние задачи */
  status: JobStatus;
  /** Состояние в терминах MassTransit, показывается в подсказке */
  state: string;
}

/**
 * Чип состояния фоновой задачи
 * @param props - Пропсы компонента
 * @param props.status - Обобщённое состояние задачи
 * @param props.state - Состояние в терминах MassTransit
 * @returns {ReactElement} JSX элемент чипа
 */
const JobStatusChip = ({ status, state }: JobStatusChipProps): ReactElement => {
  const view = STATUS_VIEW[status] ?? STATUS_VIEW.Unknown;

  return (
    <Tooltip title={state}>
      <Chip size="small" label={view.label} color={view.color} />
    </Tooltip>
  );
};

export default JobStatusChip;

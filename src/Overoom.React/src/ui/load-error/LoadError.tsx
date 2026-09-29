import { Button, Paper, Stack, Typography } from '@mui/material';
import { ReactElement } from 'react';

/** Свойства для компонента LoadError */
export interface LoadErrorProps {
  /** Текст сообщения об ошибке */
  text?: string;
  /** Обработчик повторной попытки загрузки */
  onRetry: () => void;
}

/**
 * Компонент для отображения ошибки загрузки данных с кнопкой повтора
 * @param props - Свойства компонента
 * @param props.text - Текст сообщения об ошибке
 * @param props.onRetry - Обработчик повторной попытки загрузки
 * @returns {ReactElement} JSX элемент с сообщением об ошибке
 */
const LoadError = ({
  text = 'Не удалось загрузить данные',
  onRetry,
}: LoadErrorProps): ReactElement => {
  return (
    <Paper>
      <Stack
        direction="row"
        spacing={2}
        sx={{ alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap' }}
      >
        <Typography>{text}</Typography>
        <Button variant="outlined" size="small" onClick={onRetry}>
          Повторить
        </Button>
      </Stack>
    </Paper>
  );
};

export default LoadError;

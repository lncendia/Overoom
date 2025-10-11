import { Box, Typography, Chip, Stack, styled, Tooltip } from '@mui/material';
import { ReactElement } from 'react';

/** Свойства для компонента KeyList */
export interface FilmKeyProps {
  /** Заголовок списка */
  title: string;
  /** Массив значений: либо строки, либо пары [ключ, описание?] */
  values: string[] | [string, string | null][];
  /** Обработчик выбора ключа (опционально) */
  onKeySelect?: (key: string) => void;
}

/** Стилизованный чип для отображения значения с hover эффектом */
const ValueChip = styled(Chip)(({ theme }) => ({
  marginRight: theme.spacing(0.5),
  marginBottom: theme.spacing(0.5),
  borderRadius: theme.shape.borderRadius,
  '&:hover': {
    backgroundColor: theme.palette.action.hover,
  },
  '& .MuiChip-label': {
    paddingLeft: theme.spacing(1),
    paddingRight: theme.spacing(1),
  },
}));

/**
 * Компонент списка ключей с возможностью выбора значения и подсказкой
 * @param props - Свойства компонента
 * @param props.title - Заголовок списка
 * @param props.values - Массив значений (строки или пары [ключ, описание])
 * @param props.onKeySelect - Функция-обработчик выбора значения
 * @returns {ReactElement} JSX элемент списка ключей
 */
const KeyList = ({ title, values, onKeySelect }: FilmKeyProps): ReactElement => {
  // Нормализуем данные в единый формат: {name, description}
  const normalizedValues = values.map((item) =>
    Array.isArray(item) ? { name: item[0], description: item[1] } : { name: item }
  );

  return (
    <Box sx={{ mb: 2, display: 'flex', gap: 1 }}>
      {/* Заголовок списка */}
      <Typography component="span" variant="subtitle2" gutterBottom>
        {title}
      </Typography>

      {/* Блок с чипами */}
      <Stack direction="row" flexWrap="wrap">
        {normalizedValues.map(({ name, description }) => {
          // Создаем чип с кликабельностью при наличии обработчика
          const chip = (
            <ValueChip
              label={name}
              clickable={!!onKeySelect}
              onClick={onKeySelect ? () => onKeySelect(name) : undefined}
              variant="outlined"
              size="small"
            />
          );

          return (
            <Box key={name} sx={{ display: 'flex', alignItems: 'center' }}>
              {/* Если есть описание, оборачиваем чип в Tooltip */}
              {description ? <Tooltip title={description}>{chip}</Tooltip> : chip}
            </Box>
          );
        })}
      </Stack>
    </Box>
  );
};

export default KeyList;

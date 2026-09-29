import { Autocomplete, Box, Button, MenuItem, Stack, TextField } from '@mui/material';
import { useFormik } from 'formik';
import { ReactElement, useEffect, useRef, useState } from 'react';
import * as yup from 'yup';

import { SubmitJobRequest } from '../../../services/jobs/requests/submit-job.request.ts';
import { FilmResolution } from '../../../services/jobs/responses/job.response.ts';

/** Фильм в выпадающем списке поиска */
export interface FilmOption {
  /** Идентификатор фильма */
  id: string;
  /** Название */
  title: string;
  /** Год выпуска */
  year: number;
  /** Является ли сериалом */
  isSerial: boolean;
}

/** Доступные разрешения исходного видео */
const RESOLUTIONS: { value: FilmResolution; label: string }[] = [
  { value: 'P2160', label: '2160p (4K)' },
  { value: 'P1080', label: '1080p' },
  { value: 'P720', label: '720p' },
  { value: 'P480', label: '480p' },
  { value: 'P360', label: '360p' },
];

/** Значения формы */
interface FormValues {
  film: FilmOption | null;
  magnetUri: string;
  fileName: string;
  resolution: FilmResolution;
  version: string;
  season: string;
  episode: string;
}

/** Схема валидации формы, повторяет правила сервера */
const schema = yup.object({
  film: yup.object().nullable().required('Выберите фильм'),
  magnetUri: yup
    .string()
    .trim()
    .required('Укажите magnet-ссылку')
    .matches(
      /^magnet:\?xt=urn:btih:[a-f0-9]{40,64}(&[a-z0-9]+=[^&]*)*$/i,
      'Некорректная magnet-ссылка'
    ),
  fileName: yup.string().trim().max(1000, 'Не длиннее 1000 символов'),
  resolution: yup.string().required('Укажите разрешение'),
  version: yup.string().trim().required('Укажите версию').max(100, 'Не длиннее 100 символов'),
  season: yup.string().when('film', {
    is: (film: FilmOption | null) => film?.isSerial,
    then: (s) => s.required('Укажите сезон').matches(/^[1-9]\d*$/, 'Положительное число'),
  }),
  episode: yup.string().when('film', {
    is: (film: FilmOption | null) => film?.isSerial,
    then: (s) => s.required('Укажите серию').matches(/^[1-9]\d*$/, 'Положительное число'),
  }),
});

/** Пропсы компонента SubmitJobForm */
interface SubmitJobFormProps {
  /** Поиск фильмов по названию */
  searchFilms: (query: string) => Promise<FilmOption[]>;
  /** Отправка задачи; форма очищается, если вернулось true */
  onSubmit: (request: SubmitJobRequest) => Promise<boolean>;
}

/**
 * Форма постановки задачи загрузки фильма в очередь
 * @param props - Пропсы компонента
 * @param props.searchFilms - Поиск фильмов по названию
 * @param props.onSubmit - Отправка задачи
 * @returns {ReactElement} JSX элемент формы
 */
const SubmitJobForm = ({ searchFilms, onSubmit }: SubmitJobFormProps): ReactElement => {
  /** Строка поиска фильма */
  const [query, setQuery] = useState('');

  /** Найденные фильмы */
  const [options, setOptions] = useState<FilmOption[]>([]);

  /** Идёт ли поиск */
  const [searching, setSearching] = useState(false);

  /** Последний отправленный запрос поиска, чтобы игнорировать устаревшие ответы */
  const lastQuery = useRef('');

  const formik = useFormik<FormValues>({
    initialValues: {
      film: null,
      magnetUri: '',
      fileName: '',
      resolution: 'P1080',
      version: '',
      season: '',
      episode: '',
    },
    validationSchema: schema,
    onSubmit: async (values, { resetForm }) => {
      const serial = values.film?.isSerial;
      const success = await onSubmit({
        filmId: values.film!.id,
        filmTitle: values.film!.title,
        magnetUri: values.magnetUri.trim(),
        fileName: values.fileName.trim() || undefined,
        resolution: values.resolution,
        version: values.version.trim(),
        season: serial ? Number(values.season) : undefined,
        episode: serial ? Number(values.episode) : undefined,
      });
      if (success) resetForm();
    },
  });

  /** Поиск фильмов с задержкой после ввода */
  useEffect(() => {
    const trimmed = query.trim();
    if (trimmed.length < 2) {
      setOptions([]);
      return;
    }

    const timer = setTimeout(() => {
      lastQuery.current = trimmed;
      setSearching(true);
      searchFilms(trimmed)
        .then((films) => {
          if (lastQuery.current === trimmed) setOptions(films);
        })
        .catch(() => setOptions([]))
        .finally(() => {
          if (lastQuery.current === trimmed) setSearching(false);
        });
    }, 400);

    return () => clearTimeout(timer);
  }, [query, searchFilms]);

  /**
   * Возвращает текст ошибки поля, если поле было затронуто
   * @param field - Имя поля
   * @returns {string | undefined} Текст ошибки
   */
  const errorOf = (field: keyof FormValues): string | undefined =>
    formik.touched[field] && formik.errors[field] ? String(formik.errors[field]) : undefined;

  return (
    <Box
      component="form"
      onSubmit={formik.handleSubmit}
      sx={{ display: 'flex', flexDirection: 'column', gap: 2, width: '100%' }}
    >
      <Autocomplete
        options={options}
        value={formik.values.film}
        loading={searching}
        filterOptions={(x) => x}
        getOptionLabel={(o) => `${o.title} (${o.year})`}
        isOptionEqualToValue={(a, b) => a.id === b.id}
        noOptionsText={query.trim().length < 2 ? 'Введите название' : 'Ничего не найдено'}
        onInputChange={(_, value) => setQuery(value)}
        onChange={(_, value) => formik.setFieldValue('film', value)}
        onBlur={() => formik.setFieldTouched('film', true)}
        renderInput={(params) => (
          <TextField
            {...params}
            label="Фильм"
            error={!!errorOf('film')}
            helperText={errorOf('film')}
          />
        )}
      />

      <TextField
        name="magnetUri"
        label="Magnet-ссылка"
        multiline
        minRows={2}
        value={formik.values.magnetUri}
        onChange={formik.handleChange}
        onBlur={formik.handleBlur}
        error={!!errorOf('magnetUri')}
        helperText={errorOf('magnetUri')}
      />

      <TextField
        name="fileName"
        label="Файл внутри торрента"
        value={formik.values.fileName}
        onChange={formik.handleChange}
        onBlur={formik.handleBlur}
        error={!!errorOf('fileName')}
        helperText={errorOf('fileName') ?? 'Нужен, если в торренте несколько файлов'}
      />

      <Stack direction="row" spacing={2}>
        <TextField
          select
          name="resolution"
          label="Разрешение"
          value={formik.values.resolution}
          onChange={formik.handleChange}
          sx={{ flex: 1 }}
        >
          {RESOLUTIONS.map((r) => (
            <MenuItem key={r.value} value={r.value}>
              {r.label}
            </MenuItem>
          ))}
        </TextField>

        <TextField
          name="version"
          label="Версия"
          placeholder="Например, Дубляж"
          value={formik.values.version}
          onChange={formik.handleChange}
          onBlur={formik.handleBlur}
          error={!!errorOf('version')}
          helperText={errorOf('version')}
          sx={{ flex: 1 }}
        />
      </Stack>

      {formik.values.film?.isSerial && (
        <Stack direction="row" spacing={2}>
          <TextField
            name="season"
            label="Сезон"
            type="number"
            value={formik.values.season}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={!!errorOf('season')}
            helperText={errorOf('season')}
            sx={{ flex: 1 }}
          />
          <TextField
            name="episode"
            label="Серия"
            type="number"
            value={formik.values.episode}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={!!errorOf('episode')}
            helperText={errorOf('episode')}
            sx={{ flex: 1 }}
          />
        </Stack>
      )}

      <Button
        type="submit"
        variant="contained"
        fullWidth
        disabled={formik.isSubmitting}
        sx={{ mt: 1, py: 1.5 }}
      >
        Поставить в очередь
      </Button>
    </Box>
  );
};

export default SubmitJobForm;

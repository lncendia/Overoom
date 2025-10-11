import {
  Box,
  Button,
  FormControl,
  FormHelperText,
  InputLabel,
  OutlinedInput,
  CircularProgress,
} from '@mui/material';
import { useFormik } from 'formik';
import { ReactElement } from 'react';
import * as yup from 'yup';

/** Схема валидации формы подключения к комнате */
const connectFilmRoomFormSchema = yup.object().shape({
  code: yup
    .string()
    .min(5, 'Введите 5-ти значный код')
    .max(5, 'Введите 5-ти значный код')
    .required('Введите 5-ти значный код'),
});

/** Пропсы компонента ConnectRoomForm */
interface ConnectRoomFormProps {
  /** Флаг процесса подключения */
  isConnecting: boolean;
  /** Флаг необходимости ввода кода для подключения */
  codeNeeded: boolean;
  /** Начальное значение кода комнаты */
  code?: string;
  /** Функция обработки подключения к комнате */
  onConnect: (code?: string) => void;
}

/**
 * Форма подключения к комнате с возможностью ввода кода
 * @param props - Пропсы компонента
 * @param props.isConnecting - Флаг процесса подключения
 * @param props.codeNeeded - Флаг необходимости ввода кода
 * @param props.code - Начальное значение кода
 * @param props.onConnect - Функция обработки подключения
 * @returns {ReactElement} JSX элемент формы подключения
 */
const ConnectRoomForm = (props: ConnectRoomFormProps): ReactElement => {
  const { isConnecting, codeNeeded, onConnect, code } = props;

  /** Инициализация формы с использованием useFormik */
  const formik = useFormik({
    initialValues: {
      code: code || '',
    },
    validationSchema: codeNeeded ? connectFilmRoomFormSchema : null,
    onSubmit: (values: { code?: string }) => {
      onConnect(codeNeeded ? values.code : undefined);
    },
  });

  return (
    <Box
      component="form"
      onSubmit={formik.handleSubmit}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 3,
        width: '100%',
      }}
    >
      {/* Условное отображение поля ввода кода комнаты */}
      {codeNeeded && (
        <FormControl fullWidth error={formik.touched.code && Boolean(formik.errors.code)}>
          <InputLabel htmlFor="code-input">Код комнаты</InputLabel>
          <OutlinedInput
            id="code-input"
            name="code"
            value={formik.values.code}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            label="Код комнаты"
            aria-describedby="code-error-text"
            disabled={isConnecting}
          />
          {formik.touched.code && formik.errors.code && (
            <FormHelperText id="code-error-text">{formik.errors.code}</FormHelperText>
          )}
        </FormControl>
      )}

      {/* Кнопка подключения с индикатором загрузки */}
      <Button
        type="submit"
        variant="contained"
        color="primary"
        fullWidth
        disabled={isConnecting}
        sx={{
          mt: 2,
          py: 1.5,
          fontSize: '1rem',
          fontWeight: 'medium',
          position: 'relative',
        }}
      >
        {isConnecting ? (
          <>
            <CircularProgress
              size={24}
              sx={{
                color: 'inherit',
                position: 'absolute',
                top: '50%',
                left: '50%',
                marginTop: '-12px',
                marginLeft: '-12px',
              }}
            />
            {/* Скрытый текст для сохранения размера кнопки */}
            <span style={{ opacity: 0 }}>Подключиться</span>
          </>
        ) : (
          'Подключиться'
        )}
      </Button>
    </Box>
  );
};

export default ConnectRoomForm;

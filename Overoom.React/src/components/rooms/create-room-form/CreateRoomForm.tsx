import { Button, Checkbox, FormControlLabel, Box } from '@mui/material';
import { useFormik } from 'formik';
import { ReactElement } from 'react';
import * as yup from 'yup';

/** Схема валидации формы создания комнаты */
const createFilmRoomFormSchema = yup.object().shape({
  open: yup.boolean().required('Укажите тип комнаты'),
});

/** Пропсы компонента CreateRoomForm */
interface CreateRoomFormProps {
  /** Функция обратного вызова при отправке формы, принимает значение публичности комнаты */
  callback: (open: boolean) => void;
}

/**
 * Форма создания комнаты для просмотра фильма
 * @param props - Пропсы компонента
 * @param props.callback - Функция обратного вызова при создании комнаты
 * @returns {ReactElement} JSX элемент формы создания комнаты
 */
const CreateRoomForm = ({ callback }: CreateRoomFormProps): ReactElement => {
  /** Инициализация формы с помощью useFormik */
  const formik = useFormik({
    initialValues: {
      open: false,
    },
    validationSchema: createFilmRoomFormSchema,
    onSubmit: (values) => {
      callback(values.open);
    },
  });

  return (
    <Box
      component="form"
      onSubmit={formik.handleSubmit}
      sx={{
        display: 'flex',
        flexDirection: 'column',
        gap: 2,
        width: '100%',
      }}
    >
      {/* Чекбокс "Публичная комната" */}
      <FormControlLabel
        control={
          <Checkbox
            name="open"
            checked={formik.values.open}
            onChange={formik.handleChange}
            color="primary"
          />
        }
        label="Публичная комната"
      />

      {/* Кнопка отправки формы */}
      <Button
        type="submit"
        variant="contained"
        color="primary"
        fullWidth
        sx={{
          mt: 2,
          py: 1.5,
          fontSize: '1rem',
          fontWeight: 'medium',
        }}
      >
        Создать
      </Button>
    </Box>
  );
};

export default CreateRoomForm;

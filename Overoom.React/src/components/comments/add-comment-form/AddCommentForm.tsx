import { Button, TextField, Box, Typography, Stack, Paper } from '@mui/material';
import { useFormik } from 'formik';
import { ReactElement } from 'react';
import * as yup from 'yup';

/**  Схема валидации */
const addCommentFormSchema = yup.object().shape({
  comment: yup.string().required('Поле не должно быть пустым').max(1000, 'Не больше 1000 символов'),
});

/** Пропсы компонента AddCommentForm */
interface AddCommentFormProps {
  /** Функция обратного вызова при отправке формы */
  callback: (text: string) => void;
}

/**
 * Компонент формы добавления комментария
 * @param props - Пропсы компонента
 * @param props.callback - Пропсы компонента
 * @returns {ReactElement} Форма для добавления комментария
 */
const AddCommentForm = ({ callback }: AddCommentFormProps): ReactElement => {
  const formik = useFormik({
    initialValues: {
      comment: '',
    },
    validationSchema: addCommentFormSchema,
    onSubmit: (values) => {
      callback(values.comment);
      formik.resetForm();
    },
  });

  return (
    <Paper>
      <Box component="form" onSubmit={formik.handleSubmit} sx={{ width: '100%' }}>
        <Stack spacing={2}>
          <Typography variant="subtitle1">Оставьте комментарий</Typography>

          <TextField
            name="comment"
            value={formik.values.comment}
            onChange={formik.handleChange}
            onBlur={formik.handleBlur}
            error={formik.touched.comment && Boolean(formik.errors.comment)}
            helperText={formik.touched.comment && formik.errors.comment}
            multiline
            rows={4}
            fullWidth
            variant="outlined"
          />

          <Button
            type="submit"
            variant="contained"
            color="primary"
            sx={{ alignSelf: 'flex-start' }}
          >
            Добавить
          </Button>
        </Stack>
      </Box>
    </Paper>
  );
};

export default AddCommentForm;

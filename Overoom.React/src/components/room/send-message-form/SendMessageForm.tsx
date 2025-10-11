import SendIcon from '@mui/icons-material/Send';
import { Box, TextField, IconButton, useTheme, InputAdornment } from '@mui/material';
import { Formik } from 'formik';
import { ReactElement, useCallback } from 'react';
import * as yup from 'yup';

/** Пропсы компонента SendMessageForm */
interface SendMessageFormProps {
  /** Функция отправки сообщения */
  onSend: (text: string) => void;
  /** Функция обработки ввода текста (например, уведомление о печати) */
  onTyping: () => void;
}

/** Схема валидации формы */
const sendMessageFormSchema = yup.object().shape({
  text: yup.string().required('Поле не должно быть пустым').max(1000, 'Не больше 1000 символов'),
});

/**
 * Компонент формы отправки сообщения с возможностью отслеживания ввода
 * @param props - Пропсы компонента
 * @param props.onSend - Функция отправки сообщения
 * @param props.onTyping - Функция обработки ввода текста
 * @returns {ReactElement} JSX элемент формы отправки сообщения
 */
const SendMessageForm = ({ onSend, onTyping }: SendMessageFormProps): ReactElement => {
  /** Используем хук useTheme из Material-UI для получения текущей темы */
  const theme = useTheme();

  /**
   * Функция обработки отправки формы
   * @param values - Значения формы
   * @param param1.resetForm - Функция сброса формы
   */
  const handleSubmit = useCallback(
    (values: { text: string }, { resetForm }: { resetForm: () => void }) => {
      onSend(values.text);
      resetForm();
    },
    [onSend]
  );

  return (
    <Formik
      validationSchema={sendMessageFormSchema}
      onSubmit={handleSubmit}
      initialValues={{ text: '' }}
    >
      {({ handleSubmit, handleChange, values }) => (
        <Box component="form" onSubmit={handleSubmit} sx={{ width: '100%' }}>
          <TextField
            fullWidth
            variant="outlined"
            placeholder="Отправьте сообщение"
            name="text"
            value={values.text}
            onChange={(e) => {
              handleChange(e);
              onTyping(); // вызываем callback для уведомления о печати
            }}
            onKeyDown={(event) => {
              if (event.key === 'Enter' && !event.ctrlKey && !event.shiftKey) {
                event.preventDefault();
                handleSubmit(); // отправка сообщения по Enter
              }
            }}
            multiline
            rows={2}
            slotProps={{
              input: {
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton type="submit" color="primary" disabled={!values.text.trim()}>
                      <SendIcon />
                    </IconButton>
                  </InputAdornment>
                ),
              },
              root: {
                sx: {
                  borderRadius: theme.shape.borderRadius,
                  backgroundColor: theme.palette.background.paper,
                },
              },
            }}
          />
        </Box>
      )}
    </Formik>
  );
};

export default SendMessageForm;

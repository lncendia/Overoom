import { Button } from '@mui/material';
import { ReactElement } from 'react';

/** Интерфейс пропсов компонента SignIn */
interface SignInProps {
  /** Коллбэк для перехода на страницу авторизации */
  onLogin: () => void;
}

/**
 * Компонент, отображаемый вместо UserInfo, если пользователь не авторизован.
 * Показывает кнопку входа в аккаунт.
 * @param props - Объект с коллбэком для авторизации
 * @param props.onLogin - Коллбэк для перехода на страницу авторизации
 * @returns {ReactElement} JSX элемент блока авторизации
 */
const SignIn = ({ onLogin }: SignInProps): ReactElement => {
  return (
    <Button color="inherit" variant="outlined" size="small" onClick={onLogin} sx={{ ml: 1 }}>
      Войти
    </Button>
  );
};

export default SignIn;

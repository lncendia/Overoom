import { useInjection } from 'inversify-react';
import { ReactElement, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import Redirecting from '../../components/pages/Redirecting.tsx';
import { useSafeCallback } from '../../hooks/safe-callback-hook/useSafeCallback.ts';
import { AuthApi } from '../../services/auth/auth.api.ts';

/**
 * Модуль аутентификации пользователя
 * @returns {ReactElement} JSX элемент страницы перенаправления
 */
const SignInModule = (): ReactElement => {
  /** Используем хук useNavigate для программной навигации между страницами */
  const navigate = useNavigate();

  /** Используем хук useInjection для получения экземпляра IAuthApi */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Колбэк, выполняющий callback входа и перенаправление. */
  const handleSignInCallback = useSafeCallback(async () => {
    await authApi.signInCallback();
    navigate('/');
  }, [authApi, navigate]);

  /** Выполняем колбэк при монтировании компонента. */
  useEffect(() => {
    handleSignInCallback();
  }, [handleSignInCallback]);

  // Возвращаем JSX элемент, который содержит структуру страницы с текстом "Перенаправление"
  return <Redirecting />;
};

export default SignInModule;

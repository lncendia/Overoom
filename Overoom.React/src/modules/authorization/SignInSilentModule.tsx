import { useInjection } from 'inversify-react';
import { ReactElement, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

import Redirecting from '../../components/pages/Redirecting.tsx';
import { useSafeCallback } from '../../hooks/safe-callback-hook/useSafeCallback.ts';
import { AuthApi } from '../../services/auth/auth.api.ts';

/**
 * Модуль silent аутентификации пользователя
 @returns {ReactElement} JSX элемент страницы перенаправления
 */
const SignInSilentModule = (): ReactElement => {
  /** Используем хук useNavigate для программной навигации между страницами */
  const navigate = useNavigate();

  /** Используем хук useInjection для получения экземпляра IAuthApi */
  const authApi = useInjection<AuthApi>('AuthApi');

  /** Колбэк для выполнения silent callback и перенаправления */
  const handleSilentCallback = useSafeCallback(async () => {
    await authApi.signInSilentCallback();
    navigate('/');
  }, [authApi, navigate]);

  /** Выполнение при монтировании компонента */
  useEffect(() => {
    handleSilentCallback();
  }, [handleSilentCallback]);

  // Возвращаем JSX элемент, который содержит структуру страницы с текстом "Перенаправление"
  return <Redirecting />;
};

export default SignInSilentModule;

import { useInjection } from 'inversify-react';
import { ReactElement, useEffect, useRef } from 'react';
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

  /** Флаг, исключающий повторную обработку callback */
  const handledRef = useRef(false);

  /** Колбэк, выполняющий callback входа и перенаправление на исходную страницу. */
  const handleSignInCallback = useSafeCallback(async () => {
    try {
      const returnUrl = await authApi.signInCallback();
      navigate(returnUrl, { replace: true });
    } catch (error) {
      navigate('/', { replace: true });
      throw error;
    }
  }, [authApi, navigate]);

  /** Выполняем колбэк при монтировании компонента. */
  useEffect(() => {
    if (handledRef.current) return;
    handledRef.current = true;
    handleSignInCallback().then();
  }, [handleSignInCallback]);

  return <Redirecting />;
};

export default SignInModule;

import { ReactElement } from 'react';

import SignInSilentModule from '../modules/authorization/SignInSilentModule.tsx';

/**
 * Страница silent-входа пользователя.
 * Используется для фоновой авторизации без участия пользователя.
 * @returns {ReactElement} JSX-элемент страницы silent-входа
 */
const SignInSilentPage = (): ReactElement => {
  // Рендерим модуль фоновой авторизации
  return <SignInSilentModule />;
};

export default SignInSilentPage;

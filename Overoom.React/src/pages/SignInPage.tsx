import { ReactElement } from 'react';

import SignInModule from '../modules/authorization/SignInModule.tsx';

/**
 * Страница входа пользователя.
 * Отображает модуль авторизации с формой входа.
 * @returns {ReactElement} JSX-элемент страницы входа
 */
const SignInPage = (): ReactElement => {
  // Рендерим модуль авторизации
  return <SignInModule />;
};

export default SignInPage;

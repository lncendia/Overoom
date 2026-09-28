import { Container } from '@mui/material';
import { ReactElement } from 'react';
import { isRouteErrorResponse, useNavigate, useRouteError } from 'react-router-dom';

import Error from '../components/pages/Error.tsx';
import NotFound from '../components/pages/NotFound.tsx';
import FooterModule from '../modules/home/footer-module/FooterModule.tsx';
import NavbarModule from '../modules/home/navbar-module/NavbarModule.tsx';

/**
 * Страница отображения ошибок маршрутизации и общих ошибок приложения.
 * Отображает компонент `NotFound` для ошибок 404 и `Error` для прочих.
 * @returns {ReactElement} JSX элемент страницы с обработкой ошибок
 */
const ErrorPage = (): ReactElement => {
  /** Текущая ошибка маршрута, если есть */
  const error = useRouteError();

  /** Хук для навигации между страницами */
  const navigate = useNavigate();

  /** Компонент, который будет отображён (NotFound или Error) */
  let page: ReactElement;

  if (isRouteErrorResponse(error)) {
    page = <NotFound action={() => navigate('/')} />;
  } else {
    page = <Error action={() => window.location.reload()} />;
  }

  return (
    <>
      <NavbarModule />

      <Container
        sx={{ paddingTop: '5.5rem !important' }}
        className="background-container"
        maxWidth={false}
      >
        {page}
      </Container>

      <FooterModule />
    </>
  );
};

export default ErrorPage;

import { Container } from '@mui/material';
import { ReactElement } from 'react';
import { Outlet } from 'react-router-dom';

import FooterModule from '../modules/home/footer-module/FooterModule.tsx';
import NavbarModule from '../modules/home/navbar-module/NavbarModule.tsx';

/**
 * Основной layout компонент приложения
 * @returns {ReactElement} JSX элемент layout страницы
 */
const LayoutPage = (): ReactElement => {
  return (
    <>
      {/* Модуль навигационной панели */}
      <NavbarModule />

      {/* Основной контейнер контента */}
      <Container
        sx={{ paddingTop: '5.5rem !important' }}
        className="background-container"
        maxWidth={false}
      >
        {/* Outlet для рендеринга дочерних страниц роутинга */}
        <Outlet />
      </Container>

      {/* Модуль подвала страницы */}
      <FooterModule />
    </>
  );
};

export default LayoutPage;

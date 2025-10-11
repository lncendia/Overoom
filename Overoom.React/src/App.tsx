import { ReactElement } from 'react';
import { createBrowserRouter, RouterProvider } from 'react-router-dom';

import './index.scss';

import CatalogPage from './pages/CatalogPage.tsx';
import ErrorPage from './pages/ErrorPage.tsx';
import FilmPage from './pages/FilmPage.tsx';
import FilmSearchPage from './pages/FilmSearchPage.tsx';
import HomePage from './pages/HomePage.tsx';
import LayoutPage from './pages/LayoutPage.tsx';
import PlaylistPage from './pages/PlaylistPage.tsx';
import PlaylistsPage from './pages/PlaylistsPage.tsx';
import ProfilePage from './pages/ProfilePage.tsx';
import RoomPage from './pages/RoomPage.tsx';
import RoomsPage from './pages/RoomsPage.tsx';
import SignInPage from './pages/SignInPage.tsx';
import SignInSilentPage from './pages/SignInSilentPage.tsx';
import SignOutPage from './pages/SignOutPage.tsx';

/**
 * Основной компонент приложения с маршрутизацией
 * @returns {ReactElement} JSX элемент, содержащий RouterProvider с маршрутизированными страницами
 */
const App = (): ReactElement => {
  // Создаем объект BrowserRouter для маршрутизации страниц
  const router = createBrowserRouter([
    {
      path: '/',
      element: <HomePage />,
      errorElement: <ErrorPage />,
    },
    {
      // Основной элемент-обертка (layout)
      element: <LayoutPage />,
      errorElement: <ErrorPage />,
      children: [
        { path: '/catalog', element: <CatalogPage /> },
        { path: '/playlists', element: <PlaylistsPage /> },
        { path: '/playlist', element: <PlaylistPage /> },
        { path: '/search', element: <FilmSearchPage /> },
        { path: '/film', element: <FilmPage /> },
        { path: '/profile', element: <ProfilePage /> },
        { path: '/rooms', element: <RoomsPage /> },
        { path: '/room', element: <RoomPage /> },
        { path: '/signin-oidc', element: <SignInPage /> },
        { path: '/signin-silent-oidc', element: <SignInSilentPage /> },
        { path: '/signout-oidc', element: <SignOutPage /> },
      ],
    },
  ]);

  // Возвращаем маршрутизированные страницы через RouterProvider
  return <RouterProvider router={router} />;
};

export default App;

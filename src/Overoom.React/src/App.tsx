import { lazy, ReactElement, Suspense } from 'react';
import {
  createBrowserRouter,
  data,
  LoaderFunctionArgs,
  redirect,
  RouterProvider,
} from 'react-router-dom';

import ErrorPage from './pages/ErrorPage.tsx';
import LayoutPage from './pages/LayoutPage.tsx';
import { routes } from './routes.ts';
import Spinner from './ui/spinners/Spinner.tsx';

/** Страницы приложения, загружаемые по требованию (отдельными чанками) */
const HomePage = lazy(() => import('./pages/HomePage.tsx'));
const CatalogPage = lazy(() => import('./pages/CatalogPage.tsx'));
const PlaylistsPage = lazy(() => import('./pages/PlaylistsPage.tsx'));
const PlaylistPage = lazy(() => import('./pages/PlaylistPage.tsx'));
const FilmSearchPage = lazy(() => import('./pages/FilmSearchPage.tsx'));
const FilmPage = lazy(() => import('./pages/FilmPage.tsx'));
const ProfilePage = lazy(() => import('./pages/ProfilePage.tsx'));
const RoomsPage = lazy(() => import('./pages/RoomsPage.tsx'));
const RoomPage = lazy(() => import('./pages/RoomPage.tsx'));
const AdminJobsPage = lazy(() => import('./pages/AdminJobsPage.tsx'));
const SignInPage = lazy(() => import('./pages/SignInPage.tsx'));
const SignInSilentPage = lazy(() => import('./pages/SignInSilentPage.tsx'));
const SignOutPage = lazy(() => import('./pages/SignOutPage.tsx'));

/**
 * Создает loader, перенаправляющий старые ссылки вида `/path?id=X` на новые адреса
 * @param build - Функция построения нового адреса по идентификатору и query-параметрам
 * @param fallback - Адрес, на который выполняется перенаправление при отсутствии идентификатора
 * @returns Loader маршрута
 */
const legacyRedirect =
  (build: (id: string, params: URLSearchParams) => string, fallback: string) =>
  ({ request }: LoaderFunctionArgs): Response => {
    const params = new URL(request.url).searchParams;
    const id = params.get('id');
    return redirect(id ? build(id, params) : fallback);
  };

/** Маршрутизатор приложения */
const router = createBrowserRouter([
  {
    path: '/',
    element: (
      <Suspense fallback={<Spinner sx={{ flex: 1 }} />}>
        <HomePage />
      </Suspense>
    ),
    errorElement: <ErrorPage />,
  },
  {
    // Страница обработки тихого входа открывается в скрытом iframe и не должна рендерить layout
    path: '/signin-silent-oidc',
    element: (
      <Suspense fallback={null}>
        <SignInSilentPage />
      </Suspense>
    ),
  },
  {
    element: <LayoutPage />,
    errorElement: <ErrorPage />,
    children: [
      { path: routes.catalog, element: <CatalogPage /> },
      { path: routes.playlists, element: <PlaylistsPage /> },
      { path: '/playlist/:id', element: <PlaylistPage /> },
      { path: '/search', element: <FilmSearchPage /> },
      { path: '/film/:id', element: <FilmPage /> },
      { path: routes.profile, element: <ProfilePage /> },
      { path: routes.rooms, element: <RoomsPage /> },
      { path: '/room/:id', element: <RoomPage /> },
      { path: routes.adminJobs, element: <AdminJobsPage /> },
      { path: '/signin-oidc', element: <SignInPage /> },
      { path: '/signout-oidc', element: <SignOutPage /> },

      // Перенаправления со старых адресов вида /film?id=X, /playlist?id=X и /room?id=X&code=Y
      { path: '/film', loader: legacyRedirect((id) => routes.film(id), routes.catalog) },
      { path: '/playlist', loader: legacyRedirect((id) => routes.playlist(id), routes.playlists) },
      {
        path: '/room',
        loader: legacyRedirect((id, params) => routes.room(id, params.get('code')), routes.rooms),
      },

      // Все остальные адреса — страница 404
      {
        path: '*',
        loader: () => {
          throw data('Страница не найдена', { status: 404 });
        },
      },
    ],
  },
]);

/**
 * Основной компонент приложения с маршрутизацией
 * @returns {ReactElement} JSX элемент, содержащий RouterProvider с маршрутизированными страницами
 */
const App = (): ReactElement => {
  return <RouterProvider router={router} />;
};

export default App;

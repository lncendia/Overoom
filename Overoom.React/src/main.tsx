import 'reflect-metadata';
import { Provider } from 'inversify-react';
import { createRoot } from 'react-dom/client';

import App from './App.tsx';
import './index.scss';
import createContainer from './container/inversify.config.ts';
import { AuthenticationContextProvider } from './contexts/authentication-context/AuthenticationContextProvider.tsx';
import { NotifyContextProvider } from './contexts/notify-context/NotifyContextProvider.tsx';
import { ThemeContextProvider } from './contexts/theme-context/ThemeContextProvider.tsx';

/**
 * Инициализация и рендеринг React приложения.
 * @returns {Promise<void>}
 */
const init = async (): Promise<void> => {
  const container = await createContainer();

  createRoot(document.getElementById('root')!).render(
    <Provider container={container}>
      <ThemeContextProvider>
        <NotifyContextProvider>
          <AuthenticationContextProvider>
            <App />
          </AuthenticationContextProvider>
        </NotifyContextProvider>
      </ThemeContextProvider>
    </Provider>
  );
};

init().then();

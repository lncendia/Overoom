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
  // Создаем контейнер зависимостей через Inversify
  const container = await createContainer();

  // Рендерим приложение с провайдерами контекста
  createRoot(document.getElementById('root')!).render(
    <Provider container={container}>
      {/* Провайдер темы приложения */}
      <ThemeContextProvider>
        {/* Провайдер уведомлений */}
        <NotifyContextProvider>
          {/* Провайдер аутентификации */}
          <AuthenticationContextProvider>
            {/* Основной компонент приложения */}
            <App />
          </AuthenticationContextProvider>
        </NotifyContextProvider>
      </ThemeContextProvider>
    </Provider>
  );
};

// Запускаем инициализацию приложения
init().then();

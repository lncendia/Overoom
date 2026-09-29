import 'reflect-metadata';
import { Provider } from 'inversify-react';
import { UserManager } from 'oidc-client-ts';
import { createRoot } from 'react-dom/client';

import App from './App.tsx';
import './index.scss';
import { Configuration } from './container/configuration.ts';
import createContainer from './container/inversify.config.ts';
import { AuthenticationContextProvider } from './contexts/authentication-context/AuthenticationContextProvider.tsx';
import { NotifyContextProvider } from './contexts/notify-context/NotifyContextProvider.tsx';
import { ThemeContextProvider } from './contexts/theme-context/ThemeContextProvider.tsx';

/** Корневой DOM-элемент приложения */
const rootElement = document.getElementById('root')!;

/**
 * Проверяет, открыта ли страница обработки тихого входа (внутри скрытого iframe)
 * @param config - Конфигурация приложения
 * @returns {boolean} true, если текущий адрес совпадает с silent_redirect_uri
 */
const isSilentCallback = (config: Configuration): boolean => {
  try {
    return window.location.pathname === new URL(config.oidc.silent_redirect_uri).pathname;
  } catch {
    return false;
  }
};

/**
 * Инициализация и рендеринг React приложения.
 * @returns {Promise<void>}
 */
const init = async (): Promise<void> => {
  const container = await createContainer();
  const config = container.get<Configuration>('Configuration');

  // В iframe тихого входа не рендерим приложение: достаточно передать ответ родительскому окну.
  // Отдельный UserManager без автообновления исключает вложенные попытки тихого входа.
  if (isSilentCallback(config)) {
    await new UserManager({ ...config.oidc, automaticSilentRenew: false }).signinSilentCallback();
    return;
  }

  createRoot(rootElement).render(
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

/**
 * Отображает сообщение об ошибке запуска приложения (например, если не удалось загрузить конфигурацию)
 */
const renderStartupError = (): void => {
  createRoot(rootElement).render(
    <div
      role="alert"
      style={{
        margin: 'auto',
        padding: '2rem',
        maxWidth: 480,
        textAlign: 'center',
        fontFamily: 'Roboto, sans-serif',
        color: '#365fa0',
      }}
    >
      <h2>Не удалось запустить приложение</h2>
      <p>Проверьте подключение к интернету и попробуйте обновить страницу.</p>
      <button type="button" onClick={() => window.location.reload()}>
        Обновить
      </button>
    </div>
  );
};

init().catch((error: unknown) => {
  console.error('Ошибка инициализации приложения', error);
  renderStartupError();
});

/** Интерфейс для конфигурации OIDC (OpenID Connect) */
export interface OidcConfig {
  /** URL авторизационного сервера */
  authority: string;

  /** Идентификатор клиента */
  client_id: string;

  /** URI для перенаправления после успешной аутентификации */
  redirect_uri: string;

  /** Тип ответа, ожидаемый от авторизационного сервера */
  response_type: string;

  /** Области доступа, запрашиваемые у авторизационного сервера */
  scope: string;

  /** URI для перенаправления после выхода из системы */
  post_logout_redirect_uri: string;

  /** Флаг для автоматического обновления токена в фоновом режиме */
  automaticSilentRenew: boolean;

  /** URI для перенаправления при автоматическом обновлении токена */
  silent_redirect_uri: string;
}

/** Интерфейс для конфигурации экземпляра Axios */
export interface ServiceConfig {
  /** Базовый URL для запросов */
  baseURL: string;
}

/** Интерфейс для конфигурации нескольких экземпляров Axios */
export interface ServicesConfig {
  /** Конфигурация для экземпляра Axios, используемого сервисом фильмов */
  films: ServiceConfig;

  /** Конфигурация для экземпляра Axios, используемого сервисом комнат */
  rooms: ServiceConfig;
}

/** Интерфейс для конфигурации файловых путей */
export interface Files {
  /** Префикс для миниатюр пользователей */
  userThumbnailPrefix: string;

  /** Префикс для миниатюр фильмов */
  filmThumbnailPrefix: string;

  /** Префикс для HLS потоков */
  hlsPrefix: string;
}

/** Основной интерфейс для конфигурации приложения */
export interface Configuration {
  /** Конфигурация OIDC */
  oidc: OidcConfig;

  /** Конфигурация нескольких экземпляров Axios */
  services: ServicesConfig;

  /** Конфигурация файлов */
  files: Files;
}

import { User, UserManager } from 'oidc-client-ts';

/** Состояние, передаваемое через процесс входа */
interface SignInState {
  /** Адрес страницы, на которую нужно вернуться после входа */
  returnUrl?: string;
}

/**
 * Проверяет, что адрес возврата является локальным путем приложения
 * @param url - Адрес для проверки
 * @returns {boolean} true, если адрес безопасен для перенаправления
 */
const isLocalUrl = (url: unknown): url is string =>
  typeof url === 'string' && url.startsWith('/') && !url.startsWith('//') && !url.startsWith('/\\');

/**
 * Класс для работы с аутентификацией через OpenID Connect
 */
export class AuthApi {
  /**
   * Менеджер пользователей для работы с OIDC-протоколом
   */
  private readonly userManager: UserManager;

  /**
   * Текущий запрос тихой аутентификации (для исключения параллельных запросов)
   */
  private silentRequest: Promise<User | null> | null = null;

  /**
   * Создает экземпляр AuthApi
   * @param userManager - менеджер пользователей для работы с OIDC
   */
  constructor(userManager: UserManager) {
    this.userManager = userManager;
  }

  /**
   * Получает access token текущего пользователя
   * @returns Promise с access token или null, если пользователь не аутентифицирован
   */
  public async getAccessToken(): Promise<string | null> {
    const user = await this.userManager.getUser();
    if (!user?.access_token) return null;
    return user.access_token;
  }

  /**
   * Получает id token текущего пользователя
   * @returns Promise с id token или null, если пользователь не аутентифицирован
   */
  public async getIdToken(): Promise<string | null> {
    const user = await this.userManager.getUser();
    if (!user?.id_token) return null;
    return user.id_token;
  }

  /**
   * Выполняет перенаправление на страницу аутентификации
   * @param returnUrl - Адрес, на который нужно вернуться после входа (по умолчанию текущая страница)
   * @returns Promise, который разрешается после инициации процесса аутентификации
   */
  public async signIn(returnUrl?: string): Promise<void> {
    const state: SignInState = {
      returnUrl: returnUrl ?? window.location.pathname + window.location.search,
    };
    await this.userManager.signinRedirect({ state });
  }

  /**
   * Выполняет тихую аутентификацию (без взаимодействия с пользователем)
   * @returns Promise, который разрешается после попытки тихой аутентификации
   */
  public async signInSilent(): Promise<void> {
    await this.userManager.signinSilent();
  }

  /**
   * Пытается выполнить тихую аутентификацию, не выбрасывая ошибок.
   * Параллельные вызовы используют один и тот же запрос.
   * @returns Promise с обновленным пользователем или null, если обновить сессию не удалось
   */
  public trySignInSilent(): Promise<User | null> {
    if (!this.silentRequest) {
      this.silentRequest = this.userManager
        .signinSilent()
        .catch(() => null)
        .finally(() => {
          this.silentRequest = null;
        });
    }
    return this.silentRequest;
  }

  /**
   * Обрабатывает перенаправление после успешной аутентификации
   * @returns Promise с адресом страницы, на которую нужно вернуться после входа
   */
  public async signInCallback(): Promise<string> {
    const user = await this.userManager.signinRedirectCallback();
    const returnUrl = (user.state as SignInState | undefined)?.returnUrl;
    return isLocalUrl(returnUrl) ? returnUrl : '/';
  }

  /**
   * Обрабатывает перенаправление после тихой аутентификации
   * @returns Promise, который разрешается после обработки callback
   */
  public async signInSilentCallback(): Promise<void> {
    await this.userManager.signinSilentCallback();
  }

  /**
   * Выполняет выход пользователя из системы
   * @returns Promise, который разрешается после инициации процесса выхода
   */
  public async signOut(): Promise<void> {
    const user = await this.userManager.getUser();
    await this.userManager.signoutRedirect({ id_token_hint: user?.id_token });
  }

  /**
   * Обрабатывает перенаправление после выхода из системы
   * @returns Promise, который разрешается после завершения процесса выхода
   */
  public async signOutCallback(): Promise<void> {
    try {
      await this.userManager.signoutRedirectCallback();
    } finally {
      await this.userManager.removeUser();
      await this.userManager.clearStaleState();
    }
  }
}

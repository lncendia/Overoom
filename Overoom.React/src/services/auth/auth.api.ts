// auth.api.ts
import { UserManager } from 'oidc-client';

/**
 * Класс для работы с аутентификацией через OpenID Connect
 */
export class AuthApi {
  /**
   * Менеджер пользователей для работы с OIDC-протоколом
   */
  private readonly userManager: UserManager;

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
   * @returns Promise, который разрешается после инициации процесса аутентификации
   */
  public async signIn(): Promise<void> {
    await this.userManager.signinRedirect();
  }

  /**
   * Выполняет тихую аутентификацию (без взаимодействия с пользователем)
   * @returns Promise, который разрешается после попытки тихой аутентификации
   */
  public async signInSilent(): Promise<void> {
    await this.userManager.signinSilent();
  }

  /**
   * Обрабатывает перенаправление после успешной аутентификации
   * @returns Promise, который разрешается после обработки callback
   */
  public async signInCallback(): Promise<void> {
    await this.userManager.signinRedirectCallback();
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
    await this.userManager.clearStaleState();
    await this.userManager.removeUser();
    await this.userManager.signoutRedirectCallback();
  }
}

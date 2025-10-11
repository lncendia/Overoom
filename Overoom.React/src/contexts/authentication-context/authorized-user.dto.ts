/** Интерфейс авторизованного пользователя */
export interface AuthorizedUserDto {
  /** Идентификатор пользователя */
  id: string;
  /** Имя пользователя */
  userName: string;
  /** URL аватара пользователя */
  photoUrl: string | null;
  /** Массив доступных ролей */
  roles: string[];
  /** Email пользователя */
  email: string;
  /** Локаль пользователя */
  locale: string;
}

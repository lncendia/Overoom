/** Данные актера/участника фильма */
export interface ActorResponse {
  /** Имя актера */
  name: string;
  /** Роль актера в фильме (опционально) */
  role: string | null;
}

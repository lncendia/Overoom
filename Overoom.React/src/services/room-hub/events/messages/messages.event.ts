import { CountResult } from '../../../common/count-result.ts';
import MessageResponse from '../../responses/message.response.ts';

/** Модель данных события получения истории сообщений */
export default interface MessagesEvent {
  /** Сообщения */
  messages: CountResult<MessageResponse>;
}

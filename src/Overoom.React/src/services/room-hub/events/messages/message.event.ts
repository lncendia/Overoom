import MessageResponse from '../../responses/message.response.ts';

/** Модель данных события нового сообщения в комнате */
export default interface MessageEvent {
  /** Данные сообщения */
  message: MessageResponse;
}

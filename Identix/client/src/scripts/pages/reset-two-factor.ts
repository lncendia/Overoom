import {InputWrapper} from "../helpers/input-wrapper";
import {EmailCodeHandler} from "../helpers/email-code-handler";

/** Класс функционала страницы сброса 2FA */
export class ResetTwoFactor {

    /** Метод запускает функционал страницы сброса 2FA */
    startResetTwoStep() {

        // получаем все поля ввода с классом .wrap-input input
        new InputWrapper('.wrap-input input');

        // создаем обработчик отправки кода по email
        new EmailCodeHandler('#request-email-link')
    }
}
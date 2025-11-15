import {InputWrapper} from "../helpers/input-wrapper";
import {EmailCodeHandler} from "../helpers/email-code-handler";

/** Класс функционала страницы входа 2FA */
export class LoginTwoStep {
    
    /** Метод запускает функционал страницы входа 2FA */
    startLoginTwoStep() {

        // получаем все поля ввода с классом .wrap-input input
        new InputWrapper('.wrap-input input');

        // создаем обработчик отправки кода по email
        new EmailCodeHandler('#request-email-link')
    }
}
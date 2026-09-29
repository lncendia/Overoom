import {InputWrapper} from "../helpers/input-wrapper";
import {EmailCodeHandler} from "../helpers/email-code-handler";

/** Класс функционала страницы входа 2FA */
export class LoginTwoStep {

    /** Метод запускает функционал страницы входа 2FA */
    startLoginTwoStep() {
        new InputWrapper('.wrap-input input');

        new EmailCodeHandler('#request-email-link')
    }
}
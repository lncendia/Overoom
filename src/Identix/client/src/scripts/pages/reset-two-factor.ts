import {InputWrapper} from "../helpers/input-wrapper";
import {EmailCodeHandler} from "../helpers/email-code-handler";

/** Класс функционала страницы сброса 2FA */
export class ResetTwoFactor {

    /** Метод запускает функционал страницы сброса 2FA */
    startResetTwoStep() {
        new InputWrapper('.wrap-input input');

        new EmailCodeHandler('#request-email-link')
    }
}
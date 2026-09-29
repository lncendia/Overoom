import {InputWrapper} from "../helpers/input-wrapper";
import {PasswordHide} from "../helpers/password-hide";

/** Класс функционала страницы авторизации */
export class Login {

    /** Метод запускает функционал страницы авторизации */
    startAccount() {
        new InputWrapper('.wrap-input input');
        new PasswordHide('.btn-show-pass');
    }
}
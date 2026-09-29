import {PasswordStrengthValidator} from "../helpers/password-strength-validator";
import {InputWrapper} from "../helpers/input-wrapper";
import {PasswordHide} from "../helpers/password-hide";

/** Класс функционала страницы регистрации */
export class Registration {

    /** Валидатор надежности пароля на странице */
    validator: PasswordStrengthValidator = new PasswordStrengthValidator(
        document.querySelector('#strength-valid'),
        document.querySelector('form#register'),
    )

    /** Метод запускает функционал страницы авторизации */
    startRegistration() {
        new InputWrapper('.wrap-input input');
        new PasswordHide('#show-pass');
        new PasswordHide('#show-pass-confirm');

        document.querySelector('#Password').addEventListener('input', ev =>
            this.validator.checkPasswordStrength((ev.currentTarget as HTMLInputElement).value)
        );

        document.querySelector('form#register').addEventListener('submit', ev => {
            ev.preventDefault();
            this.validator.validateFormPassword('Password');
        });
    }
}
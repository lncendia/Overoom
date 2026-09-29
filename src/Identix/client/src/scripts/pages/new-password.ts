import {InputWrapper} from "../helpers/input-wrapper";
import {PasswordHide} from "../helpers/password-hide";
import {PasswordStrengthValidator} from "../helpers/password-strength-validator";

/** Класс функционала страницы установки пароля */
export class NewPassword {

    /** Валидатор надежности пароля на странице */
    validator: PasswordStrengthValidator = new PasswordStrengthValidator(
        document.querySelector('#strength-valid') as HTMLDivElement,
        document.querySelector('#new-pass') as HTMLFormElement
    )

    /** Метод запускает функционал страницы установки нового пароля */
    startNewPassword() {
        new InputWrapper('.wrap-input input');
        new PasswordHide('#show-pass');
        new PasswordHide('#show-pass-confirm');

        document.querySelector('#NewPassword').addEventListener('input', ev =>
            this.validator.checkPasswordStrength((ev.currentTarget as HTMLInputElement).value)
        );

        document.querySelector('form#new-pass').addEventListener('submit', ev => {
            ev.preventDefault();
            this.validator.validateFormPassword('NewPassword')
        })
    }
}
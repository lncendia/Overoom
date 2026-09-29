import {PasswordStrengthValidator} from "../helpers/password-strength-validator";
import {InputWrapper} from "../helpers/input-wrapper";
import {PasswordHide} from "../helpers/password-hide";

/** Класс функционала страницы настроек */
export class Settings {

    /** Валидатор надежности пароля на странице */
    validator: PasswordStrengthValidator = new PasswordStrengthValidator(
        document.querySelector('#strength-valid') as HTMLDivElement,
        document.querySelector('form#change-pass') as HTMLFormElement
    );

    /** Метод запускает функционал страницы настроек */
    startSettings() {
        let links = document.querySelectorAll(".disabled");
        links.forEach(l => l.addEventListener("click", ev => ev.preventDefault()));
        const unlinkModal = document.getElementById('unlink-modal');

        unlinkModal.addEventListener('show.bs.modal', event => {
            const button = (event as MouseEvent).relatedTarget as HTMLElement;
            const provider = button.getAttribute('data-bs-provider');
            const providerName = button.getAttribute('data-bs-provider-name');
            const modalBody = unlinkModal.querySelector('.modal-body');
            modalBody.textContent = `${this.deleteLastWord(modalBody.textContent)} ${providerName}?`;

            const providerInput = unlinkModal.querySelector('input[name="Provider"]') as HTMLInputElement

            providerInput.value = provider;
        });

        new InputWrapper('.wrap-input input');

        const showPass = document.querySelector("#show-pass")

        if (showPass) new PasswordHide('#show-pass');

        const showOldPass = document.querySelector("#show-old-pass")

        if (showOldPass) new PasswordHide('#show-old-pass');

        new PasswordHide('#show-new-pass');
        new PasswordHide('#show-new-pass-confirm');

        document.querySelector('#NewPassword').addEventListener('input', ev =>
            this.validator.checkPasswordStrength((ev.currentTarget as HTMLInputElement).value)
        );

        document.querySelector('form#change-pass').addEventListener('submit', ev => {
            ev.preventDefault();
            this.validator.validateFormPassword('NewPassword');
        })
    }

    /**
     * Метод удаляет название провайдера, если оно указано
     */
    deleteLastWord(str: string): string {
        if (str.includes('?')) {
            const words = str.split(' ');
            words.pop();

            return words.join(' ');
        }

        return str;
    }
}
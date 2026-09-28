/** Класс проверяет пароль на надежность */
export class PasswordStrengthValidator {

    /** Блок элементов надежности пароля - ползунок прогресса и сообщения для пользователя */
    indicatorField: HTMLDivElement;

    /** Блок с ошибками */
    errorsBlock: HTMLSpanElement;

    /** Блок прогресс-бара */
    progress: HTMLDivElement;

    /** Элемент отправляемой формы */
    form: HTMLFormElement;

    /** Конструктор принимает блок с ползунком прогресса и сообщениями для пользователя и отпрвляемую форму */
    constructor(indicatorField: HTMLDivElement, form: HTMLFormElement) {
        this.indicatorField = indicatorField;

        this.errorsBlock = indicatorField.querySelector('.pass-valid-errors')

        this.progress = indicatorField.querySelector('.progress-bar')

        this.form = form;
    }

    /** Метод оценивает надежность пароля и возвращает оценку в виде числа (от 0 до 5) */
    private getPasswordStrength(password: string): number {
        password = password.replace(/\s{2,}/, ' ');
        let strength: number = 0;

        [/\p{Ll}+/u, /\p{Lu}+/u, /\p{N}+/u, /[^\p{L}\p{N}]+/u].forEach(el => {
            if (password.match(el)) strength++;
        });

        if (password.length >= 8) strength++;

        return strength;
    }

    /** Метод проверяет пароль на надежность перед отправкой формы */
    validateFormPassword(passwordFieldId: string) {
        let password = this.form.querySelector<HTMLInputElement>(`#${passwordFieldId}`).value;
        let passwordStrength = this.getPasswordStrength(password);

        if (passwordStrength === 5) {
            this.form.submit();
        }
        else {
            this.errorsBlock.innerHTML = this.indicatorField.querySelector('#invalid-pass').innerHTML;
            this.errorsBlock.style.color = "var(--bs-red)";
        }
    }

    /** Метод изменяет индикатор надежности в зависимости от пароля */
    checkPasswordStrength(password: string) {
        let progress: HTMLDivElement = this.indicatorField.querySelector('.progress-bar');

        if (password.length > 0) {
            this.indicatorField.removeAttribute('hidden');
            this.errorsBlock.style.color = "";
            let messageBlock: string;
            let style: string;
            let oldStyle: string = progress.classList.item(1);
            let strength = this.getPasswordStrength(password);

            if (strength < 3) {
                messageBlock = '#week-pass';
                style = 'bg-danger'
            }
            else if (strength >= 3 && strength <= 4) {
                messageBlock = '#medium-pass';
                style = 'bg-warning';
            }
            else {
                messageBlock = '#strong-pass';
                style = 'bg-success';
            }

            this.errorsBlock.innerHTML = this.indicatorField.querySelector(messageBlock).innerHTML;
            progress.setAttribute('aria-valuenow', String(strength * 20));
            progress.classList.replace(oldStyle, style);
            progress.style.width = `${strength * 20}%`;
        }

        else {
            this.indicatorField.setAttribute('hidden', 'hidden');
        }
    }
}
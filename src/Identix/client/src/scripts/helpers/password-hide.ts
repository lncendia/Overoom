/** Класс добавляет функциональность для просмотра введенного пароля */
export class PasswordHide {

    /** Иконка переключателя */
    private show: HTMLElement

    /** Иконка переключателя */
    private hide: HTMLElement

    /** Элемент ввода */
    private input: Element

    /** Конструктор */
    constructor(selector: string) {
        let showPass: boolean = false;
        let container = document.querySelector(selector);
        this.input = container.nextElementSibling;
        this.show = container.querySelector('[password="show"]');
        this.hide = container.querySelector('[password="hide"]');

        container.addEventListener('click', () => {
            this.togglePassword(showPass);
            showPass = !showPass;
        });

    }

    /** Метод переключает видимость пароля */
    togglePassword(showPass: boolean) {
        if (!showPass) {
            this.show.style.display = "none";
            this.hide.style.display = "block";
            this.input.setAttribute('type', 'text');
        } else {
            this.show.style.display = "block";
            this.hide.style.display = "none";
            this.input.setAttribute('type', 'password');
        }
    }
}
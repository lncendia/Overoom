/** Класс делает input функциональными */
export class InputWrapper {

    /** Конструктор */
    constructor(selector: string) {
        document.querySelectorAll(selector).forEach(element => {
            this.blur(element as HTMLInputElement);
            element.addEventListener('blur', ev => this.blur((ev.currentTarget as HTMLInputElement)));
        });
    }

    /** Метод реагирует на потерю фокуса */
    blur(element: HTMLInputElement) {
        if (element.value.trim() != "") {
            element.classList.add('has-val');
        } else {
            element.classList.remove('has-val');
        }
    }
}
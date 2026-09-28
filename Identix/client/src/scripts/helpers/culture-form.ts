/** Класс функционала формы языка текущей страницы */
export class CultureForm {

    /** Метод запускает функционал формы языка текущей страницы */
    startCultureForm() {
        const form: HTMLFormElement = document.querySelector('.form-culture') as HTMLFormElement;

        form.querySelector('select').addEventListener('change', () => {
            form.submit();
        });
    }
}
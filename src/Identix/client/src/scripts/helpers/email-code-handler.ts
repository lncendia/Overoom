/** Класс добавляет функциональность запроса кода 2FA на почту */
export class EmailCodeHandler {

    /** Конструктор */
    constructor(selector: string) {
        const link = document.querySelector(selector) as HTMLLinkElement;
        const requestLink = link.href;

        // удаляем путь из атрибута href, чтобы не срабатывал переход по нажатию
        link.href = '#';

        // удаляем путь из атрибута href, чтобы не срабатывал переход по нажатию
        (document.querySelector('#request-email-link') as HTMLLinkElement).href = '#';

        link.addEventListener('click', () => {
            this.processRequest(link, requestLink);
        });
    }

    /** Оработчик запроса */
    async processRequest(link: HTMLLinkElement, requestLink: string) {
        if (link.classList.contains('disabled')) {
            return;
        }

        let response = await fetch(requestLink);
        let error = document.querySelector('#request-email-error') as HTMLDivElement;

        link.innerHTML = document.querySelector('#resending-msg').innerHTML

        if (response.ok) {
            const duration: number = 60;
            let success = document.querySelector('#request-email-success') as HTMLDivElement;
            let timer = success.querySelector('span') as HTMLSpanElement;
            link.classList.add('disabled');

            error.setAttribute('hidden', 'hidden')

            success.removeAttribute('hidden');

            this.startTimer(duration, timer)

            setTimeout(() => {
                link.classList.remove('disabled');
                link.removeAttribute('style');
                success.setAttribute('hidden', 'hidden');
            }, duration * 1000);
        } else {
            error.removeAttribute('hidden');
        }
    }

    /** Метод отображающий таймер на странице */
    startTimer(duration: number, displayElement: HTMLElement) {
        let timerId = setInterval(() => {
            const minutes = Math.floor(duration / 60);
            const seconds = Math.floor(duration % 60);
            const minutesStr = minutes < 10 ? "0" + minutes : minutes;
            const secondsStr = seconds < 10 ? "0" + seconds : seconds;
            displayElement.innerHTML = minutesStr + ":" + secondsStr;
            duration--;
        }, 1000);

        setTimeout(() => {
            clearInterval(timerId);
        }, duration * 1000);
    }
}
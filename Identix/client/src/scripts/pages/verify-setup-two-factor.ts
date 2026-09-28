/** Класс функционала страницы с кодами восстановления */
export class VerifySetupTwoFactor {

    /** Метод запускает функционал страницы с кодами восстановления */
    startVerifySetup() {
        const copyButton = document.querySelector('.copy-codes');
        copyButton.addEventListener('click', this.copyRecoveryCodes.bind(this));

    }

    /** Метод для копирования текста из элементов, содержащие коды */
    copyRecoveryCodes() {
        const recoveryCodeElements = document.querySelectorAll('.codes');
        let recoveryCodesToCopy = '';

        recoveryCodeElements.forEach(element => {
            recoveryCodesToCopy += element.textContent?.trim() + '\n';
        });

        navigator.clipboard.writeText(recoveryCodesToCopy).then();
    }
}
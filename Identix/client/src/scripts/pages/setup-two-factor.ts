import QRCode from "qrcode";
import {InputWrapper} from "../helpers/input-wrapper";

/** Класс функционала страницы установки 2FA */
export class SetupTwoFactor {

    /** Метод запускает функционал страницы установки 2FA */
    startSetupTwoFactor() {
        new InputWrapper('.wrap-input input');
        const authKey = document.querySelector('.auth-key');
        authKey.addEventListener('click', ev => this.copyCode((ev.currentTarget as HTMLDivElement)));

        const qrCanvas = document.getElementById('qrCode');

        const qrData = qrCanvas.getAttribute("qr-data");

        QRCode.toCanvas(qrCanvas, qrData).then()
    }

    /**
     * Метод для копирования текста из элемента
     */
    copyCode(element: HTMLDivElement) {
        const textToCopy = element.textContent?.trim();

        if (textToCopy) {
            navigator.clipboard.writeText(textToCopy).then();
        }
    }
}
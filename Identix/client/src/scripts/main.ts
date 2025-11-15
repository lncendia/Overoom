import '../styles/index.scss';
import 'bootstrap'
import 'jquery-validation-unobtrusive'


import {ResetTwoFactor} from "./pages/reset-two-factor";
import {LoginTwoStep} from "./pages/login-two-step";
import {VerifySetupTwoFactor} from "./pages/verify-setup-two-factor";
import {Login} from "./pages/login";
import {NewPassword} from "./pages/new-password";
import {RecoverPassword} from "./pages/recover-password";
import {Registration} from "./pages/registration";
import {Settings} from "./pages/settings";
import {SetupTwoFactor} from "./pages/setup-two-factor";
import {CultureForm} from "./helpers/culture-form";
import {ThemeToggler} from "./helpers/theme-toggler";

window.addEventListener("load", () => {

    // получаем текущий URL
    const currentUrl = new URL(document.location.href);

    // получаем путь из URL
    const pathname = currentUrl.pathname.toLowerCase();

    // разбиваем пути URL на части
    const partsPath = pathname.split("/");

    // Создаем класс формы языка текущей страницы
    const cultureForm = new CultureForm();

    // Запускаем функционал формы языка текущей страницы
    cultureForm.startCultureForm();

    // Создаем класс переключателя темы
    const themeToggler = new ThemeToggler()

    // Запускаем функционал переключателя темы
    themeToggler.startThemeToggler()

    // Смотрим имя контроллера
    switch (partsPath[1]) {

        // Контроллер Account
        case "account": {
            switch (partsPath[2]) {
                case "login": {
                    const login = new Login();
                    login.startAccount();
                    break;
                }
                case "newpassword": {
                    const password = new NewPassword();
                    password.startNewPassword()
                    break;
                }
                case "recoverpassword": {
                    const password = new RecoverPassword();
                    password.startRecoverPassword()
                    break;
                }
            }
            break;
        }

        // Контроллер Registration
        case "registration": {
            switch (partsPath[2]) {
                case "registration": {
                    const registration = new Registration();
                    registration.startRegistration();
                    break;
                }
            }
            break;
        }

        // Контроллер Settings
        case "settings": {
            switch (partsPath[2]) {
                default: {
                    const settings = new Settings();
                    settings.startSettings();
                    break;
                }
            }
            break;
        }

        // Контроллер TwoFactor
        case "twofactor": {
            switch (partsPath[2]) {
                case "setup": {
                    const setupTwoFactor = new SetupTwoFactor()
                    setupTwoFactor.startSetupTwoFactor()
                    break;
                }
                case "logintwostep": {
                    const loginTwoStep = new LoginTwoStep()
                    loginTwoStep.startLoginTwoStep();
                    break;
                }
                case "reset": {
                    const resetTwoFactor = new ResetTwoFactor()
                    resetTwoFactor.startResetTwoStep();
                    break;
                }
                case "verifysetup": {
                    const verifySetup = new VerifySetupTwoFactor();
                    verifySetup.startVerifySetup();
                    break;
                }
            }
            break;
        }
    }
});
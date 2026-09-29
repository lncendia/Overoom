/** Класс добавляет функциональность для смены темы */
export class ThemeToggler {

    /** Переключатель на светлую тему */
    private light: HTMLElement

    /** Переключатель на темную тему */
    private dark: HTMLElement

    /** Метод запускает функционал переключения темы */
    startThemeToggler() {
        const toggler = document.querySelector(".theme-toggler");
        this.light = toggler.querySelector("#light");
        this.dark = toggler.querySelector("#dark");
        toggler.addEventListener("click", () => this.toggleTheme());
    }

    /** Метод переключает тему */
    toggleTheme() {
        let theme = window.localStorage.getItem("theme");
        if (!theme || theme === "light") this.setTheme("dark");

        else this.setTheme("light");
    }

    /** Метод устанавливает тему */
    setTheme(theme: string) {
        document.documentElement.setAttribute("data-bs-theme", theme);
        window.localStorage.setItem("theme", theme);

        if (theme === "light") {
            this.light.style.display = "none";
            this.dark.style.display = "block";
        } else if (theme === "dark") {
            this.light.style.display = "block";
            this.dark.style.display = "none";
        }
    }
}
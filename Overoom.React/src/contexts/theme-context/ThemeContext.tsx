import { PaletteMode } from '@mui/material';
import { createContext } from 'react';

/** Интерфейс контекста темы */
export interface ThemeContextType {
  /**
   * Метод установки темы приложения
   * @param mode - тип мода
   */
  setMode: (mode: PaletteMode) => void;
}

/** Контекст комнаты с undefined в качестве значения по умолчанию */
export const ThemeContext = createContext<ThemeContextType>({
  // Устанавливаем темную тему
  setMode: () => {},
});

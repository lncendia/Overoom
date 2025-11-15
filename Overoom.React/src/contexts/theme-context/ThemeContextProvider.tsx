import { PaletteMode, ThemeProvider, createTheme, alpha, Shadows } from '@mui/material';
import React, { ReactElement, ReactNode, useEffect, useMemo, useState } from 'react';

import { ThemeContext } from './ThemeContext.tsx';

/** Пропсы провайдера темы */
interface ThemeContextProviderProps {
  /** Дочерние элементы */
  children: ReactNode;
}

/**
 * Провайдер контекста темы для приложения
 * @param props - свойства компонента
 * @param props.children - дочерние элементы
 * @returns {ReactElement} JSX элемент провайдера темы
 */
export const ThemeContextProvider: React.FC<ThemeContextProviderProps> = ({
  children,
}): ReactElement => {
  /** Получаем текущий режим темы ('light' или 'dark') и функцию для его изменения */
  const [mode, setMode] = usePreferredMode('light');

  /** Генерация объекта темы Material-UI на основе выбранного режима */
  const theme = useMemo(() => {
    // Определяем базовые цвета для light и dark
    const palette = {
      dark: {
        primary: 'rgb(118,70,255)',
        secondary: 'rgb(195,149,216)',
        paperBg: 'rgba(30,30,46,0.5)',
        textPrimary: 'rgb(241,234,253)',
        textSecondary: 'rgb(206,199,220)',
        textDisabled: 'rgb(112,109,120)',
      },
      light: {
        primary: 'rgb(53,112,255)',
        secondary: 'rgb(128,97,128)',
        paperBg: 'rgba(32,32,48,0.5)',
        textPrimary: 'rgb(241,234,253)',
        textSecondary: 'rgb(206,199,220)',
        textDisabled: 'rgb(157,152,168)',
      },
    }[mode];

    /** Кастомная система теней на основе основного цвета темы. */
    const shadows: Shadows = [
      'none',
      `0px 2px 1px -1px ${alpha(palette.primary, 0.2)}, 0px 1px 1px 0px ${alpha(palette.primary, 0.14)}, 0px 1px 3px 0px ${alpha(palette.primary, 0.12)}`,
      `0px 3px 1px -2px ${alpha(palette.primary, 0.2)}, 0px 2px 2px 0px ${alpha(palette.primary, 0.14)}, 0px 1px 5px 0px ${alpha(palette.primary, 0.12)}`,
      `0px 3px 3px -2px ${alpha(palette.primary, 0.2)}, 0px 3px 4px 0px ${alpha(palette.primary, 0.14)}, 0px 1px 8px 0px ${alpha(palette.primary, 0.12)}`,
      `0px 2px 4px -1px ${alpha(palette.primary, 0.2)}, 0px 4px 5px 0px ${alpha(palette.primary, 0.14)}, 0px 1px 10px 0px ${alpha(palette.primary, 0.12)}`,
      `0px 3px 5px -1px ${alpha(palette.primary, 0.2)}, 0px 5px 8px 0px ${alpha(palette.primary, 0.14)}, 0px 1px 14px 0px ${alpha(palette.primary, 0.12)}`,
      `0px 3px 5px -1px ${alpha(palette.primary, 0.2)}, 0px 6px 10px 0px ${alpha(palette.primary, 0.14)}, 0px 1px 18px 0px ${alpha(palette.primary, 0.12)}`,
      `0px 4px 5px -2px ${alpha(palette.primary, 0.2)}, 0px 7px 10px 1px ${alpha(palette.primary, 0.14)}, 0px 2px 16px 1px ${alpha(palette.primary, 0.12)}`,
      `0px 5px 5px -3px ${alpha(palette.primary, 0.2)}, 0px 8px 10px 1px ${alpha(palette.primary, 0.14)}, 0px 3px 14px 2px ${alpha(palette.primary, 0.12)}`,
      `0px 5px 6px -3px ${alpha(palette.primary, 0.2)}, 0px 9px 12px 1px ${alpha(palette.primary, 0.14)}, 0px 3px 16px 2px ${alpha(palette.primary, 0.12)}`,
      `0px 6px 6px -3px ${alpha(palette.primary, 0.2)}, 0px 10px 14px 1px ${alpha(palette.primary, 0.14)}, 0px 4px 18px 3px ${alpha(palette.primary, 0.12)}`,
      `0px 6px 7px -4px ${alpha(palette.primary, 0.2)}, 0px 11px 15px 1px ${alpha(palette.primary, 0.14)}, 0px 4px 20px 3px ${alpha(palette.primary, 0.12)}`,
      `0px 7px 8px -4px ${alpha(palette.primary, 0.2)}, 0px 12px 17px 2px ${alpha(palette.primary, 0.14)}, 0px 5px 22px 4px ${alpha(palette.primary, 0.12)}`,
      `0px 7px 8px -4px ${alpha(palette.primary, 0.2)}, 0px 13px 19px 2px ${alpha(palette.primary, 0.14)}, 0px 5px 24px 4px ${alpha(palette.primary, 0.12)}`,
      `0px 7px 9px -4px ${alpha(palette.primary, 0.2)}, 0px 14px 21px 2px ${alpha(palette.primary, 0.14)}, 0px 5px 26px 4px ${alpha(palette.primary, 0.12)}`,
      `0px 8px 9px -5px ${alpha(palette.primary, 0.2)}, 0px 15px 22px 2px ${alpha(palette.primary, 0.14)}, 0px 6px 28px 5px ${alpha(palette.primary, 0.12)}`,
      `0px 8px 10px -5px ${alpha(palette.primary, 0.2)}, 0px 16px 24px 2px ${alpha(palette.primary, 0.14)}, 0px 6px 30px 5px ${alpha(palette.primary, 0.12)}`,
      `0px 8px 11px -5px ${alpha(palette.primary, 0.2)}, 0px 17px 26px 2px ${alpha(palette.primary, 0.14)}, 0px 6px 32px 5px ${alpha(palette.primary, 0.12)}`,
      `0px 9px 11px -5px ${alpha(palette.primary, 0.2)}, 0px 18px 28px 2px ${alpha(palette.primary, 0.14)}, 0px 7px 34px 6px ${alpha(palette.primary, 0.12)}`,
      `0px 9px 12px -6px ${alpha(palette.primary, 0.2)}, 0px 19px 29px 2px ${alpha(palette.primary, 0.14)}, 0px 7px 36px 6px ${alpha(palette.primary, 0.12)}`,
      `0px 10px 13px -6px ${alpha(palette.primary, 0.2)}, 0px 20px 31px 3px ${alpha(palette.primary, 0.14)}, 0px 8px 38px 7px ${alpha(palette.primary, 0.12)}`,
      `0px 10px 13px -6px ${alpha(palette.primary, 0.2)}, 0px 21px 33px 3px ${alpha(palette.primary, 0.14)}, 0px 8px 40px 7px ${alpha(palette.primary, 0.12)}`,
      `0px 10px 14px -6px ${alpha(palette.primary, 0.2)}, 0px 22px 35px 3px ${alpha(palette.primary, 0.14)}, 0px 8px 42px 7px ${alpha(palette.primary, 0.12)}`,
      `0px 11px 14px -7px ${alpha(palette.primary, 0.2)}, 0px 23px 36px 3px ${alpha(palette.primary, 0.14)}, 0px 9px 44px 8px ${alpha(palette.primary, 0.12)}`,
      `0px 11px 15px -7px ${alpha(palette.primary, 0.2)}, 0px 24px 38px 3px ${alpha(palette.primary, 0.14)}, 0px 9px 46px 8px ${alpha(palette.primary, 0.12)}`,
    ];

    return createTheme({
      palette: {
        mode,
        primary: { main: palette.primary },
        secondary: { main: palette.secondary },
        background: { default: alpha(palette.paperBg, 1), paper: palette.paperBg },
        text: {
          primary: palette.textPrimary,
          secondary: palette.textSecondary,
          disabled: palette.textDisabled,
        },
      },
      shadows,
      shape: { borderRadius: '8px' },
      components: {
        /** Кастомизация контейнера с градиентным фоном */
        MuiContainer: {
          styleOverrides: {
            root: {
              '&.background-container': {
                flex: 1,
                paddingTop: '1.5rem',
                paddingBottom: '2rem',
                backgroundBlendMode: 'screen',
                backgroundSize: 'cover',
                background:
                  mode === 'dark'
                    ? 'linear-gradient(#1c0f2a, transparent), linear-gradient(90deg, #081f26, transparent), linear-gradient(-90deg, #261107, transparent);'
                    : 'linear-gradient(#2f676f, transparent), linear-gradient(90deg, #86439a, transparent), linear-gradient(-90deg, #875534, transparent);',
              },
            },
          },
        },
        /** Кастомизация AppBar с градиентным фоном */
        MuiAppBar: {
          styleOverrides: {
            root: {
              padding: 0,
              color: palette.textPrimary,
              background:
                mode === 'dark'
                  ? 'linear-gradient(0.4turn, #101f40, #321a36)'
                  : 'linear-gradient(0.4turn, #5b2f7d, #3e7a83)',
            },
          },
        },
        /** Кастомизация Paper компонентов с анимированными тенями */
        MuiPaper: {
          styleOverrides: {
            root: {
              padding: '1.5rem',
              marginBottom: '1.5rem',
              boxShadow: shadows[2],
              backgroundImage: 'none',
              transition: 'all 0.3s ease',
              '& .MuiPaper-header': { boxShadow: 'none' },
            },
          },
        },
        /** Кастомизация пунктов меню с hover-эффектами */
        MuiMenuItem: {
          styleOverrides: {
            root: ({ theme }) => ({
              backgroundColor: alpha(theme.palette.background.paper, 1),
              '&:hover': {
                backgroundColor: theme.palette.secondary.light,
                color: theme.palette.secondary.contrastText,
              },
              '&.Mui-selected': {
                backgroundColor: theme.palette.secondary.dark,
                color: theme.palette.secondary.contrastText,
                '&:hover': { backgroundColor: theme.palette.secondary.main },
              },
            }),
          },
        },
        /** Кастомизация списков */
        MuiList: {
          styleOverrides: { root: { paddingTop: 0, paddingBottom: 0 } },
        },
        MuiCard: {
          styleOverrides: { root: { padding: 0, marginBottom: 0 } },
        },
        MuiDrawer: {
          styleOverrides: {
            root: {
              '.MuiDrawer-paper': {
                padding: 0,
              },
            },
          },
        },
        /** Кастомизация Paper компонентов с анимированными тенями */
        MuiPopover: {
          styleOverrides: {
            root: {
              '.MuiPopover-paper': {
                padding: 0,
              },
            },
          },
        },
        MuiDialog: {
          styleOverrides: {
            root: {
              '.MuiDialog-paper': {
                padding: 0,
              },
            },
          },
        },
        MuiAlert: {
          styleOverrides: {
            root: {
              marginBottom: 0,
            },
          },
        },
      },
    });
  }, [mode]);

  return (
    <ThemeContext.Provider value={{ setMode }}>
      <ThemeProvider theme={theme}>{children}</ThemeProvider>
    </ThemeContext.Provider>
  );
};

/**
 * Хук для получения и сохранения предпочтительного режима темы ('light' или 'dark')
 * @param defaultMode - режим по умолчанию, если нет сохранённого значения и системного предпочтения (по умолчанию 'light')
 * @returns - текущий режим и функция для его изменения
 */
const usePreferredMode = (
  defaultMode: PaletteMode = 'light'
): [PaletteMode, (mode: PaletteMode) => void] => {
  /**
   * Получает начальный режим: сначала из localStorage, затем из системных настроек
   * @returns {PaletteMode} 'light' или 'dark'
   */
  const getInitialMode = (): PaletteMode => {
    // Читаем сохранённый режим из localStorage
    const saved = localStorage.getItem('themeMode') as PaletteMode | null;
    if (saved === 'light' || saved === 'dark') return saved;

    // Проверяем системное предпочтение
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    return prefersDark ? 'dark' : defaultMode;
  };

  /** Состояние текущего режима темы */
  const [mode, setMode] = useState<PaletteMode>(getInitialMode);

  // Сохраняем выбор пользователя в localStorage при изменении режима
  useEffect(() => {
    localStorage.setItem('themeMode', mode);
  }, [mode]);

  /** Возвращаем текущий режим и функцию для его изменения */
  return [mode, setMode];
};

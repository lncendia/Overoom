import React, { useState, useRef, useCallback, useEffect, ChangeEvent } from 'react';

/** Интерфейс для пропсов, которые будут передаваться в HOC. */
interface WithDelayedInputProps {
  /** Текущее значение поля ввода (опциональное). */
  value?: string;

  /**
   * Обработчик события изменения значения поля ввода.
   * @param event - событие изменения значения поля ввода.
   * @returns {Promise<void> | void}`.
   */
  onChange?: (event: ChangeEvent<HTMLInputElement>) => Promise<void> | void;
}

/**
 * HOC для добавления задержки ввода.
 * @template P - Тип пропсов оборачиваемого компонента.
 * @param WrappedComponent - Компонент, который нужно обернуть.
 * @returns Новый компонент с добавленной логикой задержки ввода.
 */
const withDelayedInput = <P extends object>(WrappedComponent: React.ComponentType<P>) => {
  /**
   * Возвращаемый компонент с логикой задержки ввода.
   * @param props - Пропсы, переданные в компонент.
   * @returns JSX-элемент обернутого компонента.
   */
  return (props: P & WithDelayedInputProps) => {
    /** Текущее значение поля ввода. */
    const [currentValue, setCurrentValue] = useState(props.value ?? '');

    /** Референс для хранения идентификатора таймаута. */
    const timeoutIdRef = useRef<NodeJS.Timeout | null>(null);

    /**
     * Обработчик изменения значения поля ввода.
     * @param e - Событие изменения значения поля ввода.
     */
    const onChange = useCallback(
      (e: React.ChangeEvent<HTMLInputElement>) => {
        setCurrentValue(e.target.value);

        if (timeoutIdRef.current) {
          clearTimeout(timeoutIdRef.current);
        }

        timeoutIdRef.current = setTimeout(() => {
          if (props.onChange) props.onChange(e);
        }, 500);
      },
      [props]
    );

    /** Эффект для очистки таймаута при размонтировании компонента. */
    useEffect(() => {
      return () => {
        if (timeoutIdRef.current) {
          clearTimeout(timeoutIdRef.current);
        }
      };
    }, []);

    return (
      <WrappedComponent
        {...props} // Передаем все оригинальные пропсы.
        value={currentValue} // Передаем текущее значение поля ввода.
        onChange={onChange} // Передаем наш обработчик onChange.
      />
    );
  };
};

export default withDelayedInput;

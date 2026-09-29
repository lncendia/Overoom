import React, { useState, useRef, useCallback, useEffect, ChangeEvent, ReactElement } from 'react';

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
   * @returns {ReactElement} JSX-элемент обернутого компонента.
   */
  const DelayedInput = (props: P & WithDelayedInputProps): ReactElement => {
    /** Текущее значение поля ввода. */
    const [currentValue, setCurrentValue] = useState(props.value ?? '');

    /** Последнее значение, полученное через пропсы (для синхронизации при внешнем изменении). */
    const [prevPropValue, setPrevPropValue] = useState(props.value);

    // Синхронизируем внутреннее значение, если значение изменили снаружи
    if (props.value !== prevPropValue) {
      setPrevPropValue(props.value);
      setCurrentValue(props.value ?? '');
    }

    /** Референс для хранения идентификатора таймаута. */
    const timeoutIdRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    /** Референс на актуальный обработчик изменения из пропсов. */
    const onChangeRef = useRef(props.onChange);

    useEffect(() => {
      onChangeRef.current = props.onChange;
    }, [props.onChange]);

    /**
     * Обработчик изменения значения поля ввода.
     * @param e - Событие изменения значения поля ввода.
     */
    const onChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
      setCurrentValue(e.target.value);

      if (timeoutIdRef.current) {
        clearTimeout(timeoutIdRef.current);
      }

      timeoutIdRef.current = setTimeout(() => {
        onChangeRef.current?.(e);
      }, 500);
    }, []);

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

  return DelayedInput;
};

export default withDelayedInput;

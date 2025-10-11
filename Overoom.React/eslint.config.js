import js from '@eslint/js';
import globals from 'globals';
import reactHooks from 'eslint-plugin-react-hooks';
import reactRefresh from 'eslint-plugin-react-refresh';
import tseslint from 'typescript-eslint';
import prettier from 'eslint-plugin-prettier';
import jsdoc from 'eslint-plugin-jsdoc';
import imports from 'eslint-plugin-import';

export default tseslint.config(
  { ignores: ['dist'] },
  {
    extends: [js.configs.recommended, ...tseslint.configs.recommended],
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2020,
      globals: globals.browser,
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
      import: imports,
      prettier,
      jsdoc,
    },
    rules: {
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': ['warn', { additionalHooks: '(useSafeCallback)' }],
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
      'jsdoc/check-access': 1,
      'jsdoc/check-alignment': 1,
      'jsdoc/check-param-names': 1,
      'jsdoc/check-property-names': 1,
      'jsdoc/check-tag-names': 1,
      'jsdoc/check-types': 1,
      'jsdoc/no-undefined-types': 1,
      'jsdoc/require-description': 1,
      'jsdoc/require-jsdoc': 1,
      'jsdoc/require-param': 1,
      'jsdoc/require-param-description': 1,
      'jsdoc/require-param-name': 1,
      'jsdoc/require-returns': 1,
      'jsdoc/require-returns-description': 1,
      'jsdoc/require-returns-check': 1,
      'jsdoc/valid-types': 1,
      'import/order': [
        'error',
        {
          groups: ['builtin', 'external', 'internal', ['parent', 'sibling', 'index']],
          'newlines-between': 'always',
          alphabetize: { order: 'asc', caseInsensitive: true },
        },
      ],
      'import/no-duplicates': 'error',
      'import/newline-after-import': 'error',
      'import/no-unresolved': 'error',
      'import/first': 'error',
      'import/no-cycle': 'warn',
      'prettier/prettier': 'error',
    },
  }
);

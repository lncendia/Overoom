const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require("terser-webpack-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");

// Конфигурация webpack
module.exports = {

    // Тип сборки
    mode: "production",
    
    // Точка входа
    entry: {
        
        // Имя выходного файла и путь к входному файлу
        app: './src/scripts/main.ts',
    },

    // Параметры выходного бандла
    output: {

        // Название бандла
        filename: '[name].bundle.js',

        // Путь к бандлу
        path: path.resolve(__dirname, '../wwwroot/bundles'),

        // Флаг, нужно ли очищать дирректорию с бандлами перед сборкой
        clean: true,
    },

    // Разбор модулей
    module: {
        rules: [
            {
                // Файлы ts
                test: /\.ts$/,
                use: 'ts-loader',
                exclude: /node_modules/
            },
            {
                // Файлы scss и css
                test: /\.(s[ac]|c)ss$/i,
                use: [
                    // Плагин для вынесения css в отдельный бандл
                    MiniCssExtractPlugin.loader,
                    'css-loader',
                    'postcss-loader',
                    'sass-loader'
                ],
            },
            {
                // Файлы шрифтов
                test: /\.(woff|woff2|eot|ttf|otf)$/i,
                type: 'asset/resource',
                generator: {

                    // Дирректория где будут храниться шрифты
                    filename: 'fonts/[name][ext]'
                }
            },
        ]
    },

    // Разбираемые расширения файлов
    resolve: {
        extensions: ['.ts', '.js']
    },

    // Подключенные плагины
    plugins: [

        // Плагин для сборки бандла со стилями
        new MiniCssExtractPlugin({

            // Имя бандла
            filename: '[name].bundle.css',
        }),
    ],

    // Оптимизация сборки
    optimization: {

        // Оптимизируем только на продакшн
        minimize: true,

        // Перечень минимизаторов
        minimizer: [

            // Минимизатор стилей (scss, css)
            new CssMinimizerPlugin(),

            // Минимизатор js бандла
            new TerserPlugin({
                extractComments: false,
                minify: TerserPlugin.swcMinify,
                terserOptions: {
                    compress: {

                        // Настройка, убирает все выводы в консоль (console.log(...)) из конечного бандла
                        drop_console: true
                    }
                }
            })
        ]
    },

    // Настройки source maps
    devtool: 'source-map'
};

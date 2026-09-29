const path = require('path');
const MiniCssExtractPlugin = require('mini-css-extract-plugin');
const TerserPlugin = require("terser-webpack-plugin");
const CssMinimizerPlugin = require("css-minimizer-webpack-plugin");

module.exports = {
    mode: "production",

    entry: {
        app: './src/scripts/main.ts',
    },

    output: {
        filename: '[name].bundle.js',

        path: path.resolve(__dirname, '../wwwroot/bundles'),

        clean: true,
    },

    module: {
        rules: [
            {
                // TypeScript только транспилируется, проверку типов выполняет tsc в скрипте build
                test: /\.ts$/,
                use: {
                    loader: 'swc-loader',
                    options: {
                        sourceMaps: true,
                        jsc: {
                            parser: { syntax: 'typescript' },
                            target: 'es2020'
                        }
                    }
                },
                exclude: /node_modules/
            },
            {
                test: /\.(s[ac]|c)ss$/i,
                use: [
                    MiniCssExtractPlugin.loader,
                    'css-loader',
                    'postcss-loader',
                    'sass-loader'
                ],
            },
            {
                test: /\.(woff|woff2|eot|ttf|otf)$/i,
                type: 'asset/resource',
                generator: {
                    filename: 'fonts/[name][ext]'
                }
            },
        ]
    },

    resolve: {
        extensions: ['.ts', '.js']
    },

    plugins: [
        new MiniCssExtractPlugin({
            filename: '[name].bundle.css',
        }),
    ],

    optimization: {
        minimize: true,

        minimizer: [
            new CssMinimizerPlugin(),

            new TerserPlugin({
                extractComments: false,
                minify: TerserPlugin.swcMinify,
                terserOptions: {
                    compress: {
                        drop_console: true
                    }
                }
            })
        ]
    },

    devtool: 'source-map'
};

/// <binding BeforeBuild='all' ProjectOpened='watch' />
/**
 * Запускает задачу наблюдения при открытии проекта, все задачи перед 
 * билдом (выбирается в меню Task Runner)
 * @param grunt
 */

module.exports = function (grunt) {

    const sass = require('node-sass');
    
    grunt.initConfig({
        
        /** очистка файлов какие папки/файлы очищать */
        clean: ["wwwroot/css/*", "wwwroot/js/app.min.js", "ScriptsAndCss/Combined/*"], 
        concat: {
            //объединение JS
            js: {
                //сюда можно писать файлы для объединения через запятую
                src: [
                    "ScriptsAndCss/JsScripts/**/*.js"
                ],
                //расположение объединенного файла
                dest: "ScriptsAndCss/Combined/combined.js" 
            },
            //объединение SCSS
            scss: {
                //сюда можно писать файлы для объединения через запятую
                src: ["ScriptsAndCss/CssFiles/*.scss"],
                //расположение объединенного файла
                dest: "ScriptsAndCss/Combined/combined.scss" 
            }
        },
        sass: {
            options: {
                implementation: sass,
                sourceMap: true
            },
            dist: {
                files: {
                    'ScriptsAndCss/Combined/combined.css':'ScriptsAndCss/Combined/combined.scss'
                }
            }
        },
        //сжатие JS
        uglify: { 
            js: {
                //какой файл сжимать
                src: ["ScriptsAndCss/Combined/combined.js"],
                //сжатый выходной файл
                dest: "wwwroot/js/app.min.js" 
            }
        },
        //сжатие CSS
        cssmin: { 
            css: {
                //какой файл сжимать
                src: ["ScriptsAndCss/Combined/combined.css"],
                //сжатый выходной файл
                dest: "wwwroot/css/app.min.css" 
            }
        },
        //наблюдение за изменениями
        watch: {
            //за изменением каких файлов наблюдаем
            files: ["ScriptsAndCss/JsScripts/**/*.js", "ScriptsAndCss/CssFiles/*.scss"],
            //какую задачу запускаем
            tasks: ["all"] 
        }
    });

    //для очистки файлов
    grunt.loadNpmTasks("grunt-contrib-clean");
    //для объединения JS и CSS
    grunt.loadNpmTasks("grunt-contrib-concat");
    //для сжатия JS
    grunt.loadNpmTasks("grunt-contrib-uglify");
    //для сжатия CSS
    grunt.loadNpmTasks("grunt-contrib-cssmin");
    //для компиляции scss
    grunt.loadNpmTasks("grunt-sass");
    //общая задача
    grunt.registerTask("all", ["clean", "concat", "sass", "uglify", "cssmin"]);
    //для наблюдения за изменениями в файлах
    grunt.loadNpmTasks("grunt-contrib-watch"); 
};
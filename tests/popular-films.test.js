import http from 'k6/http';
import {check, sleep} from 'k6';

export const options = {
    vus: 50,
    duration: '60s',
};

export default function () {
    const res = http.get('http://films.overoom.ru/api/films/popular');

    check(res, {
        'status is 200': (r) => r.status === 200,
        'body not empty': (r) => r.body.length > 0,
    });

    sleep(0.3);
}

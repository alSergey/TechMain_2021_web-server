# Веб сервер на С# + Thread Pool

## Запуск через .NET 5.0 SDK

```bash
dotnet run
```

## Запуск через Docker

```bash
docker build -t server .
docker run -p 8080:8080 server
```

## Нагрузочное тестирование через wrk

```bash
~ ❯ wrk -t8 -c10000 -d30s http://127.0.0.1:8080/httptest/wikipedia_russia.html
Running 30s test @ http://127.0.0.1:8080/httptest/wikipedia_russia.html
  8 threads and 10000 connections
  Thread Stats   Avg      Stdev     Max   +/- Stdev
    Latency   156.07ms   30.25ms 326.82ms   83.07%
    Req/Sec   102.10     64.50   313.00     68.36%
  16153 requests in 30.10s, 14.37GB read
  Socket errors: connect 9755, read 16993, write 102, timeout 0
Requests/sec:    536.73
Transfer/sec:    488.81MB
```

## Нагрузочное тестирование nginx через wrk

```bash
~ ❯ wrk -t8 -c10000 -d30s http://127.0.0.1/httptest/wikipedia_russia.html                                                                         30s
Running 30s test @ http://127.0.0.1/httptest/wikipedia_russia.html
  8 threads and 10000 connections
  Thread Stats   Avg      Stdev     Max   +/- Stdev
    Latency    74.17ms   43.88ms 951.21ms   70.87%
    Req/Sec   403.94    272.13     2.12k    79.73%
  96497 requests in 30.10s, 85.69GB read
  Socket errors: connect 9755, read 114, write 0, timeout 0
  Non-2xx or 3xx responses: 154
Requests/sec:   3205.57
Transfer/sec:      2.85GB
```

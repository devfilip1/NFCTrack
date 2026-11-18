# ShortLink client

Angular 22 dashboard for ShortLink. See the [main README](../README.md) for the full project.

```bash
npm install
npm start          # dev server on http://localhost:4200, proxies /api to http://localhost:8080
npm run build      # production build in dist/shortlink-client
```

The API must be running (for example with `docker compose up -d` from the repository root).
In Docker, the built app is served by nginx on port 8081 (see `Dockerfile` and `nginx/default.conf`).

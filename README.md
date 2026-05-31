# MorningRoutine

A web application built with [Fable](https://fable.io/) and [Feliz](https://github.com/Zaid-Ajaj/Feliz) to help manage your morning routine.

## Requirements

* [.NET SDK](https://dotnet.microsoft.com/download) 10.0 or higher
* [Node.js](https://nodejs.org) 22 LTS or higher

## Editor

VS Code with [Ionide](http://ionide.io/), Rider, or Visual Studio.

## Development

Install JS dependencies once:
```
npm install
```

Start the dev server (Fable watch + Vite) at http://localhost:8080:
```
npm start
```

Build for production into `dist/`:
```
npm run build
```

## Deployment

Production is deployed by [Vercel](https://vercel.com) on every push to `main`. The build is driven by [`vercel.json`](vercel.json), which bootstraps the .NET SDK in Vercel's Node-only build image and then runs `npm run build`.

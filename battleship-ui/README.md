# battleship-ui

The React + TypeScript (Vite) web client for the Battleship game. It renders the
setup and battle screens, handles ship placement, and talks to the
`Battleship.Api` backend over REST.

See the [root README](../README.md) for the full architecture, game rules, and
API reference.

## Development

```bash
npm install
npm run dev      # start the dev server (http://localhost:5173)
npm run build    # type-check and build for production
npm run lint     # run ESLint
```

The client expects the API at `http://localhost:5181`. Override with the
`VITE_API_BASE` environment variable:

```bash
VITE_API_BASE=http://localhost:5181 npm run dev
```

## Structure

```
src/
├── api/          # Typed API client and request/response types
├── game/         # Board encoding + ship geometry helpers
├── components/   # Boards, ship segments, fleet palette
├── screens/      # SetupScreen (placement) and GameScreen (battle)
└── index.css     # Global styles
```

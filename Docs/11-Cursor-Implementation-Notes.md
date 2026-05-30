## 31. Cursor Implementation Notes

Generate:

1. ASP.NET Core 9 Minimal API
2. Swagger/OpenAPI
3. React + TypeScript + Vite UI
4. SVG board rendering
5. In-memory game storage
6. UInt128 bitmap-based game engine
7. Placement-mask validation
8. Random bot placement
9. Random bot targeting
10. Unit tests for core game logic

Do not add:

- Authentication
- Database persistence
- SignalR
- Multiplayer
- Matchmaking
- User accounts
- Chat
- External services
- UI framework unless explicitly requested

Keep the implementation simple and prototype-oriented.

The server is authoritative.

The client renders only the state returned by the API.

# Overview

This project implements a simple Battleship game where a human player competes against a server-controlled opponent.

Design goals:

- Simple API
- Server-authoritative game state
- Stateless client rendering
- Compact game state representation
- Efficient server-side implementation
- In-memory game storage
- SVG-based user interface
- No user accounts
- No multiplayer support
- No game history
- No matchmaking
- No authentication
- No database
- No SignalR

The server maintains game state in memory using a `Guid` Game ID.

The client never calculates hits, misses, sunk ships, or game status. The client only renders the game state returned by the API.

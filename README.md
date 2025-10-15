# Truco

A console-based implementation of the classic Truco (the gaudério one) card game written in F#.

## About

Truco is a traditional card game played with a Spanish deck. This project implements the game logic and provides a console interface to play the game. 

The project is structured with:

- **Truco.Core**: Core game logic library containing game models, actions, and game loop
- **Truco.Console**: Console application for playing the game
- **Truco.Core.Test**: Unit tests for the core game logic


## Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later

### Environment Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd truco
```

2. Build the solution:
```bash
dotnet build
```

### Running Unit Tests

Run all tests:

```bash
dotnet test
```

Run tests with detailed output:

```bash
dotnet test --verbosity normal
```

## Running the Application (Console version)

To start the game, run the following command from the project root:

```bash
dotnet run --project src/Truco.Console/Truco.Console.fsproj
```

Or navigate to the console project directory and run:

```bash
cd src/Truco.Console
dotnet run
```

## Project Structure

```
truco/
├── src/
│   ├── Truco.Core/          # Core game logic library (domain layer)
│   └── Truco.Console/       # Console application (presentation layer)
├── test/
│   └── Truco.Core.Test/     # Unit tests
└── tools/
    └── scripts/             # Useful scripts
```

## Technologies Used

- **Language**: F# 9.0
- **Framework**: .NET 9
- **Testing**: xUnit, FsUnit
- **Code Coverage**: Coverlet

## Next steps

- [ ] Add the other rules:
  - [ ] Envido, real envido and falta envido
  - [ ] Flower and counter-flower
  - [ ] Truco, re-truco and vale quatro (four points)
- [ ] Create a web application (perhaps using Blazor)

## References

- [Regras Como Jogar Truco Gaudério](https://www.jogatina.com/regras-como-jogar-truco-gauderio.html)

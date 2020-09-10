# chess-app
Chess engine in C# with CLI interface

A ground-up re-build of a [former student project](https://github.com/stevenaw/2PlayerChess), this engine is built with a performance-first mindset using an approach that will support both human-human and human-computer games.

## CLI

The CLI allows for a game to be played. There are two modes:
- Interactive (`ChessApp.exe play`)
- Replay (`ChessApp.exe replay`)

### Interactive Mode

Interactive mode allows for a 2-player game to be played based on user input through various commands.
Interactive mode is entered by running `ChessApp.exe play`. All moves use Standard Algebraic Notation (SAN).

#### Commands

A list of commands is as follows:

##### move {moveNotation}

The 'move' command will move a piece based on the provided notation. Standard algebraic notation is supported.
Entering a move following standard algebraic notation without specifying the move command will still be treated as a move.

Ex: `move Nc3` or `Qxh4`

##### undo

The 'undo' command will undo the previous move .

Ex: `undo`

##### hint {square}

The 'hint' command will highlight a list of possible moves for the piece on the given square.

Ex: `hint e2`

##### exit

The 'exit' command exit the application. Before exiting, a prompt will be displayed to confirm the choice to exit.

##### help

The 'help' command will display a list of possible commands.

### Replay Mode

Replay mode allows a game to be replayed from a file saved on disk. The file is specified as a command file argument
and must be a valid file saved in Portable Game Notation (PGN) format. An optional second argument can be specified
to control the time delay between moves (in seconds). The default delay is 1 second.

Example:

`ChessApp.exe replay ..\my-pgn-file.pgn`

`ChessApp.exe replay C:\temp\my-other pgn-file.pgn 2.5`

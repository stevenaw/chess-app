# chess-app
Chess engine in C# with CLI interface

A ground-up re-build of a [former student project](https://github.com/stevenaw/2PlayerChess), this engine is built with a performance-first mindset using an approach that will support both human-human and human-computer games.
It makes extensive use of bit fields, and will only work on big endian architectures.

## CLI

The CLI allows for a game to be played.

### Commands

A list of commands is as follows:

#### move {moveNotation}

The 'move' command will move a piece based on the provided notation. Standard algebraic notation is supported.
Entering a move following standard algebraic notation without specifying the move command will still be treated as a move.

Ex: `move Nc3` or `Qxh4`

#### undo

The 'undo' command will undo the previous move .

Ex: `undo`

#### hint {square}

The 'hint' command will highlight a list of possible moves for the piece on the given square.

Ex: `hint e2`

#### exit

The 'exit' command exit the application. Before exiting, a prompt will be displayed to confirm the choice to exit.

#### help

The 'help' command will display a list of possible commands.
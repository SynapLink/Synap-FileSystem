# Synap Editor (Console Version)

## Overview

This is a console-based version of Synap Editor, providing a text-based interface for editing and encrypting text files. It maintains all the security and encryption features of the original applications while offering a simplified console interface.

## Features

- Text-based console interface
- Full AES-256 encryption for all saved files
- File browser dialogs for opening and saving files
- Text formatting options (Bold, Italic, Bold+Italic)
- Key management (Import, Export, Generate New Key)
- Compatible with the original .synap file format

## Installation

1. Ensure you have .NET 6.0 SDK or later installed
2. Run the build.bat script to compile the application:
   ```
   build.bat
   ```
3. The compiled executable will be available in the output directory mentioned by the build script

## Usage

1. Launch the application using run.bat or directly via the executable
2. Use the menu to navigate the application:
   - 1/N: Create a new file
   - 2/O: Open an existing file (opens file dialog)
   - 3/S: Save the current file (opens file dialog if needed)
   - 4/E: Edit text in the console
   - 5/F: Format text with Bold/Italic/Bold+Italic
   - 6/I: Import an encryption key
   - 7/X: Export the current encryption key
   - 8/G: Generate a new encryption key
   - 0/Q: Quit the application

## Editing Text

When in edit mode:
- Type your text as normal
- Use `:save` on a new line to save changes to memory
- Use `:exit` on a new line to discard changes

## Security Notes

- The encryption key is stored in the application directory as `synap_editor.key`
- For maximum security, consider backing up and protecting this key file
- All files are encrypted using AES-256 with a random initialization vector (IV)

## License

MIT 
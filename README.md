# Synap Editor

## Overview

Synap Editor is a simple, secure text editor built with encryption capabilities. It features built-in AES-256 encryption for all saved files, ensuring your sensitive text content remains private and protected.

## Versions

### Electron Version (Cross-Platform)

- Built with Electron and Node.js
- Works on Windows, macOS, and Linux
- Uses Node.js crypto module for encryption
- Located in the `NodeJS` folder

### Python Version (Cross-Platform)

- Built with Python and Tkinter
- Works on Windows, macOS, and Linux
- Uses the cryptography library for AES-256 encryption
- Located in the `Python` folder

### Windows Native Version (C#)

- Built with C# and WPF (Windows Presentation Foundation)
- Native Windows application with better performance
- Uses .NET cryptography libraries for AES-256 encryption
- Self-contained executable with no dependencies
- Located in the `Windows` folder

### Console Version

- Built with C# (.NET)
- Text-based console interface for Windows
- Uses .NET cryptography libraries for encryption
- Located in the `Console` folder

## File Transfer Tool

- SynapSender: A file transfer tool for sharing encrypted files
- Supports local hosting and remote domain hosting
- Available as a ZIP archive in the root folder

## Features

- Clean, minimal text editing interface
- End-to-end AES-256 encryption for all saved files
- Custom `.synap` file format for encrypted files
- Automatic encryption key management
- Cross-platform support (Electron and Python versions) or native Windows support (C# version)
- Text formatting (bold, italic, underline)
- Dark mode and customizable UI

## Installation

### Electron Version

1. Navigate to the NodeJS folder
2. Install dependencies:
   ```
   npm install
   ```
3. Start the application:
   ```
   node synapeditor.js
   ```
   
### Python Version

1. Navigate to the Python folder
2. Install dependencies:
   ```
   pip install -r requirements.txt
   ```
3. Start the application:
   ```
   python synap_editor.py
   ```
   
### Windows C# Version

1. Navigate to the Windows folder
2. Run the build.bat script to compile the application:
   ```
   build.bat
   ```
3. The compiled executable will be available in the output directory mentioned by the build script

### Console Version

1. Navigate to the Console folder
2. Run the build.bat script to compile the application
3. Launch the console version via generated executable

## Usage

- **Start Menu**: Choose between creating a new file, opening an existing file, or adjusting settings
- **Open File**: Load and decrypt a .synap file
- **Save File**: Encrypt and save your text as a .synap file
- **Formatting**: Use the formatting buttons or shortcuts to make text bold, italic, or underlined
- **Settings**: Adjust theme preferences, colors, and manage encryption keys

## Security Notes

- The encryption key is stored in the application directory as `synap_editor.key`
- For maximum security, consider backing up and protecting this key file
- All files are encrypted using AES-256 with a random initialization vector (IV)

## License

MIT 

Created by coding enthusiast. Free to use software.
Some parts of code is fixed by AI as coding the same program on 3 programming languages is hard for me. Sorry.

Honorable Mention: claude-3.7-sonnet-thinking

Thanks for using, any bugs can be reported to the github repository in `Issues`
// electron synap editor with encryption

const { app, BrowserWindow, ipcMain, dialog } = require('electron');
const fs = require('fs');
const path = require('path');
const crypto = require('crypto');

// key storage location
const algorithm = 'aes-256-cbc';
const KEY_FILE = path.join(__dirname, 'synap_editor.key');
const SETTINGS_FILE = path.join(__dirname, 'synap_settings.json');

// default settings
let settings = {
    darkMode: false,
    textColor: '#000000',
    backgroundColor: '#ffffff',
    highlightColor: '#4a90e2',
    previewLength: 200
};

// loads settings or creates defaults
function loadSettings() {
    try {
        if (fs.existsSync(SETTINGS_FILE)) {
            const data = fs.readFileSync(SETTINGS_FILE, 'utf8');
            const loadedSettings = JSON.parse(data);
            settings = { ...settings, ...loadedSettings };
        } else {
            saveSettings();
        }
    } catch (err) {
        console.error('error loading settings:', err);
    }
}

// writes settings to disk
function saveSettings() {
    try {
        fs.writeFileSync(SETTINGS_FILE, JSON.stringify(settings, null, 2), 'utf8');
    } catch (err) {
        console.error('error saving settings:', err);
    }
}

function loadKey() {
    if (!fs.existsSync(KEY_FILE)) {
        const key = generateNewKey();
        saveKey(key);
        return key;
    }
    return fs.readFileSync(KEY_FILE);
}

function generateNewKey() {
    return crypto.randomBytes(32);
}

function saveKey(key) {
    fs.writeFileSync(KEY_FILE, key);
}

let key = loadKey();

function encrypt(text) {
    const iv = crypto.randomBytes(16);
    const cipher = crypto.createCipheriv(algorithm, key, iv);
    let encrypted = cipher.update(text, 'utf8', 'hex');
    encrypted += cipher.final('hex');
    return iv.toString('hex') + ':' + encrypted;
}

function decrypt(data) {
    const parts = data.split(':');
    const iv = Buffer.from(parts[0], 'hex');
    const encryptedText = parts[1];
    const decipher = crypto.createDecipheriv(algorithm, key, iv);
    let decrypted = decipher.update(encryptedText, 'hex', 'utf8');
    decrypted += decipher.final('utf8');
    return decrypted;
}

let mainWindow;
let currentFilePath = null;
let startMenuVisible = true;

// init settings on start
loadSettings();

function createWindow() {
    mainWindow = new BrowserWindow({
        width: 800,
        height: 600,
        webPreferences: {
            nodeIntegration: true,
            contextIsolation: false
        }
    });
    mainWindow.loadURL(`data:text/html,
        <html>
        <head>
            <style>
                :root {
                    --main-bg-color: ${settings.darkMode ? '#2e2e2e' : '#ffffff'};
                    --main-text-color: ${settings.darkMode ? '#ffffff' : '#000000'};
                    --secondary-bg-color: ${settings.darkMode ? '#3e3e3e' : '#f0f0f0'};
                    --border-color: ${settings.darkMode ? '#555555' : '#cccccc'};
                    --highlight-color: ${settings.highlightColor};
                    --button-bg: ${settings.darkMode ? '#555555' : '#ffffff'};
                    --button-hover: ${settings.darkMode ? '#666666' : '#e0e0e0'};
                }
                
                body { 
                    font-family: 'Segoe UI', Tahoma, sans-serif; 
                    margin: 0; 
                    padding: 0; 
                    display: flex; 
                    flex-direction: column; 
                    height: 100vh;
                    background-color: var(--main-bg-color);
                    color: var(--main-text-color);
                    transition: all 0.3s ease;
                }
                
                .toolbar { 
                    background-color: var(--secondary-bg-color); 
                    padding: 10px; 
                    display: flex; 
                    gap: 5px; 
                    border-bottom: 1px solid var(--border-color); 
                }
                
                .toolbar button {
                    padding: 5px 10px;
                    cursor: pointer;
                    background-color: var(--button-bg);
                    border: 1px solid var(--border-color);
                    border-radius: 3px;
                    color: var(--main-text-color);
                }
                
                .toolbar button:hover {
                    background-color: var(--button-hover);
                }
                
                #editor { 
                    flex-grow: 1; 
                    resize: none; 
                    padding: 10px; 
                    font-family: monospace; 
                    font-size: 14px; 
                    border: none; 
                    outline: none;
                    background-color: var(--main-bg-color);
                    color: var(--main-text-color);
                }
                
                .bottom-panel { 
                    display: flex; 
                    padding: 10px; 
                    border-top: 1px solid var(--border-color); 
                    justify-content: space-between;
                    background-color: var(--secondary-bg-color);
                }
                
                .bottom-panel div { 
                    display: flex; 
                    gap: 5px; 
                }
                
                .format-help {
                    padding: 5px 10px;
                    border: 1px solid var(--border-color);
                    border-radius: 3px;
                    background-color: var(--secondary-bg-color);
                    color: var(--main-text-color);
                }
                
                /* Start menu styles */
                #start-menu {
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background-color: rgba(0, 0, 0, 0.5);
                    display: flex;
                    justify-content: center;
                    align-items: center;
                    z-index: 100;
                }
                
                .menu-container {
                    background-color: var(--main-bg-color);
                    border-radius: 8px;
                    box-shadow: 0 4px 20px rgba(0, 0, 0, 0.3);
                    width: 80%;
                    max-width: 600px;
                    overflow: hidden;
                    animation: fadeIn 0.3s ease;
                }
                
                @keyframes fadeIn {
                    from { opacity: 0; transform: translateY(-20px); }
                    to { opacity: 1; transform: translateY(0); }
                }
                
                .menu-header {
                    background-color: var(--highlight-color);
                    color: white;
                    padding: 15px 20px;
                    font-size: 20px;
                    font-weight: bold;
                }
                
                .menu-content {
                    padding: 20px;
                }
                
                .menu-options {
                    display: grid;
                    grid-template-columns: repeat(2, 1fr);
                    gap: 15px;
                    margin-bottom: 20px;
                }
                
                .menu-option {
                    background-color: var(--secondary-bg-color);
                    border: 1px solid var(--border-color);
                    border-radius: 5px;
                    padding: 15px;
                    cursor: pointer;
                    transition: all 0.2s ease;
                    display: flex;
                    flex-direction: column;
                    align-items: center;
                    text-align: center;
                }
                
                .menu-option:hover {
                    transform: translateY(-2px);
                    box-shadow: 0 5px 15px rgba(0, 0, 0, 0.1);
                    border-color: var(--highlight-color);
                }
                
                .menu-option h3 {
                    margin: 10px 0 5px 0;
                    color: var(--main-text-color);
                }
                
                .menu-option p {
                    margin: 0;
                    font-size: 13px;
                    opacity: 0.8;
                }
                
                .recent-files {
                    border-top: 1px solid var(--border-color);
                    padding-top: 15px;
                }
                
                .recent-files h3 {
                    margin-top: 0;
                    color: var(--main-text-color);
                }
                
                .recent-item {
                    padding: 8px 10px;
                    border-radius: 3px;
                    cursor: pointer;
                    display: flex;
                    align-items: center;
                }
                
                .recent-item:hover {
                    background-color: var(--secondary-bg-color);
                }
                
                .hidden {
                    display: none !important;
                }
                
                /* Settings dialog */
                #settings-dialog {
                    position: absolute;
                    top: 0;
                    left: 0;
                    width: 100%;
                    height: 100%;
                    background-color: rgba(0, 0, 0, 0.5);
                    display: none;
                    justify-content: center;
                    align-items: center;
                    z-index: 100;
                }
            </style>
        </head>
        <body>
            <!-- Start Menu -->
            <div id="start-menu">
                <div class="menu-container">
                    <div class="menu-header">Synap Editor</div>
                    <div class="menu-content">
                        <div class="menu-options">
                            <div class="menu-option" onclick="window.electron.newFile()">
                                <h3>New File</h3>
                                <p>Create an empty encrypted text file</p>
                            </div>
                            <div class="menu-option" onclick="window.electron.openFile()">
                                <h3>Open File</h3>
                                <p>Browse for an existing .synap file</p>
                            </div>
                            <div class="menu-option" onclick="window.electron.showEditor()">
                                <h3>Text Editor</h3>
                                <p>Open the text editor directly</p>
                            </div>
                            <div class="menu-option" onclick="window.electron.showSettings()">
                                <h3>Settings</h3>
                                <p>Customize appearance and encryption</p>
                            </div>
                        </div>
                        
                        <div class="recent-files" id="recent-files-section">
                            <h3>Recent Files</h3>
                            <div id="recent-files-list">
                                <!-- Will be populated dynamically -->
                                <div class="recent-item">No recent files</div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- Settings Dialog -->
            <div id="settings-dialog">
                <div class="menu-container">
                    <div class="menu-header">Settings & Customization</div>
                    <div class="menu-content">
                        <div style="margin-bottom: 20px;">
                            <div style="display: flex; justify-content: space-between; margin-bottom: 15px;">
                                <label>Dark Mode</label>
                                <input type="checkbox" id="dark-mode-toggle">
                            </div>
                            
                            <div style="display: flex; justify-content: space-between; margin-bottom: 15px;">
                                <label>Highlight Color</label>
                                <input type="color" id="highlight-color" value="${settings.highlightColor}">
                            </div>
                            
                            <div style="display: flex; justify-content: space-between; margin-bottom: 15px;">
                                <label>Preview Length (chars)</label>
                                <input type="number" id="preview-length" min="50" max="500" value="${settings.previewLength}" style="width: 70px">
                            </div>
                        </div>
                        
                        <div style="border-top: 1px solid var(--border-color); padding-top: 15px;">
                            <h3>Encryption Options</h3>
                            <div style="display: flex; gap: 10px; margin-top: 10px;">
                                <button onclick="window.electron.importKey()">Import Key</button>
                                <button onclick="window.electron.exportKey()">Export Key</button>
                                <button onclick="window.electron.generateNewKey()">Generate New Key</button>
                            </div>
                        </div>
                        
                        <div style="margin-top: 20px; text-align: right;">
                            <button onclick="window.electron.saveSettings()">Save Settings</button>
                            <button onclick="window.electron.closeSettings()">Cancel</button>
                        </div>
                    </div>
                </div>
            </div>
        
            <!-- Main Editor Interface (hidden initially) -->
            <div id="editor-interface" class="hidden">
                <div class="toolbar">
                    <button onclick="window.electron.formatText('bold')">Bold</button>
                    <button onclick="window.electron.formatText('italic')">Italic</button>
                    <button onclick="window.electron.formatText('boldItalic')">Bold+Italic</button>
                    <span style="margin-left: 15px; align-self: center;">Format: </span>
                    <select class="format-help">
                        <option>**Bold**</option>
                        <option>***Italic***</option>
                        <option>****Bold Italic****</option>
                    </select>
                    <div style="margin-left: auto;">
                        <button onclick="window.electron.showStartMenu()">Main Menu</button>
                    </div>
                </div>
                <textarea id="editor"></textarea>
                <div class="bottom-panel">
                    <div>
                        <button onclick="window.electron.newFile()">New File</button>
                    </div>
                    <div>
                        <button onclick="window.electron.openFile()">Open</button>
                        <button onclick="window.electron.saveFile()">Save</button>
                    </div>
                    <div>
                        <button onclick="window.electron.showSettings()">Settings</button>
                    </div>
                </div>
            </div>
            
            <script>
                const { ipcRenderer } = require('electron');
                
                // Initialize UI based on settings
                document.getElementById('dark-mode-toggle').checked = ${settings.darkMode};
                document.getElementById('highlight-color').value = '${settings.highlightColor}';
                document.getElementById('preview-length').value = ${settings.previewLength};
                
                window.electron = {
                    newFile: () => {
                        ipcRenderer.invoke('new-file');
                        document.getElementById('start-menu').classList.add('hidden');
                        document.getElementById('editor-interface').classList.remove('hidden');
                    },
                    openFile: () => {
                        ipcRenderer.invoke('open-file');
                        document.getElementById('start-menu').classList.add('hidden');
                        document.getElementById('editor-interface').classList.remove('hidden');
                    },
                    saveFile: () => ipcRenderer.invoke('save-file'),
                    importKey: () => ipcRenderer.invoke('import-key'),
                    exportKey: () => ipcRenderer.invoke('export-key'),
                    generateNewKey: () => ipcRenderer.invoke('generate-new-key'),
                    showEditor: () => {
                        document.getElementById('start-menu').classList.add('hidden');
                        document.getElementById('editor-interface').classList.remove('hidden');
                    },
                    showStartMenu: () => {
                        document.getElementById('start-menu').classList.remove('hidden');
                        document.getElementById('editor-interface').classList.add('hidden');
                    },
                    showSettings: () => {
                        document.getElementById('settings-dialog').style.display = 'flex';
                    },
                    closeSettings: () => {
                        document.getElementById('settings-dialog').style.display = 'none';
                    },
                    saveSettings: () => {
                        const settings = {
                            darkMode: document.getElementById('dark-mode-toggle').checked,
                            highlightColor: document.getElementById('highlight-color').value,
                            previewLength: parseInt(document.getElementById('preview-length').value)
                        };
                        ipcRenderer.invoke('save-settings', settings);
                    },
                    formatText: (type) => {
                        const editor = document.getElementById('editor');
                        const start = editor.selectionStart;
                        const end = editor.selectionEnd;
                        const selectedText = editor.value.substring(start, end);
                        let marker = '';
                        
                        if (type === 'bold') marker = '**';
                        else if (type === 'italic') marker = '***';
                        else if (type === 'boldItalic') marker = '****';
                        
                        if (start !== end) {
                            // Text is selected, wrap it with markers
                            const newText = marker + selectedText + marker;
                            editor.setRangeText(newText, start, end, 'select');
                            editor.setSelectionRange(start + marker.length, start + marker.length + selectedText.length);
                        } else {
                            // No text selected, just insert markers and place cursor in the middle
                            const newText = marker + marker;
                            editor.setRangeText(newText, start, end, 'select');
                            editor.setSelectionRange(start + marker.length, start + marker.length);
                        }
                        editor.focus();
                    }
                };
                
                ipcRenderer.on('file-content', (event, content) => {
                    document.getElementById('editor').value = content;
                });
                
                ipcRenderer.on('update-title', (event, title) => {
                    document.title = title;
                });
                
                ipcRenderer.on('update-theme', (event, newSettings) => {
                    document.documentElement.style.setProperty('--main-bg-color', newSettings.darkMode ? '#2e2e2e' : '#ffffff');
                    document.documentElement.style.setProperty('--main-text-color', newSettings.darkMode ? '#ffffff' : '#000000');
                    document.documentElement.style.setProperty('--secondary-bg-color', newSettings.darkMode ? '#3e3e3e' : '#f0f0f0');
                    document.documentElement.style.setProperty('--border-color', newSettings.darkMode ? '#555555' : '#cccccc');
                    document.documentElement.style.setProperty('--highlight-color', newSettings.highlightColor);
                    document.documentElement.style.setProperty('--button-bg', newSettings.darkMode ? '#555555' : '#ffffff');
                    document.documentElement.style.setProperty('--button-hover', newSettings.darkMode ? '#666666' : '#e0e0e0');
                });
                
                document.title = "Synap Editor";
            </script>
        </body>
        </html>`);
        
    mainWindow.on('closed', () => {
        mainWindow = null;
    });
}

app.whenReady().then(createWindow);

app.on('window-all-closed', () => {
    if (process.platform !== 'darwin') {
        app.quit();
    }
});

app.on('activate', () => {
    if (mainWindow === null) {
        createWindow();
    }
});

// New File handler
ipcMain.handle('new-file', async () => {
    // Check if there's content to save
    const content = await mainWindow.webContents.executeJavaScript('document.getElementById("editor").value');
    
    if (content && content.trim() !== '') {
        const { response } = await dialog.showMessageBox({
            type: 'question',
            buttons: ['Yes', 'No', 'Cancel'],
            title: 'Save File',
            message: 'Do you want to save the current file?'
        });
        
        if (response === 0) { // Yes
            await handleSaveFile();
        } else if (response === 2) { // Cancel
            return;
        }
    }
    
    // Clear editor and reset file path
    mainWindow.webContents.executeJavaScript('document.getElementById("editor").value = ""');
    currentFilePath = null;
    mainWindow.webContents.send('update-title', "Synap Editor - New File");
});

// Open File handler
ipcMain.handle('open-file', async () => {
    const { canceled, filePaths } = await dialog.showOpenDialog({
        filters: [{ name: 'Synap Files', extensions: ['synap'] }]
    });
    
    if (!canceled && filePaths.length > 0) {
        try {
            const data = fs.readFileSync(filePaths[0], 'utf8');
            const content = decrypt(data);
            mainWindow.webContents.send('file-content', content);
            currentFilePath = filePaths[0];
            mainWindow.webContents.send('update-title', `Synap Editor - ${path.basename(filePaths[0])}`);
        } catch (err) {
            dialog.showErrorBox('Error', 'Failed to decrypt file. The file may be corrupted or encrypted with a different key.');
        }
    }
});

// Save File handler
ipcMain.handle('save-file', async () => {
    await handleSaveFile();
});

// Save settings handler
ipcMain.handle('save-settings', async (event, newSettings) => {
    settings = { ...settings, ...newSettings };
    saveSettings();
    mainWindow.webContents.send('update-theme', settings);
    mainWindow.webContents.send('update-title', document.title);
    
    // Close settings dialog
    mainWindow.webContents.executeJavaScript('document.getElementById("settings-dialog").style.display = "none"');
    
    dialog.showMessageBox({
        type: 'info',
        title: 'Settings Saved',
        message: 'Your settings have been saved and applied.'
    });
});

async function handleSaveFile() {
    let filePath = currentFilePath;
    
    if (!filePath) {
        const { canceled, filePath: newPath } = await dialog.showSaveDialog({
            defaultPath: 'file.synap',
            filters: [{ name: 'Synap Files', extensions: ['synap'] }]
        });
        
        if (canceled || !newPath) {
            return false;
        }
        
        filePath = newPath;
    }
    
    try {
        const content = await mainWindow.webContents.executeJavaScript('document.getElementById("editor").value');
        const encrypted = encrypt(content);
        fs.writeFileSync(filePath, encrypted, 'utf8');
        currentFilePath = filePath;
        mainWindow.webContents.send('update-title', `Synap Editor - ${path.basename(filePath)}`);
        dialog.showMessageBox({ message: 'File saved and encrypted successfully.' });
        return true;
    } catch (err) {
        dialog.showErrorBox('Error', `Failed to save file: ${err.message}`);
        return false;
    }
}

// Import Key handler
ipcMain.handle('import-key', async () => {
    const { canceled, filePaths } = await dialog.showOpenDialog({
        title: 'Import Encryption Key',
        filters: [{ name: 'Key Files', extensions: ['key'] }],
        properties: ['openFile']
    });
    
    if (!canceled && filePaths.length > 0) {
        try {
            // Read the key file
            const importedKey = fs.readFileSync(filePaths[0]);
            
            // Verify key is valid (32 bytes for AES-256)
            if (importedKey.length !== 32) {
                dialog.showErrorBox('Invalid Key', 'The selected file is not a valid encryption key. It must be exactly 32 bytes in length.');
                return;
            }
            
            const { response } = await dialog.showMessageBox({
                type: 'warning',
                buttons: ['Yes', 'No'],
                title: 'Import Key',
                message: 'WARNING: Importing a new key will make all previously encrypted files unreadable with the current key.\n\nAre you sure you want to import this key?'
            });
            
            if (response === 0) { // Yes
                // Backup the old key
                const backupPath = KEY_FILE + '.backup';
                fs.copyFileSync(KEY_FILE, backupPath);
                
                // Save the imported key
                fs.writeFileSync(KEY_FILE, importedKey);
                
                // Update the key variable
                key = importedKey;
                
                dialog.showMessageBox({
                    type: 'info',
                    title: 'Key Imported',
                    message: `Encryption key imported successfully.\nYour old key has been backed up to: ${backupPath}`
                });
            }
        } catch (err) {
            dialog.showErrorBox('Error', `Failed to import key: ${err.message}`);
        }
    }
});

// Export Key handler
ipcMain.handle('export-key', async () => {
    const { canceled, filePath } = await dialog.showSaveDialog({
        title: 'Export Encryption Key',
        defaultPath: 'synap_export.key',
        filters: [{ name: 'Key Files', extensions: ['key'] }]
    });
    
    if (!canceled && filePath) {
        try {
            fs.copyFileSync(KEY_FILE, filePath);
            dialog.showMessageBox({
                type: 'warning',
                title: 'Key Exported',
                message: 'Encryption key exported successfully.\n\nWARNING: Keep this key secure! Anyone with this key can decrypt your files.'
            });
        } catch (err) {
            dialog.showErrorBox('Error', `Failed to export key: ${err.message}`);
        }
    }
});

// Generate New Key handler
ipcMain.handle('generate-new-key', async () => {
    const { response } = await dialog.showMessageBox({
        type: 'warning',
        buttons: ['Yes', 'No'],
        title: 'Generate New Key',
        message: 'WARNING: Generating a new key will make all previously encrypted files unreadable unless you\'ve backed up the current key.\n\nAre you sure you want to continue?'
    });
    
    if (response === 0) { // Yes
        try {
            // Backup the old key
            const backupPath = KEY_FILE + '.backup';
            fs.copyFileSync(KEY_FILE, backupPath);
            
            // Generate and save new key
            const newKey = generateNewKey();
            saveKey(newKey);
            
            // Update the key variable
            key = Buffer.from(newKey);
            
            dialog.showMessageBox({
                type: 'info',
                title: 'Key Generated',
                message: `New encryption key generated successfully.\nYour old key has been backed up to: ${backupPath}`
            });
        } catch (err) {
            dialog.showErrorBox('Error', `Failed to generate new key: ${err.message}`);
        }
    }
});

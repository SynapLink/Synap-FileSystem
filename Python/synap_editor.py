#!/usr/bin/env python3
# python synap editor with encryption

import os
import sys
import json
import tkinter as tk
from tkinter import filedialog, messagebox, ttk
import tkinter.font as tkfont
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
from cryptography.hazmat.primitives import padding
from cryptography.hazmat.backends import default_backend
import base64
import secrets

# paths for key and settings
KEY_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'synap_editor.key')
SETTINGS_FILE = os.path.join(os.path.dirname(os.path.abspath(__file__)), 'synap_settings.json')

# default settings
settings = {
    'dark_mode': False,
    'text_color': '#000000',
    'background_color': '#ffffff',
    'highlight_color': '#4a90e2',
    'preview_length': 200
}

def load_settings():
    """load settings from file or create defaults"""
    global settings
    try:
        if os.path.exists(SETTINGS_FILE):
            with open(SETTINGS_FILE, 'r') as f:
                loaded_settings = json.load(f)
                settings.update(loaded_settings)
        else:
            # no settings file found so create one
            save_settings()
    except Exception as e:
        print(f"error loading settings: {e}")

def save_settings():
    """save settings to file"""
    try:
        with open(SETTINGS_FILE, 'w') as f:
            json.dump(settings, f, indent=2)
    except Exception as e:
        print(f"failed to save settings: {e}")

def load_key():
    """load encryption key or generate new one"""
    if not os.path.exists(KEY_FILE):
        # no key file so make a new one
        key = generate_new_key()
        save_key(key)
        return key
    with open(KEY_FILE, 'rb') as f:
        return f.read()

def generate_new_key():
    """generate new 32-byte encryption key"""
    return secrets.token_bytes(32)

def save_key(key):
    """save key to file"""
    with open(KEY_FILE, 'wb') as f:
        f.write(key)

def encrypt(text, key):
    """encrypt text using aes-256-cbc"""
    # random iv for security
    iv = secrets.token_bytes(16)
    
    # pad data to needed length
    padder = padding.PKCS7(algorithms.AES.block_size).padder()
    padded_data = padder.update(text.encode()) + padder.finalize()
    
    # create cipher and encrypt
    cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
    encryptor = cipher.encryptor()
    encrypted_data = encryptor.update(padded_data) + encryptor.finalize()
    
    # concat iv and data
    result = iv + encrypted_data
    return base64.b64encode(result).decode('utf-8')

def decrypt(encrypted_data, key):
    """decrypt data with aes-256-cbc"""
    # decode from base64
    data = base64.b64decode(encrypted_data.encode('utf-8'))
    
    # get iv from first 16 bytes
    iv = data[:16]
    ciphertext = data[16:]
    
    # create cipher and decrypt
    cipher = Cipher(algorithms.AES(key), modes.CBC(iv), backend=default_backend())
    decryptor = cipher.decryptor()
    padded_data = decryptor.update(ciphertext) + decryptor.finalize()
    
    # remove padding
    unpadder = padding.PKCS7(algorithms.AES.block_size).unpadder()
    data = unpadder.update(padded_data) + unpadder.finalize()
    return data.decode('utf-8')

class StartMenu(tk.Frame):
    """start menu for app"""
    def __init__(self, parent, on_new_file, on_open_file, on_settings, on_exit):
        super().__init__(parent, bg="#f0f0f0")
        self.parent = parent
        
        # header stuff
        header_frame = tk.Frame(self, bg="#f0f0f0")
        header_frame.pack(pady=20, fill=tk.X)
        
        title = tk.Label(header_frame, text="SYNAP EDITOR", font=("Helvetica", 24, "bold"), bg="#f0f0f0")
        title.pack()
        
        subtitle = tk.Label(header_frame, text="Secure Text Editor with Encryption", 
                          font=("Helvetica", 12), bg="#f0f0f0", fg="#666666")
        subtitle.pack(pady=5)
        
        # buttons container
        button_frame = tk.Frame(self, bg="#f0f0f0")
        button_frame.pack(pady=20)
        
        # button config
        button_width = 20
        button_height = 2
        button_font = ("Helvetica", 12)
        button_padding = 10
        
        # new file btn
        new_btn = tk.Button(button_frame, text="New File", width=button_width, height=button_height,
                           font=button_font, command=on_new_file, bg="#4a90e2", fg="white")
        new_btn.pack(pady=button_padding)
        
        # open file btn
        open_btn = tk.Button(button_frame, text="Open File", width=button_width, height=button_height,
                            font=button_font, command=on_open_file, bg="#4a90e2", fg="white")
        open_btn.pack(pady=button_padding)
        
        # settings btn
        settings_btn = tk.Button(button_frame, text="Settings", width=button_width, height=button_height,
                               font=button_font, command=on_settings, bg="#4a90e2", fg="white")
        settings_btn.pack(pady=button_padding)
        
        # exit btn
        exit_btn = tk.Button(button_frame, text="Exit", width=button_width, height=button_height,
                            font=button_font, command=on_exit, bg="#d9534f", fg="white")
        exit_btn.pack(pady=button_padding)

class SettingsDialog(tk.Toplevel):
    """settings dialog window"""
    def __init__(self, parent, on_save):
        super().__init__(parent)
        self.parent = parent
        self.on_save = on_save
        
        self.title("Settings")
        self.geometry("400x350")
        self.resizable(False, False)
        
        # vars for settings
        self.dark_mode_var = tk.BooleanVar(value=settings['dark_mode'])
        self.text_color_var = tk.StringVar(value=settings['text_color'])
        self.background_color_var = tk.StringVar(value=settings['background_color'])
        self.highlight_color_var = tk.StringVar(value=settings['highlight_color'])
        self.preview_length_var = tk.IntVar(value=settings['preview_length'])
        
        # main container
        main_frame = tk.Frame(self, padx=20, pady=20)
        main_frame.pack(fill=tk.BOTH, expand=True)
        
        # dark mode toggle
        dark_mode_check = tk.Checkbutton(main_frame, text="Dark Mode", variable=self.dark_mode_var)
        dark_mode_check.grid(row=0, column=0, sticky=tk.W, pady=5)
        
        # color settings
        tk.Label(main_frame, text="Text Color:").grid(row=1, column=0, sticky=tk.W, pady=5)
        tk.Entry(main_frame, textvariable=self.text_color_var, width=10).grid(row=1, column=1, pady=5)
        
        tk.Label(main_frame, text="Background Color:").grid(row=2, column=0, sticky=tk.W, pady=5)
        tk.Entry(main_frame, textvariable=self.background_color_var, width=10).grid(row=2, column=1, pady=5)
        
        tk.Label(main_frame, text="Highlight Color:").grid(row=3, column=0, sticky=tk.W, pady=5)
        tk.Entry(main_frame, textvariable=self.highlight_color_var, width=10).grid(row=3, column=1, pady=5)
        
        # preview len
        tk.Label(main_frame, text="Preview Length:").grid(row=4, column=0, sticky=tk.W, pady=5)
        tk.Spinbox(main_frame, from_=50, to=500, increment=10, textvariable=self.preview_length_var, width=5).grid(row=4, column=1, pady=5)
        
        # encryption key stuff
        tk.Label(main_frame, text="Encryption Key:").grid(row=5, column=0, sticky=tk.W, pady=5)
        key_frame = tk.Frame(main_frame)
        key_frame.grid(row=5, column=1, pady=5)
        
        tk.Button(key_frame, text="Generate New Key", command=self.generate_key).pack(side=tk.LEFT)
        
        # save/cancel btns
        button_frame = tk.Frame(main_frame)
        button_frame.grid(row=6, column=0, columnspan=2, pady=20)
        
        tk.Button(button_frame, text="Save", command=self.save_settings, width=10).pack(side=tk.LEFT, padx=5)
        tk.Button(button_frame, text="Cancel", command=self.destroy, width=10).pack(side=tk.LEFT, padx=5)
    
    def generate_key(self):
        """make new encryption key"""
        if messagebox.askyesno("Generate New Key", 
                             "Generating a new key will make existing files unreadable. Are you sure?"):
            global key
            key = generate_new_key()
            save_key(key)
            messagebox.showinfo("Key Generated", "New encryption key has been generated and saved.")
    
    def save_settings(self):
        """save settings and update ui"""
        global settings
        settings['dark_mode'] = self.dark_mode_var.get()
        settings['text_color'] = self.text_color_var.get()
        settings['background_color'] = self.background_color_var.get()
        settings['highlight_color'] = self.highlight_color_var.get()
        settings['preview_length'] = self.preview_length_var.get()
        
        save_settings()
        self.on_save()
        self.destroy()

class SynapEditor:
    """main app class"""
    def __init__(self, root):
        self.root = root
        self.root.title("Synap Editor")
        self.root.geometry("800x600")
        
        # load key for encryption
        self.key = load_key()
        
        # load saved settings
        load_settings()
        
        # track current file
        self.current_file_path = None
        
        # main container for ui
        self.main_container = tk.Frame(self.root)
        self.main_container.pack(fill=tk.BOTH, expand=True)
        
        # show menu on start
        self.show_start_menu()
    
    def show_start_menu(self):
        """show the start menu"""
        # clear container first
        for widget in self.main_container.winfo_children():
            widget.destroy()
        
        # create menu
        self.start_menu = StartMenu(
            self.main_container,
            on_new_file=self.new_file,
            on_open_file=self.open_file,
            on_settings=self.show_settings,
            on_exit=self.root.quit
        )
        self.start_menu.pack(fill=tk.BOTH, expand=True)
    
    def show_editor(self):
        """show the text editor"""
        # clear container first
        for widget in self.main_container.winfo_children():
            widget.destroy()
        
        # set theme colors
        bg_color = "#2e2e2e" if settings['dark_mode'] else settings['background_color']
        fg_color = "#ffffff" if settings['dark_mode'] else settings['text_color']
        button_bg = "#555555" if settings['dark_mode'] else "#f0f0f0"
        
        # editor container
        self.editor_frame = tk.Frame(self.main_container, bg=bg_color)
        self.editor_frame.pack(fill=tk.BOTH, expand=True)
        
        # toolbar for buttons
        toolbar = tk.Frame(self.editor_frame, bg=button_bg)
        toolbar.pack(fill=tk.X)
        
        # toolbar buttons
        tk.Button(toolbar, text="Menu", command=self.show_start_menu).pack(side=tk.LEFT, padx=5, pady=5)
        tk.Button(toolbar, text="Open", command=self.open_file).pack(side=tk.LEFT, padx=5, pady=5)
        tk.Button(toolbar, text="Save", command=self.save_file).pack(side=tk.LEFT, padx=5, pady=5)
        tk.Button(toolbar, text="Save As", command=self.save_file_as).pack(side=tk.LEFT, padx=5, pady=5)
        
        # format buttons
        format_frame = tk.Frame(toolbar, bg=button_bg)
        format_frame.pack(side=tk.RIGHT, padx=5, pady=5)
        
        tk.Button(format_frame, text="B", font=("Helvetica", 10, "bold"), 
                 command=lambda: self.insert_format_tag("**")).pack(side=tk.LEFT, padx=2)
        tk.Button(format_frame, text="I", font=("Helvetica", 10, "italic"), 
                 command=lambda: self.insert_format_tag("*")).pack(side=tk.LEFT, padx=2)
        tk.Button(format_frame, text="U", font=("Helvetica", 10, "underline"),
                 command=lambda: self.insert_format_tag("__")).pack(side=tk.LEFT, padx=2)
        
        # text editor widget
        self.editor = tk.Text(self.editor_frame, wrap=tk.WORD, bg=bg_color, fg=fg_color,
                            insertbackground=fg_color, font=("Courier New", 12))
        self.editor.pack(fill=tk.BOTH, expand=True, padx=10, pady=10)
        
        # status bar
        self.status_bar = tk.Label(self.editor_frame, text="Ready", bd=1, relief=tk.SUNKEN, anchor=tk.W, bg=button_bg, fg=fg_color)
        self.status_bar.pack(side=tk.BOTTOM, fill=tk.X)
    
    def show_settings(self):
        """show settings dialog"""
        settings_dialog = SettingsDialog(self.root, on_save=self.apply_settings)
        settings_dialog.transient(self.root)
        settings_dialog.grab_set()
        self.root.wait_window(settings_dialog)
    
    def apply_settings(self):
        """apply settings to ui"""
        # update editor if visible
        if hasattr(self, 'editor_frame'):
            bg_color = "#2e2e2e" if settings['dark_mode'] else settings['background_color']
            fg_color = "#ffffff" if settings['dark_mode'] else settings['text_color']
            
            self.editor.config(bg=bg_color, fg=fg_color, insertbackground=fg_color)
    
    def insert_format_tag(self, tag):
        """insert format tag at cursor or selection"""
        if self.editor.tag_ranges("sel"):
            # get selection
            start = self.editor.index("sel.first")
            end = self.editor.index("sel.last")
            
            # get selected text
            selected_text = self.editor.get(start, end)
            
            # replace with tagged text
            self.editor.delete(start, end)
            self.editor.insert(start, f"{tag}{selected_text}{tag}")
        else:
            # just insert at cursor
            current_pos = self.editor.index(tk.INSERT)
            self.editor.insert(current_pos, f"{tag}{tag}")
            # move cursor between tags
            self.editor.mark_set(tk.INSERT, f"{current_pos}+{len(tag)}c")
    
    def new_file(self):
        """create new empty file"""
        self.current_file_path = None
        self.show_editor()
        self.editor.delete(1.0, tk.END)
        self.update_status()
    
    def open_file(self):
        """open and decrypt file"""
        file_path = filedialog.askopenfilename(
            defaultextension=".synap",
            filetypes=[("Synap Files", "*.synap"), ("All Files", "*.*")]
        )
        
        if file_path:
            try:
                with open(file_path, 'r') as f:
                    encrypted_data = f.read()
                
                # decrypt it
                decrypted_text = decrypt(encrypted_data, self.key)
                
                # show editor if needed
                self.show_editor()
                
                # set text
                self.editor.delete(1.0, tk.END)
                self.editor.insert(1.0, decrypted_text)
                
                # update file path
                self.current_file_path = file_path
                self.update_status()
            except Exception as e:
                messagebox.showerror("Error", f"Could not open file: {e}")
    
    def save_file(self):
        """save and encrypt current file"""
        if not self.current_file_path:
            self.save_file_as()
        else:
            self.save_to_file(self.current_file_path)
    
    def save_file_as(self):
        """save as new file"""
        file_path = filedialog.asksaveasfilename(
            defaultextension=".synap",
            filetypes=[("Synap Files", "*.synap"), ("All Files", "*.*")]
        )
        
        if file_path:
            self.save_to_file(file_path)
    
    def save_to_file(self, file_path):
        """encrypt and save to file"""
        try:
            # get text
            text = self.editor.get(1.0, tk.END)
            
            # encrypt it
            encrypted_data = encrypt(text, self.key)
            
            # write to file
            with open(file_path, 'w') as f:
                f.write(encrypted_data)
            
            # update path
            self.current_file_path = file_path
            self.update_status()
            
            messagebox.showinfo("Success", "File saved successfully.")
        except Exception as e:
            messagebox.showerror("Error", f"Could not save file: {e}")
    
    def update_status(self):
        """update status bar with file info"""
        if hasattr(self, 'status_bar'):
            if self.current_file_path:
                filename = os.path.basename(self.current_file_path)
                self.status_bar.config(text=f"File: {filename}")
            else:
                self.status_bar.config(text="New File")

if __name__ == "__main__":
    # load settings first
    load_settings()
    
    # create window
    root = tk.Tk()
    app = SynapEditor(root)
    
    # start app
    root.mainloop() 
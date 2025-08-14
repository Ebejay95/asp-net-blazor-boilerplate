import os

# Automatisches Login aktivieren
LOGIN_BANNER = "CMC Development Environment - Auto-configured"

# Session-Konfiguration
SESSION_EXPIRATION_TIME = 24 * 60 * 60  # 24 Stunden

# Auto-Discovery von Servern
SHOW_GRAVATAR_IMAGE = False

# Development-optimierte Einstellungen
CONSOLE_LOG_LEVEL = 10  # DEBUG
FILE_LOG_LEVEL = 10     # DEBUG

# Automatische Passwort-Speicherung erlauben
ALLOW_SAVE_PASSWORD = True

# Server-Connection Defaults
DEFAULT_SERVER = 'postgres:5432'

print("✅ pgAdmin Development Konfiguration geladen")

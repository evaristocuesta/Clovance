#!/usr/bin/env sh
# Clovance updater for Linux / macOS.
#
# Thin wrapper around install.sh's "update" command so the documented
# one-liner works:
#
#   curl -fsSL https://evaristocuesta.github.io/clovance/install/update.sh | sh
#
# It uses the install.sh sitting next to this file when running from a
# checkout, and downloads the published one otherwise.

set -eu

URL="https://evaristocuesta.github.io/clovance/install/install.sh"

# When this script is piped into a shell, stdin is the script itself, so the
# prompts in install.sh would read EOF and silently keep their defaults.
# Reattach the terminal for the child process only (never for this shell, which
# may still be reading the rest of this file from stdin).
DIR=""
case "$0" in
  sh|dash|bash|ash|ksh|-sh|*/sh) DIR="" ;;
  *) DIR=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd) ;;
esac

if [ -z "$DIR" ] || [ ! -f "$DIR/install.sh" ]; then
  DIR=$(mktemp -d)
  echo "==> Downloading installer..."
  curl -fsSL "$URL" -o "$DIR/install.sh"
  trap 'rm -rf "$DIR"' EXIT INT TERM
fi

if [ ! -t 0 ] && ( : </dev/tty ) 2>/dev/null; then
  exec sh "$DIR/install.sh" update < /dev/tty
fi

exec sh "$DIR/install.sh" update

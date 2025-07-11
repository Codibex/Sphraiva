#!/bin/bash
set -x  # Zeigt alle Befehle im Log an

echo "Starte entrypoint.sh"
echo "GIT_USER_NAME: $GIT_USER_NAME"
echo "GIT_USER_EMAIL: $GIT_USER_EMAIL"
echo "GH_TOKEN: ${GH_TOKEN:0:4}..."  # Zeigt nur die ersten Zeichen aus Sicherheitsgründen

if [ -n "$GIT_USER_NAME" ]; then
  git config --global user.name "$GIT_USER_NAME"
fi
if [ -n "$GIT_USER_EMAIL" ]; then
  git config --global user.email "$GIT_USER_EMAIL"
fi
if [ -n "$GH_TOKEN" ]; then
  echo "$GH_TOKEN" | gh auth login --with-token
fi

echo "Container is ready and running!"

exec "$@"
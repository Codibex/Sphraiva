# Installation

## Host Environment Setup

1. Install Github CLI
2. Authenticate
3. Configure appsettings

    ```json
    "DevContainerSettings": {
        // ...
        "VolumeBinds": [
            "/host/path/.config/gh:/root/.config/gh"
        ]
    }
    ```
  
# Production Setup

## Host Environment Setup

1. Create a GitHub Personal Access Token (PAT) at [GitHub Settings > Developer settings > Personal access tokens](https://github.com/settings/tokens).
2. Grant the following permissions:
   - `repo` (Full access to repository contents, including pull, push, commits, PRs)
   - `workflow` (optional, if you need to run workflows)
3. Copy the token and store it securely in a file.
   - Folder containing the file must mapped into `/agent_data/dockerimages`
4. Configure the `appsettings` in MCP.Server as follows:

    ```json
    "DevContainerSettings": {
      "DataDirectory": "..//agent_data",
      "GitUserName": "Agent Sphraiva",
      "GitUserEmail":  "agent@sphraiva.at",
      "GithubPatTokenFile": "Sphraiva_Github_PAT.txt",
      "DockerImages": [
        {
          "InstructionName": "net9", // Name is used as communication name for the tool
          "ImageName": "agent-dev-net9", // Name of the image
          "Path": "net9", // Folder containing all files for the docker image (DockerFile, (entrypoint.sh optionl, ...))
          "VolumeBinds": [] // Volumes that should be bound to the docker agent
        }
      ]
    }
    ```  
